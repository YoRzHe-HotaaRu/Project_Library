using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectLibrary.Services;

namespace ProjectLibrary.ViewModels;

/// <summary>
/// Settings page view model. Phase 4 ships a read-only info view (app version,
/// DB location, project count, tech stack) plus an "open DB folder" shortcut.
/// Editable preferences will land in a future revision.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _kanji = "設定";
    [ObservableProperty] private string _title = "SETTINGS";
    [ObservableProperty] private string _subtitle = "Application information";

    public string AppVersion { get; } = "1.0.0";
    public string DbPath => DatabaseService.DbPath;
    public string DbSize => FormatBytes(new FileInfo(DatabaseService.DbPath).Length);

    [ObservableProperty]
    private int _projectCount;

    [ObservableProperty]
    private int _favoriteCount;

    public SettingsViewModel()
    {
        Refresh();
    }

    /// <summary>Reload stats from DB. Called externally when projects change.</summary>
    public void Refresh()
    {
        var projects = DatabaseService.GetAllProjects();
        ProjectCount = projects.Count;
        FavoriteCount = projects.Count(p => p.IsFavorite);
    }

    [RelayCommand]
    private void RefreshStats() => Refresh();

    /// <summary>Open the folder containing library.db in File Explorer.</summary>
    [RelayCommand]
    private void OpenDbFolder()
    {
        var folder = Path.GetDirectoryName(DatabaseService.DbPath);
        if (!string.IsNullOrEmpty(folder))
            LauncherService.OpenInExplorer(folder);
    }

    private static string FormatBytes(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        _ => $"{bytes / (1024.0 * 1024):F1} MB"
    };
}
