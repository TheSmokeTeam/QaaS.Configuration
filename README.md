# QaaS Elastic Bootstrap

`QaaS.ElasticConfiguration` is a small NuGet package that supplies fallback Elastic logging defaults to `QaaS.Framework.Executions`.

The package is meant to stay separate from `QaaS.Framework`. The framework keeps its normal flag-driven behavior, and this package only fills in defaults when a run did not already provide explicit Elastic settings.

## How it works

1. `QaaS.Framework.Executions` declares a NuGet dependency on `QaaS.ElasticConfiguration` version `1.0.0`.
2. When a consuming app restores `QaaS.Framework.Executions`, NuGet also restores `QaaS.ElasticConfiguration`.
3. This package injects a small module initializer into the consuming build through `buildTransitive`.
4. On application startup, that initializer calls `QaaS.ElasticConfiguration.Bootstrap.Register()`.
5. `Bootstrap.Register()` calls `QaaS.Framework.Executions.ExecutionLogging.RegisterDefaults(...)` by reflection.
6. Later, `QaaS.Framework.Executions` uses those defaults only when the run did not already specify `send-logs`, `elastic-uri`, `elastic-username`, `elastic-password`, or a logger configuration file.

The existing Elastic sink behavior in `QaaS.Framework` is unchanged. This package only provides fallback values.

## Where to change the defaults

The built-in values live in:

`QaaS.ElasticConfiguration/ElasticConfigurationDefaults.cs`

Public/default package values should stay:

- `SendLogs = false`
- `ElasticUri = null`
- `ElasticUsername = null`
- `ElasticPassword = null`

For the air-gapped variant, edit that file and rebuild the package with the same package ID and the same version.

## Public package behavior

The public package should be published as:

- Package ID: `QaaS.ElasticConfiguration`
- Version: `1.0.0`
- Built-in defaults: disabled / null values

That version is safe to publish publicly because it does not contain any classified endpoint or credentials.

## Air-gapped package behavior

Inside the air-gapped environment, publish another package with:

- the same package ID: `QaaS.ElasticConfiguration`
- the same version: `1.0.0`
- different values in `ElasticConfigurationDefaults.cs`

Your Artifactory virtual NuGet source must resolve the air-gapped bootstrap repo before the mirrored public repo. That way the virtual feed serves the internal `1.0.0` package, not the public `1.0.0` package.

Do not rely on multiple client-side NuGet sources that both expose `QaaS.ElasticConfiguration` `1.0.0`. In local validation, NuGet preferred the package from the same source as `QaaS.Framework.Executions`. The override needs to happen server-side in the virtual feed that the client restores from.

Example air-gapped defaults:

```csharp
public static class ElasticConfigurationDefaults
{
    public static bool SendLogs => true;
    public static string? ElasticUri => "http://your-internal-elastic:9200";
    public static string? ElasticUsername => null;
    public static string? ElasticPassword => null;
}
```

## Important limitation

This design only works if the bootstrap package version required by `QaaS.Framework.Executions` is available on the restore sources used to build and consume the framework package.

Because the framework package now depends on `QaaS.ElasticConfiguration`, you must publish the public `1.0.0` package before publishing a framework version that references it.

## Build

Build:

```powershell
dotnet build .\QaaS.ElasticConfiguration.sln -c Release
```

Pack version `1.0.0`:

```powershell
dotnet pack .\QaaS.ElasticConfiguration\QaaS.ElasticConfiguration.csproj `
  -c Release `
  -p:PackageVersion=1.0.0 `
  -p:Version=1.0.0
```

Example push to a NuGet source:

```powershell
dotnet nuget push .\QaaS.ElasticConfiguration\bin\Release\QaaS.ElasticConfiguration.1.0.0.nupkg `
  --source <your-source-name> `
  --skip-duplicate
```

This repository intentionally does not include CI workflows.
