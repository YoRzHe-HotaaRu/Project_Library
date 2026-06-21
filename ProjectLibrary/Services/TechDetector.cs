using System.IO;
using System.Text.Json;
using ProjectLibrary.Helpers;
using ProjectLibrary.Models;

namespace ProjectLibrary.Services;

/// <summary>
/// Scans a project folder for known file signatures and returns the detected tech stacks.
/// </summary>
public static class TechDetector
{
    public static List<TechInfo> Detect(string folderPath)
    {
        var result = new List<TechInfo>();
        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            return result;

        // Git is special — checked first since it affects how we read other files
        if (Directory.Exists(Path.Combine(folderPath, ".git")))
            result.Add(TechRegistry.Git);

        // JavaScript / Node ecosystem
        var packageJsonPath = Path.Combine(folderPath, "package.json");
        if (File.Exists(packageJsonPath))
        {
            result.Add(TechRegistry.Node);
            foreach (var tech in DetectFromPackageJson(packageJsonPath))
                if (!result.Contains(tech))
                    result.Add(tech);
        }


        // Python
        if (File.Exists(Path.Combine(folderPath, "requirements.txt")) ||
            File.Exists(Path.Combine(folderPath, "pyproject.toml")) ||
            File.Exists(Path.Combine(folderPath, "setup.py")) ||
            File.Exists(Path.Combine(folderPath, "Pipfile")))
        {
            result.Add(TechRegistry.Python);
        }

        // Rust
        if (File.Exists(Path.Combine(folderPath, "Cargo.toml")))
            result.Add(TechRegistry.Rust);

        // Go
        if (File.Exists(Path.Combine(folderPath, "go.mod")))
            result.Add(TechRegistry.Go);

        // Java
        if (File.Exists(Path.Combine(folderPath, "pom.xml")) ||
            File.Exists(Path.Combine(folderPath, "build.gradle")) ||
            File.Exists(Path.Combine(folderPath, "build.gradle.kts")))
        {
            result.Add(TechRegistry.Java);
        }

        // C# / .NET
        if (Directory.EnumerateFiles(folderPath, "*.csproj").Any() ||
            Directory.EnumerateFiles(folderPath, "*.sln").Any() ||
            Directory.EnumerateFiles(folderPath, "*.fsproj").Any())
        {
            result.Add(TechRegistry.Csharp);
        }

        // Web (only flag if no JS framework already claimed it)
        if (File.Exists(Path.Combine(folderPath, "index.html")) && !result.Contains(TechRegistry.Node))
            result.Add(TechRegistry.Html);

        // TypeScript as standalone (if package.json already present, deps scan handled it)
        if (File.Exists(Path.Combine(folderPath, "tsconfig.json")) && !result.Contains(TechRegistry.Typescript))
            result.Add(TechRegistry.Typescript);

        // Markdown (only if no other tech detected — many projects have READMEs)
        if (result.Count == 0 && Directory.EnumerateFiles(folderPath, "*.md").Any())
            result.Add(TechRegistry.Markdown);

        return result;
    }

    /// <summary>Reads the first non-empty heading line from README.md (or falls back to null).</summary>
    public static string? ExtractReadmeDescription(string folderPath)
    {
        foreach (var name in new[] { "README.md", "readme.md", "README.MD", "Readme.md" })
        {
            var path = Path.Combine(folderPath, name);
            if (!File.Exists(path)) continue;
            try
            {
                foreach (var line in File.ReadLines(path))
                {
                    var trimmed = line.TrimStart(' ', '#');
                    if (!string.IsNullOrWhiteSpace(trimmed))
                        return trimmed;
                }
            }
            catch { /* ignore */ }
        }
        return null;
    }

    private static List<TechInfo> DetectFromPackageJson(string packageJsonPath)
    {
        var found = new List<TechInfo>();
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(packageJsonPath));
            if (!doc.RootElement.TryGetProperty("dependencies", out var deps) &&
                !doc.RootElement.TryGetProperty("devDependencies", out deps))
                return found;

            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in deps.EnumerateObject())
                keys.Add(prop.Name);

            if (keys.Contains("react"))          found.Add(TechRegistry.React);
            if (keys.Contains("vue"))            found.Add(TechRegistry.Vue);
            if (keys.Contains("@angular/core"))  found.Add(TechRegistry.Angular);
            if (keys.Contains("next"))           found.Add(TechRegistry.Next);
            if (keys.Contains("typescript"))     found.Add(TechRegistry.Typescript);
        }
        catch { /* malformed package.json — skip */ }
        return found;
    }
}
