using System.Runtime.CompilerServices;

namespace QaaS.Framework.ElasticBootstrap;

internal static class QaaSFrameworkElasticBootstrapModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize() => Bootstrap.Register();
}
