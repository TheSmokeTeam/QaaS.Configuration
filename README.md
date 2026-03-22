# QaaS Elastic Bootstrap

`QaaS.Framework.ElasticBootstrap` is an internal package that supplies default Elastic logging values to `QaaS.Framework.Executions` at runtime.

It is intentionally separate from `QaaS.Framework` so the public framework package stays almost unchanged. The public path continues to use the normal command-line flags and logger configuration file behavior. This package is only for environments where you want to provide default Elastic settings from a local, controlled location.

## What this package does

- Registers Elastic logging defaults during application startup.
- Reads those defaults from a local JSON file.
- Does nothing when the JSON file is missing.
- Does not change explicit user settings. In `QaaS.Framework`, explicit flags and logger configuration files still win.

## How it works

The flow is:

1. A consuming application references `QaaS.Framework.ElasticBootstrap`.
2. The package injects a small module initializer into that consuming build through `buildTransitive`.
3. When the application starts, that initializer calls `QaaS.Framework.ElasticBootstrap.Bootstrap.Register()`.
4. `Bootstrap.Register()` looks for a local JSON configuration file.
5. If a configuration file is found, the bootstrap package calls `QaaS.Framework.Executions.ExecutionLogging.RegisterDefaults(...)`.
6. Later, when `QaaS.Framework.Executions` builds the per-run logger, it uses those registered defaults only if the caller did not already provide explicit logging settings.

That means `QaaS.Framework` does **not** automatically discover this package by name from a NuGet source. The framework only becomes aware of it when the consuming application actually references the package and loads the bootstrap assembly at runtime.

## Important limitation

NuGet will not restore this package automatically unless a consuming project references it, or another package in that project's graph depends on it.

That means this package solves the "keep QaaS.Framework minimal and internalize the defaults source" problem, but it does **not** by itself create zero-touch restore behavior for all consumers. If you want completely implicit restore in the air-gapped environment, something in the package graph still has to bring this package in.

## Configuration file locations

The package checks these paths in order and uses the first file that exists:

1. The path in the `QAAS_ELASTIC_BOOTSTRAP_CONFIG_PATH` environment variable.
2. `qaas.elastic.bootstrap.json` next to the application binaries.
3. `%ProgramData%\QaaS\ElasticBootstrap\settings.json`

## Configuration format

```json
{
  "sendLogs": true,
  "elasticUri": "https://your-internal-elastic:9200",
  "elasticUsername": "optional-user",
  "elasticPassword": "optional-password"
}
```

Notes:

- Set `sendLogs` to `true` to enable the existing Elastic sink path.
- Leave `elasticUsername` and `elasticPassword` empty if your internal Elastic endpoint does not use basic auth.
- Do not put classified values in this repository. Put them only in the local JSON file inside the air-gapped environment.

## Usage

Reference the package from the application or package graph that eventually loads `QaaS.Framework.Executions`.

Example:

```xml
<ItemGroup>
  <PackageReference Include="QaaS.Framework.ElasticBootstrap" Version="1.0.0" />
</ItemGroup>
```

Then place the JSON file in one of the supported locations.

## Changing the defaults

You do not need to rebuild the package to change the Elastic values.

Change the JSON file in the chosen local path and restart the application. The package reads the file during startup.

## Build

```powershell
dotnet build .\QaaS.ElasticBootstrap.sln -c Release
dotnet pack .\QaaS.Framework.ElasticBootstrap\QaaS.Framework.ElasticBootstrap.csproj -c Release
```

This repository intentionally does not include CI workflows.
