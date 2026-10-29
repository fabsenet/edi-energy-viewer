using System.Security.Cryptography.X509Certificates;

namespace Fabsenet.EdiEnergyViewer.Hosting;

/// <summary>
/// Selects the Kestrel HTTPS server certificate from the Windows certificate store.
/// Configure on the server, e.g. via environment variable <c>EDIENERGYVIEWER_HttpsCertificate__DnsName</c>.
/// </summary>
public record HttpsCertificateOptions
{
    public const string SectionName = "HttpsCertificate";

    /// <summary>DNS name that must match the certificate CN or a DNS entry in the subject alternative name. Store lookup is disabled when empty.</summary>
    public string? DnsName { get; set; }

    /// <summary>Store name as used by Windows, e.g. <c>My</c> (Personal) or <c>WebHosting</c> (Web Hosting).</summary>
    public string StoreName { get; set; } = "My";

    public StoreLocation StoreLocation { get; set; } = StoreLocation.LocalMachine;

    /// <summary>How often the store is re-scanned, so renewed certificates are picked up without a restart.</summary>
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromHours(1);
}
