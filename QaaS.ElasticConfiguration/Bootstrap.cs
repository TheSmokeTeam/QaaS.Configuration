using System.Reflection;

namespace QaaS.ElasticConfiguration;

/// <summary>
/// Registers the package's Elastic logging defaults when the consuming app starts.
/// </summary>
public static class Bootstrap
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
                ElasticConfigurationDefaults.SendLogs,
                ElasticConfigurationDefaults.ElasticUri,
                ElasticConfigurationDefaults.ElasticUsername,
                ElasticConfigurationDefaults.ElasticPassword
            ]);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
