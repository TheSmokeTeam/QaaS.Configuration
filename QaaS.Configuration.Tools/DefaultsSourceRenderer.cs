namespace QaaS.Configuration.Tools;

/// <summary>
/// Renders source files for package defaults used by the internal packaging CLI.
/// </summary>
public static class DefaultsSourceRenderer
{
    /// <summary>
    /// Recreates the Elastic defaults source file with the requested fallback values.
    /// </summary>
    public static string RenderElasticDefaultsFile(
        bool sendLogs,
        string? elasticUri,
        string? elasticUsername,
        string? elasticPassword)
    {
        return $$"""
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
                     public static bool SendLogs => {{sendLogs.ToString().ToLowerInvariant()}};
                 
                     /// <summary>
                     /// Default Elasticsearch URI used only when no explicit run value was provided.
                     /// </summary>
                     public static string? ElasticUri => {{ToCSharpLiteral(elasticUri)}};
                 
                     /// <summary>
                     /// Default Elasticsearch username used only when no explicit run value was provided.
                     /// </summary>
                     public static string? ElasticUsername => {{ToCSharpLiteral(elasticUsername)}};
                 
                     /// <summary>
                     /// Default Elasticsearch password used only when no explicit run value was provided.
                     /// </summary>
                     public static string? ElasticPassword => {{ToCSharpLiteral(elasticPassword)}};
                 }
                 """;
    }

    /// <summary>
    /// Recreates the ReportPortal defaults source file with the requested fallback values.
    /// </summary>
    public static string RenderReportPortalDefaultsFile(
        bool reportPortalEnabled,
        string? reportPortalUri,
        string? reportPortalApiKey)
    {
        return $$"""
                 namespace QaaS.Configuration;
                 
                 /// <summary>
                 /// Built-in fallback values for ReportPortal integration.
                 /// Replace these values in the air-gapped variant and publish it with the same package ID and version.
                 /// </summary>
                 public static class ReportPortalDefaults
                 {
                     /// <summary>
                     /// Enables ReportPortal integration when no explicit run value was provided.
                     /// </summary>
                     public static bool Enabled => {{reportPortalEnabled.ToString().ToLowerInvariant()}};
                 
                     /// <summary>
                     /// Default ReportPortal URI used only when no explicit run value was provided.
                     /// </summary>
                     public static string? ReportPortalUri => {{ToCSharpLiteral(reportPortalUri)}};
                 
                     /// <summary>
                     /// Default ReportPortal API key used only when no explicit run value was provided.
                     /// </summary>
                     public static string? ReportPortalApiKey => {{ToCSharpLiteral(reportPortalApiKey)}};
                 }
                 """;
    }

    /// <summary>
    /// Converts optional string values into valid C# literals for the generated defaults source file.
    /// </summary>
    private static string ToCSharpLiteral(string? value)
    {
        return value is null
            ? "null"
            : $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
    }
}
