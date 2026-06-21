using System.Diagnostics;
using System.IO;

namespace ProjectLibrary.Services;

/// <summary>
/// Launches external tools against a project folder. Single-click on a card opens
/// File Explorer (per the user's locked decision in Phase 1); the quick-launch
/// buttons offer VS Code and Windows Terminal as alternatives.
/// </summary>
public static class LauncherService
{
    public static void OpenInExplorer(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;
        StartDetached("explorer.exe", folderPath);
    }

    public static void OpenInVsCode(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;
        // `code` is on PATH if VS Code's "Add to PATH" installer option was used.
        StartDetached("code", $"\"{folderPath}\"");
    }

    public static void OpenInTerminal(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return;
        // Prefer Windows Terminal (wt), fall back to cmd if not installed.
        try { StartDetached("wt.exe", $"-d \"{folderPath}\""); }
        catch { StartDetached("cmd.exe", $"/k \"cd /d {folderPath}\""); }
    }

    private static void StartDetached(string fileName, string arguments)
    {
        var info = new ProcessStartInfo(fileName, arguments)
        {
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Normal
        };
        Process.Start(info);
    }
}
