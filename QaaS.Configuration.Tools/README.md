# QaaS.Configuration.Tools

`QaaS.Configuration.Tools` rebuilds the internal QaaS configuration package with environment-specific fallback defaults.

Repository path:

- `QaaS.Configuration.Tools`

## Command

```powershell
dotnet run --project .\QaaS.Configuration.Tools\QaaS.Configuration.Tools.csproj -- `
  --package-version 1.0.0 `
  --send-logs true `
  --elastic-uri http://your-internal-elastic:9200 `
  --reportportal-enabled true `
  --reportportal-uri https://your-internal-reportportal `
  --reportportal-api-key <key>
```

Optional push arguments:

- `--push-to-artifactory true`
- `--artifactory-source <url>`
- `--artifactory-api-key <key>`

## What it does

- copies the repository into a disposable staging area so the public source tree is never mutated in place
- rewrites `ElasticDefaults.cs` with the requested internal Elastic defaults
- rewrites `ReportPortalDefaults.cs` with the requested internal ReportPortal defaults
- packs the NuGet package with the requested version
- optionally pushes both package artifacts to Artifactory

## Documentation contract

- The README documents the packaging flow and the operator-facing arguments.
- The command entrypoint and helper functions carry XML documentation comments so the tool is understandable without the old script.
