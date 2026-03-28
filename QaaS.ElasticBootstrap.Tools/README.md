# QaaS.ElasticBootstrap.Tools

`QaaS.ElasticBootstrap.Tools` replaces the old internal PowerShell packaging script with a documented C# CLI.

Repository path:

- `QaaS.ElasticBootstrap.Tools`

## Command

```powershell
dotnet run --project .\QaaS.ElasticBootstrap.Tools\QaaS.ElasticBootstrap.Tools.csproj -- `
  --package-version 1.0.0 `
  --send-logs true `
  --elastic-uri http://your-internal-elastic:9200
```

Optional push arguments:

- `--push-to-artifactory true`
- `--artifactory-source <url>`
- `--artifactory-api-key <key>`

## What it does

- copies the repository into a disposable staging area so the public source tree is never mutated in place
- rewrites `ElasticBootstrapDefaults.cs` with the requested internal defaults
- packs the NuGet package with the requested version
- optionally pushes both package artifacts to Artifactory

## Documentation contract

- The README documents the packaging flow and the operator-facing arguments.
- The command entrypoint and helper functions carry XML documentation comments so the replacement is understandable without the removed script.
