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
    public static bool Enabled => false;

    /// <summary>
    /// Default ReportPortal URI used only when no explicit run value was provided.
    /// </summary>
    public static string? ReportPortalUri => null;

    /// <summary>
    /// Default ReportPortal API key used only when no explicit run value was provided.
    /// </summary>
    public static string? ReportPortalApiKey => null;
}
