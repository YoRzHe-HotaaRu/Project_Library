using System.IO;

namespace ProjectLibrary.Services;

/// <summary>
/// Inspects a project folder for "last active" timestamp without doing a deep walk
/// (which would be slow for large repos). Checks top-level files + folders only.
/// </summary>
public static class ProjectScanner
{
    public static DateTime? GetLastActive(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            return null;

        DateTime latest = DateTime.MinValue;

        try
        {
            var dir = new DirectoryInfo(folderPath);
            if (dir.LastWriteTime > latest) latest = dir.LastWriteTime;

            foreach (var file in dir.EnumerateFiles())
                if (file.LastWriteTime > latest) latest = file.LastWriteTime;

            foreach (var sub in dir.EnumerateDirectories())
                if (sub.LastWriteTime > latest) latest = sub.LastWriteTime;
        }
        catch { /* permission or IO errors — return what we have */ }

        return latest == DateTime.MinValue ? null : latest;
    }
}
