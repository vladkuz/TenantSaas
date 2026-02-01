# TenantSaas

## Prerequisites

- .NET SDK 10.0.102 (per `global.json`)
  - Verify with: `dotnet --version`
- Optional tooling:
  - Git (for cloning)
  - curl (for health check verification)
  - IDE (Visual Studio, Rider, or VS Code)

## Initialize the Repository (Template)

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

## Local Setup

1. Clone the repository:
   ```bash
   git clone <repo-url>
   cd TenantSaas
   ```
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Build the solution:
   ```bash
   dotnet build TenantSaas.sln
   ```
4. Run the sample host:
   ```bash
   dotnet run --project TenantSaas.Sample/TenantSaas.Sample.csproj
   ```

The host prints a listening URL (http or https) in the console output. Use that URL in the verification step.

To run a full build + test pass:

```bash
dotnet build TenantSaas.sln
dotnet test TenantSaas.sln
```

## CI/CD

GitHub Actions runs a CI workflow on every pull request and on pushes to `main`. The workflow restores dependencies, builds the solution, and runs the test suite. CI will fail if any step exits with a non-zero code.

Common CI failure scenarios:

- SDK version mismatch (global.json vs. installed SDK)
- Build errors (compilation failures)
- Test failures (failing contract or integration tests)
- Restore failures (NuGet connectivity or package issues)
- Workflow misconfiguration (missing steps or invalid YAML)

View results in the GitHub Actions tab for the repository.

## Verification

With the sample host running, call the health endpoint:

```bash
curl -L http://localhost:<port>/health
```

Expected response:

```json
{"status":"healthy"}
```

If you use the HTTPS URL, ensure your dev certificate is trusted:

```bash
dotnet dev-certs https --trust
```

## Troubleshooting

Common failures and fixes:

### Troubleshooting missing templates or dependencies

- Missing .NET SDK 10.0.102
  - Symptom: `The specified SDK version [10.0.102] could not be found.`
  - Fix: Install the exact SDK and confirm with `dotnet --list-sdks`.
- Restore failures from NuGet
  - Symptom: `Unable to load the service index for source https://api.nuget.org/v3/index.json`.
  - Fix: Verify proxy/network access, then rerun `dotnet restore`.
- Missing templates or frameworks
  - Symptom: `net10.0 is not a valid value for -f`.
  - Fix: Install .NET SDK 10.0.102 and ensure `dotnet --version` returns `10.0.102`.
  - Temporary fallback (if you cannot upgrade immediately): run the template without `-f net10.0`, then set `TargetFramework` to `net10.0` in each project and `Directory.Build.props`.
  - Symptom: `No templates found matching: 'webapi'` (or `classlib`, `xunit`).
  - Fix: Reinstall the .NET SDK or install the templates with:
    ```bash
    dotnet new --install Microsoft.DotNet.Common.ProjectTemplates.10.0
    ```
- HTTPS dev certificate not trusted
  - Symptom: browser or curl TLS errors on https URL.
  - Fix: run `dotnet dev-certs https --trust` or use the http URL printed on startup.
