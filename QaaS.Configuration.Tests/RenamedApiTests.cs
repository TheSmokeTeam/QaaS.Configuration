using NUnit.Framework;
using QaaS.Configuration;

namespace QaaS.Configuration.Tests;

[TestFixture]
public sealed class RenamedApiTests
{
    [Test]
    public void RenamedConfigurationTypes_AreAvailable()
    {
        Assert.Multiple(() =>
        {
            Assert.That(typeof(ConfigurationBootstrap).Namespace, Is.EqualTo("QaaS.Configuration"));
            Assert.That(typeof(ElasticDefaults).Namespace, Is.EqualTo("QaaS.Configuration"));
            Assert.That(typeof(ReportPortalDefaults).Namespace, Is.EqualTo("QaaS.Configuration"));
        });
    }
}
