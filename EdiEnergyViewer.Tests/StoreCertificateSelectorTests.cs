using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Fabsenet.EdiEnergyViewer.Hosting;

namespace EdiEnergyViewer.Tests;

/// <summary>
/// Unit tests for the HTTPS certificate selection from the certificate store.
/// </summary>
public class StoreCertificateSelectorTests
{
    private const string DnsName = "server.example.test";
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    [Fact]
    public void IsMatch_CommonNameMatches_ReturnsTrue()
    {
        using var cert = CreateCertificate($"CN={DnsName.ToUpperInvariant()}");

        Assert.True(StoreCertificateSelector.IsMatch(cert, DnsName, Now));
    }

    [Fact]
    public void IsMatch_SubjectAlternativeNameMatches_ReturnsTrue()
    {
        using var cert = CreateCertificate("CN=other.example.test", sanDnsNames: ["other.example.test", DnsName]);

        Assert.True(StoreCertificateSelector.IsMatch(cert, DnsName, Now));
    }

    [Fact]
    public void IsMatch_NoNameMatches_ReturnsFalse()
    {
        using var cert = CreateCertificate("CN=other.example.test", sanDnsNames: ["other.example.test"]);

        Assert.False(StoreCertificateSelector.IsMatch(cert, DnsName, Now));
    }

    [Fact]
    public void IsMatch_Expired_ReturnsFalse()
    {
        using var cert = CreateCertificate($"CN={DnsName}", notBefore: Now.AddDays(-30), notAfter: Now.AddDays(-1));

        Assert.False(StoreCertificateSelector.IsMatch(cert, DnsName, Now));
    }

    [Fact]
    public void IsMatch_NotYetValid_ReturnsFalse()
    {
        using var cert = CreateCertificate($"CN={DnsName}", notBefore: Now.AddDays(1), notAfter: Now.AddDays(30));

        Assert.False(StoreCertificateSelector.IsMatch(cert, DnsName, Now));
    }

    [Fact]
    public void IsMatch_WithoutPrivateKey_ReturnsFalse()
    {
        using var certWithKey = CreateCertificate($"CN={DnsName}");
        using var cert = X509CertificateLoader.LoadCertificate(certWithKey.RawData);

        Assert.False(StoreCertificateSelector.IsMatch(cert, DnsName, Now));
    }

    [Fact]
    public void IsMatch_ClientAuthenticationOnly_ReturnsFalse()
    {
        using var cert = CreateCertificate($"CN={DnsName}", ekuOid: "1.3.6.1.5.5.7.3.2");

        Assert.False(StoreCertificateSelector.IsMatch(cert, DnsName, Now));
    }

    [Fact]
    public void SelectBest_PicksValidCertificateWithLatestExpiry()
    {
        using var expired = CreateCertificate($"CN={DnsName}", notBefore: Now.AddDays(-400), notAfter: Now.AddDays(-1));
        using var older = CreateCertificate($"CN={DnsName}", notAfter: Now.AddDays(10));
        using var renewed = CreateCertificate($"CN={DnsName}", notAfter: Now.AddDays(365));
        using var otherHost = CreateCertificate("CN=other.example.test", notAfter: Now.AddDays(800));

        var selected = StoreCertificateSelector.SelectBest([expired, older, renewed, otherHost], DnsName, Now);

        Assert.Same(renewed, selected);
    }

    [Fact]
    public void SelectBest_NoMatch_ReturnsNull()
    {
        using var otherHost = CreateCertificate("CN=other.example.test");

        Assert.Null(StoreCertificateSelector.SelectBest([otherHost], DnsName, Now));
    }

    private static X509Certificate2 CreateCertificate(
        string subject,
        string[]? sanDnsNames = null,
        DateTimeOffset? notBefore = null,
        DateTimeOffset? notAfter = null,
        string ekuOid = "1.3.6.1.5.5.7.3.1")
    {
        using var key = RSA.Create(2048);
        var request = new CertificateRequest(subject, key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([new Oid(ekuOid)], false));
        if (sanDnsNames != null)
        {
            var san = new SubjectAlternativeNameBuilder();
            foreach (var name in sanDnsNames)
            {
                san.AddDnsName(name);
            }
            request.CertificateExtensions.Add(san.Build());
        }

        return request.CreateSelfSigned(notBefore ?? Now.AddDays(-1), notAfter ?? Now.AddDays(30));
    }
}
