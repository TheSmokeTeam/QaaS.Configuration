# AGENTS.md — QaaS.Configuration

Guidance for AI agents working in this repository.

## What this repo is

A **buildTransitive NuGet package** that injects fallback configuration defaults (Elastic, ReportPortal) into consuming QaaS applications. It is deliberately separate from QaaS.Framework. Mechanism:

1. Consumers reference the `QaaS.Configuration` package.
2. `buildTransitive/QaaS.Configuration.targets` runs **BeforeTargets="CoreCompile"** in every consuming project: copies `QaaS.Configuration.g.cs` into `$(IntermediateOutputPath)` and adds it as a Compile item (guarded by `QaaSConfigurationInjected`).
3. That file is a C# `[ModuleInitializer]` calling `ConfigurationBootstrap.Register()` on assembly load.
4. `Register()` (thread-safe, idempotent) uses **reflection** to invoke, when present:
   - `QaaS.Framework.Executions.ExecutionLogging.RegisterDefaults(bool, string?, string?, string?)` (Elastic)
   - `QaaS.Runner.Assertions.ConfigurationObjects.ReporterConfigs.ReportPortalConfig.RegisterDefaults(bool, string?, string?)` (ReportPortal)
5. Frameworks use these defaults ONLY when the user supplied no explicit values.

## Projects (net10.0)

| Project | Purpose |
|---|---|
| QaaS.Configuration | the package: `ConfigurationBootstrap.cs`, `ElasticDefaults.cs`, `ReportPortalDefaults.cs`, `buildTransitive/**` |
| QaaS.Configuration.Tests | NUnit 4.5.1 tests (`RenamedApiTests`, `ReportPortalDefaultsTests`) |
| QaaS.Configuration.Tools | CLI that **rebuilds the internal/air-gapped variant** with custom defaults (`Program.cs`, `DefaultsSourceRenderer.cs`) |

## Build & test

```powershell
dotnet build -m --no-restore        # after one dotnet restore
dotnet test --no-build
dotnet pack QaaS.Configuration/QaaS.Configuration.csproj -c Release -o <feed>
```

## Critical gotchas

- **Air-gap rebuild keeps SAME package ID + version** with different default values. The NuGet global-packages cache will happily serve the stale binary — after any variant swap run `dotnet nuget locals all --clear` (or delete the package folder) before judging behavior.
- The public package ships **null/false defaults** — behavior changes come from the Tools-rebuilt internal variant.
- Reflection target signatures (`RegisterDefaults(...)`) are a **hidden contract with QaaS.Framework / QaaS.Runner**: renaming those types/methods there breaks injection silently (reflection uses `throwOnError: false`). Any change here must be validated against a consuming fixture, not unit tests alone.
- Changes to `buildTransitive/*.targets` affect EVERY consuming project's build — test through an actual consumer (`dotnet new qaas-runner` sandbox) before shipping.
- Tier-0 repo: ripples into Common.*, Runner, Mocker, templates. Check the dependency tiers before changing public surface.

## Process

Non-trivial changes follow the QaaS harness pipeline (plan → contract → implement → adversarial evaluation; rubric: correctness, completeness, craft, robustness — each ≥7/10). Write failing NUnit tests first; never mock the class under test; prove package behavior through a consuming fixture. Keep commits small with conventional-commit messages (`fix:`, `feat:`, `chore(release):`).
