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
   reg add HKLM\SYSTEM\CurrentControlSet\Services\EdiEnergyViewer /v Environment /t REG_MULTI_SZ /d "DOTNET_ENVIRONMENT=Production"
   sc.exe start EdiEnergyViewer
   ```

   `DOTNET_ENVIRONMENT` must be set explicitly, otherwise the app falls back to `development`.

By default Kestrel listens on `http://*:442` (see the `Kestrel` section in `appsettings.json`). Override it per server, e.g. with `EDIENERGYVIEWER_Kestrel__Endpoints__Http__Url`.

The health endpoint `GET /alive` checks the RavenDB connection and returns a JSON report (HTTP 200 when healthy, 503 otherwise).