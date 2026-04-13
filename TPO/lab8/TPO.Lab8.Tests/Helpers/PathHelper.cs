namespace TPO.Lab8.Tests.Helpers;

public static class PathHelper
{
    private const string ProjectFileName = "TPO.Lab8.Tests.csproj";

    public static string ProjectRoot => FindProjectRoot();

    public static string ArtifactsRoot => EnsureDirectory(Path.Combine(ProjectRoot, "Artifacts"));

    public static string ScreenshotsRoot => EnsureDirectory(Path.Combine(ArtifactsRoot, "Screenshots"));

    public static string CookiesRoot => EnsureDirectory(Path.Combine(ArtifactsRoot, "Cookies"));

    public static string ReportsRoot => EnsureDirectory(Path.Combine(ArtifactsRoot, "Reports"));

    private static string FindProjectRoot()
    {
        var candidates = new[]
        {
            new DirectoryInfo(Directory.GetCurrentDirectory()),
            new DirectoryInfo(AppContext.BaseDirectory)
        };

        foreach (var startDir in candidates)
        {
            var current = startDir;

            while (current != null)
            {
                if (File.Exists(Path.Combine(current.FullName, ProjectFileName)))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }
        }

        throw new DirectoryNotFoundException($"Could not find {ProjectFileName} by traversing parent directories.");
    }

    public static string EnsureDirectory(string path)
    {
        Directory.CreateDirectory(path);
        return path;
    }
}
