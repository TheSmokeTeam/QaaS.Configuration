using System.Reflection;

namespace QaaS.Configuration;

/// <summary>
/// Registers the package's configuration defaults when the consuming app starts.
/// </summary>
public static class ConfigurationBootstrap
{
    private static readonly object SyncRoot = new();
    private static bool _registered;

    /// <summary>
    /// Attempts to register the package defaults once.
    /// </summary>
    public static void Register()
    {
        lock (SyncRoot)
        {
            if (_registered)
            {
                return;
            }

            _registered = TryRegisterDefaults();
        }
    }

    private static bool TryRegisterDefaults()
    {
        var elasticRegistered = TryRegisterElasticDefaults();
        var reportPortalRegistered = TryRegisterReportPortalDefaults();

        return elasticRegistered || reportPortalRegistered;
    }

    private static bool TryRegisterElasticDefaults()
    {
        try
        {
            var executionLoggingType = Type.GetType(
                "QaaS.Framework.Executions.ExecutionLogging, QaaS.Framework.Executions",
                throwOnError: false);
            var registerDefaultsMethod = executionLoggingType?.GetMethod(
                "RegisterDefaults",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types:
                [
                    typeof(bool),
                    typeof(string),
                    typeof(string),
                    typeof(string)
                ],
                modifiers: null);

            if (registerDefaultsMethod is null)
            {
                return false;
            }

            registerDefaultsMethod.Invoke(null,
            [
                ElasticDefaults.SendLogs,
                ElasticDefaults.ElasticUri,
                ElasticDefaults.ElasticUsername,
                ElasticDefaults.ElasticPassword
            ]);

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
            var reportPortalConfigType = Type.GetType(
                "QaaS.Runner.Assertions.ConfigurationObjects.ReporterConfigs.ReportPortalConfig, QaaS.Runner.Assertions",
                throwOnError: false);
            var registerDefaultsMethod = reportPortalConfigType?.GetMethod(
                "RegisterDefaults",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types:
                [
                    typeof(bool),
                    typeof(string),
                    typeof(string)
                ],
                modifiers: null);

            if (registerDefaultsMethod is null)
            {
                return false;
            }

            registerDefaultsMethod.Invoke(null,
            [
                ReportPortalDefaults.Enabled,
                ReportPortalDefaults.ReportPortalUri,
                ReportPortalDefaults.ReportPortalApiKey
            ]);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
