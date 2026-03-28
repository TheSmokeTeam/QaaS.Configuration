using System.Diagnostics;

namespace QaaS.ElasticBootstrap.Tools;

/// <summary>
/// Rebuilds the internal Elastic bootstrap package with environment-specific default values.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Copies the repository into a disposable staging area, rewrites the defaults source file, packs the package, and optionally pushes it.
    /// </summary>
    public static async Task<int> Main(string[] args)
    {
        var arguments = CommandArguments.Parse(args);
        var repositoryRoot = arguments.GetOptionalPath("--repository-root") ?? FindRepositoryRoot();
        var packageVersion = arguments.GetOptionalValue("--package-version") ?? "1.0.0";
        var sendLogs = arguments.GetOptionalBool("--send-logs") ?? true;
        var elasticUri = arguments.GetOptionalValue("--elastic-uri") ?? "http://your-internal-elastic:9200";
        var elasticUsername = arguments.GetOptionalValue("--elastic-username");
        var elasticPassword = arguments.GetOptionalValue("--elastic-password");
        var pushToArtifactory = arguments.GetOptionalBool("--push-to-artifactory") ?? false;
        var artifactorySource = arguments.GetOptionalValue("--artifactory-source") ?? "https://your-artifactory.example/api/nuget/qaas-local";
        var artifactoryApiKey = arguments.GetOptionalValue("--artifactory-api-key") ?? string.Empty;

        var projectPath = Path.Combine(repositoryRoot, "QaaS.ElasticBootstrap", "QaaS.ElasticBootstrap.csproj");
        var artifactsRoot = Path.Combine(repositoryRoot, "artifacts");
        var outputDirectory = Path.Combine(artifactsRoot, "internal-package");
        var tempRoot = Path.Combine(Path.GetTempPath(), $"qaas-elastic-bootstrap-{Guid.NewGuid():N}");

        if (!File.Exists(projectPath))
        {
            throw new FileNotFoundException($"Project file not found at '{projectPath}'.", projectPath);
        }

        Directory.CreateDirectory(artifactsRoot);
        Directory.CreateDirectory(outputDirectory);
        Directory.CreateDirectory(tempRoot);

        try
        {
            CopyRepositoryTree(repositoryRoot, tempRoot);

            var defaultsFilePath = Path.Combine(tempRoot, "QaaS.ElasticBootstrap", "ElasticBootstrapDefaults.cs");
            await File.WriteAllTextAsync(
                defaultsFilePath,
                RenderDefaultsFile(sendLogs, elasticUri, elasticUsername, elasticPassword));

            await ProcessRunner.RunAsync(
                "dotnet",
                [
                    "pack",
                    Path.Combine(tempRoot, "QaaS.ElasticBootstrap", "QaaS.ElasticBootstrap.csproj"),
                    "-c",
                    "Release",
                    "-o",
                    outputDirectory,
                    $"-p:PackageVersion={packageVersion}",
                    $"-p:Version={packageVersion}"
                ],
                tempRoot);

            var packageFiles = Directory.EnumerateFiles(outputDirectory, $"QaaS.ElasticBootstrap.{packageVersion}.nupkg", SearchOption.TopDirectoryOnly)
                .ToArray();
            var symbolPackages = Directory.EnumerateFiles(outputDirectory, $"QaaS.ElasticBootstrap.{packageVersion}.snupkg", SearchOption.TopDirectoryOnly)
                .ToArray();

            if (pushToArtifactory)
            {
                if (string.IsNullOrWhiteSpace(artifactoryApiKey))
                {
                    throw new InvalidOperationException(
                        "Artifactory push is enabled, but --artifactory-api-key was not provided.");
                }

                foreach (var package in packageFiles.Concat(symbolPackages))
                {
                    await ProcessRunner.RunAsync(
                        "dotnet",
                        [
                            "nuget",
                            "push",
                            package,
                            "--source",
                            artifactorySource,
                            "--api-key",
                            artifactoryApiKey,
                            "--skip-duplicate"
                        ],
                        repositoryRoot);
                }
            }

            Console.WriteLine($"Internal bootstrap package created in '{outputDirectory}'.");
            return 0;
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    /// <summary>
    /// Copies the repository tree into a staging directory while excluding transient build output and git metadata.
    /// </summary>
    private static void CopyRepositoryTree(string sourceDirectory, string destinationDirectory)
    {
        foreach (var entry in Directory.EnumerateFileSystemEntries(sourceDirectory))
        {
            var name = Path.GetFileName(entry);
            if (name is ".git" or "bin" or "obj" or "artifacts")
            {
                continue;
            }

            var destinationPath = Path.Combine(destinationDirectory, name);
            if (Directory.Exists(entry))
            {
                DirectoryCopy(entry, destinationPath);
                continue;
            }

            File.Copy(entry, destinationPath, overwrite: true);
        }
    }

    /// <summary>
    /// Recursively copies a repository subdirectory while preserving the original relative layout.
    /// </summary>
    private static void DirectoryCopy(string sourceDirectory, string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);

        foreach (var entry in Directory.EnumerateFileSystemEntries(sourceDirectory))
        {
            var name = Path.GetFileName(entry);
            if (name is "bin" or "obj" or "artifacts")
            {
                continue;
            }

            var destinationPath = Path.Combine(destinationDirectory, name);
            if (Directory.Exists(entry))
            {
                DirectoryCopy(entry, destinationPath);
                continue;
            }

            File.Copy(entry, destinationPath, overwrite: true);
        }
    }

    /// <summary>
    /// Recreates the generated defaults source file with the requested fallback values.
    /// </summary>
    private static string RenderDefaultsFile(
        bool sendLogs,
        string? elasticUri,
        string? elasticUsername,
        string? elasticPassword)
    {
        return $$"""
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
    /// Converts optional string values into valid C# literals for the generated defaults source file.
    /// </summary>
    private static string ToCSharpLiteral(string? value)
    {
        return value is null
            ? "null"
            : $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
    }

    /// <summary>
    /// Finds the repository root by walking up from the compiled tool output.
    /// </summary>
    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "QaaS.ElasticBootstrap.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the QaaS.ElasticBootstrap repository root.");
    }
}

/// <summary>
/// Minimal argument parser for the internal bootstrap packaging CLI.
/// </summary>
internal sealed class CommandArguments
{
    private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Parses a PowerShell-style list of named arguments into a lookup table.
    /// </summary>
    public static CommandArguments Parse(IEnumerable<string> args)
    {
        var parsed = new CommandArguments();
        var tokens = args.ToArray();
        for (var index = 0; index < tokens.Length; index++)
        {
            if (!tokens[index].StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            if (index + 1 < tokens.Length && !tokens[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                parsed._values[tokens[index]] = tokens[index + 1];
                index++;
            }
        }

        return parsed;
    }

    /// <summary>
    /// Reads a string value when the option is present.
    /// </summary>
    public string? GetOptionalValue(string key) => _values.GetValueOrDefault(key);

    /// <summary>
    /// Reads and normalizes a path value when the option is present.
    /// </summary>
    public string? GetOptionalPath(string key)
    {
        var path = GetOptionalValue(key);
        return string.IsNullOrWhiteSpace(path) ? null : Path.GetFullPath(path);
    }

    /// <summary>
    /// Reads a boolean value when the option is present.
    /// </summary>
    public bool? GetOptionalBool(string key)
    {
        var value = GetOptionalValue(key);
        return value is null ? null : bool.Parse(value);
    }
}

/// <summary>
/// Executes external processes while streaming their output for the operator.
/// </summary>
internal static class ProcessRunner
{
    /// <summary>
    /// Runs a process and throws when it exits unsuccessfully.
    /// </summary>
    public static async Task RunAsync(string fileName, IReadOnlyList<string> arguments, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start process '{fileName}'.");
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        var output = await outputTask;
        var error = await errorTask;
        if (!string.IsNullOrWhiteSpace(output))
        {
            Console.Write(output);
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            Console.Error.Write(error);
        }

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Command '{fileName} {string.Join(' ', arguments)}' failed with exit code {process.ExitCode}.");
        }
    }
}
