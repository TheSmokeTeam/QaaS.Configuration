# Copilot instructions — QaaS.Configuration

Read `AGENTS.md` at the repo root first — it explains the buildTransitive module-initializer injection mechanism, the air-gapped same-ID/same-version rebuild workflow, and the reflection contract with QaaS.Framework/QaaS.Runner.

Essentials:
- net10.0; NUnit 4.5.1 tests; build `dotnet build -m`, test `dotnet test --no-build`.
- `buildTransitive/QaaS.Configuration.targets` injects a `[ModuleInitializer]` into every consumer → changes here must be verified through a real consuming project, not just unit tests.
- Air-gap variant is rebuilt with the SAME package id+version — always clear the NuGet cache (`dotnet nuget locals all --clear`) when swapping variants.
- Reflection targets `ExecutionLogging.RegisterDefaults` / `ReportPortalConfig.RegisterDefaults` are a hidden cross-repo contract; signature changes break silently.
- Conventional commits; small, story-scoped changes; tests first.
