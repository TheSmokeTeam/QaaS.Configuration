using System.Runtime.CompilerServices;

namespace QaaS.ElasticConfiguration;

internal static class QaaSElasticConfigurationModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize() => Bootstrap.Register();
}
