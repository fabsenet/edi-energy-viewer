edi-energy-viewer
=================

A website displaying the data from the [edi-energy] website in a more user-friendly way.

[edi-energy]: https://www.edi-energy.de

Hosting as a Windows service
----------------------------

The server runs self-hosted with Kestrel as a Windows service (no IIS required).

1. Publish: `dotnet publish EdiEnergyViewer.Server/EdiEnergyViewer.Server.csproj -c Release -o output --no-self-contained --runtime win-x64`
2. Copy the `output` folder to the server, e.g. `C:\EdiEnergyViewer`.
3. Configure the database (`DatabaseUrl`, `DatabaseName`, optional `DatabaseCertificate`) via `appsettings.{Environment}.json` or environment variables prefixed with `EDIENERGYVIEWER_`.
4. Create and start the service (elevated prompt):

   ```cmd
   sc.exe create EdiEnergyViewer binPath= "C:\EdiEnergyViewer\EdiEnergyViewer.Server.exe" start= delayed-auto
   reg add HKLM\SYSTEM\CurrentControlSet\Services\EdiEnergyViewer /v Environment /t REG_MULTI_SZ /d "DOTNET_ENVIRONMENT=Production\0EDIENERGYVIEWER_HttpsCertificate__DnsName=<your.server.fqdn>"
   sc.exe start EdiEnergyViewer
   ```

   `DOTNET_ENVIRONMENT` must be set explicitly, otherwise the app falls back to `development`.

By default Kestrel listens on `https://*:442` (see the `Kestrel` section in `appsettings.json`). Override it per server, e.g. with `EDIENERGYVIEWER_Kestrel__Endpoints__Https__Url`.

### HTTPS certificate

The server certificate is picked from the Windows certificate store, so a renewed certificate is used without config changes or a restart:

| Setting (env var `EDIENERGYVIEWER_HttpsCertificate__...`) | Default | Description |
|-----|-----|-----|
| `DnsName` | – (required) | Certificate must have this name as CN or as DNS entry in the subject alternative name |
| `StoreLocation` | `LocalMachine` | Certificate store location |
| `StoreName` | `My` | Certificate store name, e.g. `My` (Personal) or `WebHosting` (Web Hosting) |
| `RefreshInterval` | `01:00:00` | How often the store is re-scanned |

Of all matching certificates (currently valid, private key present, server authentication usage) the one expiring last is used. The service fails to start if none is found. The service account must have read access to the private key (given for `LocalSystem`).

The health endpoint `GET /alive` checks the RavenDB connection and returns a JSON report (HTTP 200 when healthy, 503 otherwise).