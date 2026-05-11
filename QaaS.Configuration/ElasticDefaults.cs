namespace QaaS.Configuration;

/// <summary>
/// Built-in fallback values registered by the configuration package when no explicit Elastic options were provided.
/// Replace these values in the air-gapped variant and publish it with the same package ID and version.
/// </summary>
public static class ElasticDefaults
{
    /// <summary>
    /// Enables the existing Elastic sink path when no explicit run value was provided.
    /// </summary>
    public static bool SendLogs => false;

    /// <summary>
    /// Default Elasticsearch URI used only when no explicit run value was provided.
    /// </summary>
    public static string? ElasticUri => null;

    /// <summary>
    /// Default Elasticsearch username used only when no explicit run value was provided.
    /// </summary>
    public static string? ElasticUsername => null;

    /// <summary>
    /// Default Elasticsearch password used only when no explicit run value was provided.
    /// </summary>
    public static string? ElasticPassword => null;
}
