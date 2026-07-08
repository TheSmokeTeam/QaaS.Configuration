using System.Reflection;

namespace QaaS.Configuration;

/// <summary>
/// Registers the package's configuration defaults when the consuming app starts.
/// </summary>
public static class ConfigurationBootstrap
{
    private const string ExecutionLoggingAssemblyName = "QaaS.Framework.Executions";
    private const string ExecutionLoggingTypeName = "QaaS.Framework.Executions.ExecutionLogging";
    private const string ReportPortalConfigAssemblyName = "QaaS.Runner.Assertions";
    private const string ReportPortalConfigTypeName =
        "QaaS.Runner.Assertions.ConfigurationObjects.ReporterConfigs.ReportPortalConfig";

    private static readonly object SyncRoot = new();
    private static bool _elasticRegistered;
    private static bool _reportPortalRegistered;

    /// <summary>
    /// Attempts to register the package defaults once.
    /// </summary>
    public static void Register()
    {
        lock (SyncRoot)
        {
            if (!_elasticRegistered)
            {
                _elasticRegistered = TryRegisterElasticDefaults();
            }

            if (!_reportPortalRegistered)
            {
                _reportPortalRegistered = TryRegisterReportPortalDefaults();
            }
        }
    }

    private static bool TryRegisterElasticDefaults()
    {
        try
        {
            var executionLoggingType = ResolveType(
                ExecutionLoggingAssemblyName,
                ExecutionLoggingTypeName
            );
            var registerDefaultsMethod = executionLoggingType?.GetMethod(
                "RegisterDefaults",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: [typeof(bool), typeof(string), typeof(string), typeof(string)],
                modifiers: null
            );

            if (registerDefaultsMethod is null)
            {
                return false;
            }

            registerDefaultsMethod.Invoke(
                null,
                [
                    ElasticDefaults.SendLogs,
                    ElasticDefaults.ElasticUri,
                    ElasticDefaults.ElasticUsername,
                    ElasticDefaults.ElasticPassword,
                ]
            );

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryRegisterReportPortalDefaults()
    {
        try
        {
            var reportPortalConfigType = ResolveType(
                ReportPortalConfigAssemblyName,
                ReportPortalConfigTypeName
            );
            var registerDefaultsMethod = reportPortalConfigType?.GetMethod(
                "RegisterDefaults",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: [typeof(bool), typeof(string), typeof(string)],
                modifiers: null
            );

            if (registerDefaultsMethod is null)
            {
                return false;
            }

            registerDefaultsMethod.Invoke(
                null,
                [
                    ReportPortalDefaults.Enabled,
                    ReportPortalDefaults.ReportPortalUri,
                    ReportPortalDefaults.ReportPortalApiKey,
                ]
            );

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static Type? ResolveType(string assemblyName, string typeName) =>
        Type.GetType($"{typeName}, {assemblyName}", throwOnError: false)
        ?? AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly => assembly.GetName().Name == assemblyName)
            ?.GetType(typeName, throwOnError: false);
}
