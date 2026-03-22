namespace QaaS.Framework.ElasticBootstrap;

internal sealed class ElasticBootstrapSettings
{
    public bool SendLogs { get; init; }

    public string? ElasticUri { get; init; }

    public string? ElasticUsername { get; init; }

    public string? ElasticPassword { get; init; }
}
