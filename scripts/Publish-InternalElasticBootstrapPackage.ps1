Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$PackageVersion = "1.0.0"
$SendLogs = $true
$ElasticUri = "http://your-internal-elastic:9200"
$ElasticUsername = $null
$ElasticPassword = $null

$PushToArtifactory = $false
$ArtifactorySource = "https://your-artifactory.example/api/nuget/qaas-local"
$ArtifactoryApiKey = ""

$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepositoryRoot = Split-Path -Parent $ScriptRoot
$ProjectPath = Join-Path $RepositoryRoot "QaaS.ElasticBootstrap\QaaS.ElasticBootstrap.csproj"
$DefaultsFilePath = Join-Path $RepositoryRoot "QaaS.ElasticBootstrap\ElasticBootstrapDefaults.cs"
$ArtifactsRoot = Join-Path $RepositoryRoot "artifacts"
$OutputDirectory = Join-Path $ArtifactsRoot "internal-package"
$TempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("qaas-elastic-bootstrap-" + [System.Guid]::NewGuid().ToString("N"))

function ConvertTo-CSharpLiteral {
    param(
        [AllowNull()]
        [string]$Value
    )

    if ($null -eq $Value) {
        return "null"
    }

    return '"' + $Value.Replace('\', '\\').Replace('"', '\"') + '"'
}

function New-DefaultsFileContent {
    param(
        [bool]$ConfiguredSendLogs,
        [AllowNull()][string]$ConfiguredElasticUri,
        [AllowNull()][string]$ConfiguredElasticUsername,
        [AllowNull()][string]$ConfiguredElasticPassword
    )

    @"
namespace QaaS.ElasticBootstrap;

/// <summary>
/// Built-in fallback values registered by the bootstrap package when no explicit Elastic options were provided.
/// Replace these values in the air-gapped variant and publish it with the same package ID and version.
/// </summary>
public static class ElasticBootstrapDefaults
{
    /// <summary>
    /// Enables the existing Elastic sink path when no explicit run value was provided.
    /// </summary>
    public static bool SendLogs => $($ConfiguredSendLogs.ToString().ToLowerInvariant());

    /// <summary>
    /// Default Elasticsearch URI used only when no explicit run value was provided.
    /// </summary>
    public static string? ElasticUri => $(ConvertTo-CSharpLiteral $ConfiguredElasticUri);

    /// <summary>
    /// Default Elasticsearch username used only when no explicit run value was provided.
    /// </summary>
    public static string? ElasticUsername => $(ConvertTo-CSharpLiteral $ConfiguredElasticUsername);

    /// <summary>
    /// Default Elasticsearch password used only when no explicit run value was provided.
    /// </summary>
    public static string? ElasticPassword => $(ConvertTo-CSharpLiteral $ConfiguredElasticPassword);
}
"@
}

if (-not (Test-Path $ProjectPath)) {
    throw "Project file not found at '$ProjectPath'."
}

New-Item -ItemType Directory -Path $ArtifactsRoot -Force | Out-Null
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
New-Item -ItemType Directory -Path $TempRoot -Force | Out-Null

try {
    Copy-Item $RepositoryRoot\* $TempRoot -Recurse -Force -Exclude ".git", "bin", "obj", "artifacts"

    $tempDefaultsFilePath = Join-Path $TempRoot "QaaS.ElasticBootstrap\ElasticBootstrapDefaults.cs"
    $defaultsFileContent = New-DefaultsFileContent -ConfiguredSendLogs $SendLogs `
        -ConfiguredElasticUri $ElasticUri `
        -ConfiguredElasticUsername $ElasticUsername `
        -ConfiguredElasticPassword $ElasticPassword

    Set-Content -Path $tempDefaultsFilePath -Value $defaultsFileContent -Encoding UTF8

    dotnet pack (Join-Path $TempRoot "QaaS.ElasticBootstrap\QaaS.ElasticBootstrap.csproj") `
        -c Release `
        -o $OutputDirectory `
        -p:PackageVersion=$PackageVersion `
        -p:Version=$PackageVersion

    if ($PushToArtifactory) {
        if ([string]::IsNullOrWhiteSpace($ArtifactoryApiKey)) {
            throw "Artifactory push is enabled, but ArtifactoryApiKey is empty."
        }

        dotnet nuget push (Join-Path $OutputDirectory "*.nupkg") `
            --source $ArtifactorySource `
            --api-key $ArtifactoryApiKey `
            --skip-duplicate

        Get-ChildItem $OutputDirectory -Filter "*.snupkg" -File | ForEach-Object {
            dotnet nuget push $_.FullName `
                --source $ArtifactorySource `
                --api-key $ArtifactoryApiKey `
                --skip-duplicate
        }
    }

    Write-Host "Internal bootstrap package created in '$OutputDirectory'."
}
finally {
    if (Test-Path $TempRoot) {
        Remove-Item $TempRoot -Recurse -Force
    }
}
