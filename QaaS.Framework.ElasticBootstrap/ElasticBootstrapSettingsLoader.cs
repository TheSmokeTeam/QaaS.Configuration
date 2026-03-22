using Microsoft.Extensions.Configuration;

namespace QaaS.Framework.ElasticBootstrap;

internal static class ElasticBootstrapSettingsLoader
{
    private const string ConfigPathEnvironmentVariableName = "QAAS_ELASTIC_BOOTSTRAP_CONFIG_PATH";
    private const string AppLocalConfigFileName = "qaas.elastic.bootstrap.json";

    public static ElasticBootstrapSettings? TryLoad()
    {
        foreach (var candidatePath in GetCandidatePaths())
        {
            if (string.IsNullOrWhiteSpace(candidatePath) || !File.Exists(candidatePath))
            {
                continue;
            }

            try
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile(candidatePath, optional: false, reloadOnChange: false)
                    .Build();

                return configuration.Get<ElasticBootstrapSettings>();
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    private static IEnumerable<string?> GetCandidatePaths()
    {
        yield return Environment.GetEnvironmentVariable(ConfigPathEnvironmentVariableName);
        yield return Path.Combine(AppContext.BaseDirectory, AppLocalConfigFileName);

        var commonApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        if (!string.IsNullOrWhiteSpace(commonApplicationDataPath))
        {
            yield return Path.Combine(commonApplicationDataPath, "QaaS", "ElasticBootstrap", "settings.json");
        }
    }
}
