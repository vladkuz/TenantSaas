# TenantSaas

## Setup

### Prerequisites

- .NET SDK 10.0.102 (LTS)

### Initialize the repository

Run the exact .NET SDK template commands below from the repo root:

```bash
dotnet new sln -n TenantSaas

dotnet new classlib -n TenantSaas.Core -f net10.0
dotnet new classlib -n TenantSaas.EfCore -f net10.0
dotnet new xunit -n TenantSaas.ContractTests -f net10.0
dotnet new webapi -n TenantSaas.Sample -f net10.0

dotnet sln TenantSaas.sln add \
  TenantSaas.Core/TenantSaas.Core.csproj \
  TenantSaas.EfCore/TenantSaas.EfCore.csproj \
  TenantSaas.ContractTests/TenantSaas.ContractTests.csproj \
  TenantSaas.Sample/TenantSaas.Sample.csproj
```

### Build and test

```bash
dotnet build TenantSaas.sln
dotnet test TenantSaas.sln
```

### Troubleshooting missing templates or dependencies

If initialization fails because a template or framework is missing, use the guidance below.

Common errors and fixes:

- Error: `net10.0 is not a valid value for -f`.
  - Fix: Install .NET SDK 10.0.102 and ensure `dotnet --version` returns `10.0.102`.
  - Temporary fallback (if you cannot upgrade immediately): run the template without `-f net10.0`, then set `TargetFramework` to `net10.0` in each project and `Directory.Build.props`.
- Error: `No templates found matching: 'webapi'` (or `classlib`, `xunit`).
  - Fix: Reinstall the .NET SDK or install the templates with:
    ```bash
    dotnet new --install Microsoft.DotNet.Common.ProjectTemplates.10.0
    ```
- Error: `Unable to load the service index for source https://api.nuget.org/v3/index.json`.
  - Fix: Check network access/proxy settings, then run:
    ```bash
    dotnet restore
    ```

If you must run with a different SDK patch version temporarily, update `global.json` and then restore. Revert to 10.0.102 once installed.
