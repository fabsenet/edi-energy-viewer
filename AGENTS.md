# EDI Energy Viewer — Agent Instructions

A web application that displays EDI (Electronic Data Interchange) energy documents from [edi-energy.de](https://www.edi-energy.de) in a user-friendly way. Documents (PDFs, XMLs) and their metadata are stored in RavenDB by a separate scraper/export tool; this repo is the read-only viewer.

## Architecture

**Two-project solution:**

- `EdiEnergyViewer.Server` — ASP.NET Core (.NET 10) REST API + static file host
- `edienergyviewer.client` — Vue 3 + TypeScript SPA (Vite, Vuetify 4)

**Data store: RavenDB**

- `EdiDocument` records store metadata + `CheckIdentifier: Dictionary<int, List<int>>` (maps a check-identifier number → list of PDF page numbers)
- `EdiXmlDocument` records store XML schema document metadata
- PDFs are stored as RavenDB attachments named `"pdf"` on each `EdiDocument`; XML files as `"xml"` on each `EdiXmlDocument`
- Extracted PDF sub-parts are cached as attachments named `"pdf-{checkIdentifier}"` (generated on first request via iTextSharp, then served from cache)
- RavenDB indexes live in `Util/` and are auto-registered on startup via `IndexCreation.CreateIndexes(Assembly.GetExecutingAssembly(), store)`

**SPA integration:**

- In development, Vite dev server runs on port 53000 and proxies `/api/`, `/openapi`, `/swagger` to the ASP.NET backend (port 53735)
- In production, the Vue app is built during `dotnet publish` (via the `.esproj` project reference) and served as static files from ASP.NET

**OpenAPI / type generation:**

- ASP.NET generates an OpenAPI spec named `EdiDocumentsOpenApi` (see `<OpenApiGenerateDocumentsOptions>` in the `.csproj`)
- `src/api/EdiDocsApi.d.ts` is the TypeScript type file generated from that spec via `openapi-typescript`
- `src/api/EdiDocsClient.ts` creates a type-safe `openapi-fetch` client from those types — all API calls go through this client

## Build & Run Commands

### Backend

```bash
# Run (also starts the Vite dev proxy automatically via SpaProxy)
dotnet run --project EdiEnergyViewer.Server

# Production publish (win-x64)
dotnet publish EdiEnergyViewer.Server/EdiEnergyViewer.Server.csproj -c Release -o output --no-self-contained --runtime win-x64

# Verify C# code style (no changes allowed — matches CI)
dotnet format EdiEnergyViewer.Server/EdiEnergyViewer.Server.csproj -v d --verify-no-changes --no-restore
```

### Frontend (run from `edienergyviewer.client/`)

```bash
npm run dev          # Vite dev server (proxies to backend)
npm run build        # Type-check + production build
npm run type-check   # vue-tsc only
npm run lint         # ESLint --fix
```

There are no automated tests in this repository.

## Configuration

Backend reads config from `appsettings.json` and environment variables prefixed with `EDIENERGYVIEWER_`:

| Key | Description |
|-----|-------------|
| `DatabaseUrl` | RavenDB server URL (e.g. `http://127.0.0.1:8080`) |
| `DatabaseName` | RavenDB database name |
| `DatabaseCertificate` | Path to a PKCS#12 `.pfx` cert file (optional, for secured RavenDB) |

`appsettings.Development.json` defaults to `http://127.0.0.1:8080` / `EdiDocsDev`.

## Key Conventions

### C# (backend)

- Namespace: `Fabsenet.EdiEnergyViewer`; file-scoped namespaces throughout
- **Records** for all data models (`EdiDocument`, `EdiDocumentSlim`, `FilterData`, etc.)
- **Primary constructors** for controllers (injected `IDocumentStore` and `ILogger`)
- `var` everywhere (enforced by `.editorconfig`)
- Instance fields: `_camelCase`; constants and public members: `PascalCase`
- RavenDB document IDs always follow the pattern `EdiDocuments/{numericId}` — controllers prepend this prefix when receiving a bare numeric ID from the URL
- All three controllers share `[ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]`
- Logging via NLog (configured in `nlog.config`); use `ILogger<T>` for structured logging with named parameters

### TypeScript / Vue (frontend)

- All HTTP calls use the typed `EdiDocsClient` from `src/api/EdiDocsClient.ts` — never use raw `fetch`
- When `EdiDocsApi.d.ts` needs to be regenerated after backend API changes, run `npx openapi-typescript` against the OpenAPI spec
- Filter state is persisted to `localStorage` under the key `edidocs_filter`
- UI locale is German (`de`) — user-facing strings should be in German
- Vuetify 4 components are globally registered; use `v-*` components rather than building custom UI primitives
- The ESLint config allows Vuetify slot modifier syntax (`#item.columnName`) via `vue/valid-v-slot: allowModifiers: true`
