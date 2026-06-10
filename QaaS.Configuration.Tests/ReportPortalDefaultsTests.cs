using NUnit.Framework;
using QaaS.Configuration;
using QaaS.Configuration.Tools;

namespace QaaS.Configuration.Tests;

[TestFixture]
public sealed class ReportPortalDefaultsTests
{
    [Test]
    public void Enabled_DefaultsToFalse()
    {
        Assert.That(ReportPortalDefaults.Enabled, Is.False);
    }

    [Test]
    public void ReportPortalUri_CanBeConfiguredThroughGeneratedDefaults()
    {
        var source = DefaultsSourceRenderer.RenderReportPortalDefaultsFile(
            reportPortalEnabled: true,
            reportPortalUri: "https://reportportal.example",
            reportPortalApiKey: null);

        Assert.That(source, Does.Contain("public static string? ReportPortalUri => \"https://reportportal.example\";"));
    }

    [Test]
    public void ReportPortalApiKey_CanBeConfiguredThroughGeneratedDefaults()
    {
        var source = DefaultsSourceRenderer.RenderReportPortalDefaultsFile(
            reportPortalEnabled: true,
            reportPortalUri: null,
            reportPortalApiKey: "secret-api-key");

        Assert.That(source, Does.Contain("public static string? ReportPortalApiKey => \"secret-api-key\";"));
    }
}
