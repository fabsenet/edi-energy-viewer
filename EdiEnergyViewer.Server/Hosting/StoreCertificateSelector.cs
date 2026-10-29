using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;

namespace Fabsenet.EdiEnergyViewer.Hosting;

/// <summary>
/// Picks the best matching server certificate from the certificate store and periodically re-scans the store,
/// so a rotated certificate is used without configuration changes or a restart.
/// </summary>
public sealed class StoreCertificateSelector(IOptions<HttpsCertificateOptions> options, TimeProvider timeProvider, ILogger<StoreCertificateSelector> logger)
{
    private const string ServerAuthenticationOid = "1.3.6.1.5.5.7.3.1";

    private readonly HttpsCertificateOptions _options = options.Value;
    private readonly Lock _lock = new();
    private X509Certificate2? _current;
    private DateTimeOffset _nextRefresh = DateTimeOffset.MinValue;

    public X509Certificate2? GetCertificate()
    {
        var now = timeProvider.GetUtcNow();
        var current = _current;
        if (current != null && now < _nextRefresh && now < current.NotAfter)
        {
            return current;
        }

        lock (_lock)
        {
            if (_current == null || now >= _nextRefresh || now >= _current.NotAfter)
            {
                Refresh(now);
            }
            return _current;
        }
    }

    private void Refresh(DateTimeOffset now)
    {
        _nextRefresh = now + _options.RefreshInterval;

        X509Certificate2? selected;
        try
        {
            selected = LoadFromStore(now);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read certificate store {StoreLocation}\\{StoreName}", _options.StoreLocation, _options.StoreName);
            return;
        }

        if (selected == null)
        {
            logger.LogError("No valid HTTPS certificate for {DnsName} found in {StoreLocation}\\{StoreName}", _options.DnsName, _options.StoreLocation, _options.StoreName);
            return;
        }

        if (_current?.Thumbprint == selected.Thumbprint)
        {
            selected.Dispose();
            return;
        }

        // the previous certificate is not disposed, as it may still be in use by running TLS handshakes
        _current = selected;
        logger.LogInformation("Using HTTPS certificate {Subject} ({Thumbprint}), valid until {NotAfter}", selected.Subject, selected.Thumbprint, selected.NotAfter);
    }

    private X509Certificate2? LoadFromStore(DateTimeOffset now)
    {
        using var store = new X509Store(_options.StoreName, _options.StoreLocation);
        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

        var certificates = store.Certificates;
        var selected = SelectBest(certificates, _options.DnsName!, now);
        foreach (var certificate in certificates)
        {
            if (!ReferenceEquals(certificate, selected))
            {
                certificate.Dispose();
            }
        }
        return selected;
    }

    public static X509Certificate2? SelectBest(IEnumerable<X509Certificate2> certificates, string dnsName, DateTimeOffset now)
    {
        return certificates
            .Where(c => IsMatch(c, dnsName, now))
            .OrderByDescending(c => c.NotAfter)
            .ThenByDescending(c => c.NotBefore)
            .FirstOrDefault();
    }

    public static bool IsMatch(X509Certificate2 certificate, string dnsName, DateTimeOffset now)
    {
        var utcNow = now.UtcDateTime;
        if (utcNow < certificate.NotBefore.ToUniversalTime() || utcNow >= certificate.NotAfter.ToUniversalTime())
        {
            return false;
        }

        if (!certificate.HasPrivateKey)
        {
            return false;
        }

        var eku = certificate.Extensions.OfType<X509EnhancedKeyUsageExtension>().FirstOrDefault();
        if (eku != null && !eku.EnhancedKeyUsages.Cast<System.Security.Cryptography.Oid>().Any(o => o.Value == ServerAuthenticationOid))
        {
            return false;
        }

        if (string.Equals(certificate.GetNameInfo(X509NameType.SimpleName, false), dnsName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var san = certificate.Extensions.OfType<X509SubjectAlternativeNameExtension>().FirstOrDefault();
        return san != null && san.EnumerateDnsNames().Any(n => string.Equals(n, dnsName, StringComparison.OrdinalIgnoreCase));
    }
}
