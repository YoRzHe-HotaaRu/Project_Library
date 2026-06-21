using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectLibrary.Helpers;
using ProjectLibrary.Models;
using ProjectLibrary.Services;

namespace ProjectLibrary.ViewModels;

/// <summary>
/// View model for a single manga-styled project card. Wraps a <see cref="Project"/>,
/// exposes display-friendly properties (relative timestamps, shortened paths, tech badges),
/// and provides commands for the card click + quick-launch buttons.
/// </summary>
public partial class ProjectCardViewModel : ObservableObject
{
    private readonly Project _project;

    public ProjectCardViewModel(Project project)
    {
        _project = project;
        _isFavorite = project.IsFavorite;
        TechBadges = new ObservableCollection<TechBadgeVm>(BuildBadges(project.DetectedTech));
    }

    public long Id => _project.Id;
    public string Name => _project.Name;
    public string FolderPath => _project.FolderPath;
    public DateTime? LastActive => _project.LastActive;
    public DateTime DateAdded => _project.DateAdded;
    public string Description => string.IsNullOrWhiteSpace(_project.Description) ? "No description" : _project.Description;

    /// <summary>Short path with the user profile prefix collapsed to ~ for display.</summary>
    public string ShortPath => ShortenPath(_project.FolderPath);

    /// <summary>Relative time like "3d ago", "2w ago", "just now".</summary>
    public string RelativeLastActive => FormatRelative(_project.LastActive);

    /// <summary>First letter of the project name (uppercased) for the placeholder thumbnail.</summary>
    public string ThumbnailLetter =>
        string.IsNullOrEmpty(Name) ? "?" : Name.Substring(0, 1).ToUpperInvariant();

    /// <summary>First detected tech's color (used to tint the placeholder thumbnail background).</summary>
    public string ThumbnailColorHex =>
        TechBadges.FirstOrDefault()?.ColorHex ?? "#7A7A7A";

    public ObservableCollection<TechBadgeVm> TechBadges { get; }

    [ObservableProperty]
    private bool _isFavorite;

    [RelayCommand]
    private void ToggleFavorite()
    {
        IsFavorite = DatabaseService.ToggleFavorite(_project.Id);
        _project.IsFavorite = IsFavorite;
    }

    /// <summary>Single-click default action: open in File Explorer.</summary>
    [RelayCommand]
    private void Open() => LauncherService.OpenInExplorer(_project.FolderPath);

    [RelayCommand]
    private void OpenInVsCode() => LauncherService.OpenInVsCode(_project.FolderPath);

    [RelayCommand]
    private void OpenInTerminal() => LauncherService.OpenInTerminal(_project.FolderPath);

    // ============================== Helpers ==============================

    private static List<TechBadgeVm> BuildBadges(string? detectedTechJson)
    {
        var badges = new List<TechBadgeVm>();
        if (string.IsNullOrWhiteSpace(detectedTechJson))
            return badges;

        try
        {
            foreach (var key in JsonSerializer.Deserialize<List<string>>(detectedTechJson) ?? new())
            {
                if (TechRegistry.Find(key) is { } info)
                    badges.Add(TechBadgeVm.From(info));
            }
        }
        catch { /* malformed JSON — show what we have */ }

        return badges;
    }

    /// <summary>Builds a TechBadgeVm list from raw TechInfo objects (used by AddProject flow).</summary>
    public static List<TechBadgeVm> BuildBadges(IEnumerable<TechInfo> tech) =>
        tech.Select(TechBadgeVm.From).ToList();

    private static string FormatRelative(DateTime? value)
    {
        if (!value.HasValue) return "never";

        var delta = DateTime.Now - value.Value;
        if (delta.TotalMinutes < 1)   return "just now";
        if (delta.TotalHours < 1)     return $"{(int)delta.TotalMinutes}m ago";
        if (delta.TotalDays < 1)      return $"{(int)delta.TotalHours}h ago";
        if (delta.TotalDays < 7)      return $"{(int)delta.TotalDays}d ago";
        if (delta.TotalDays < 30)     return $"{(int)(delta.TotalDays / 7)}w ago";
        if (delta.TotalDays < 365)    return $"{(int)(delta.TotalDays / 30)}mo ago";
        return $"{(int)(delta.TotalDays / 365)}y ago";
    }

    private static string ShortenPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return "";
        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (path.StartsWith(profile, StringComparison.OrdinalIgnoreCase))
            path = "~" + path.Substring(profile.Length);
        return path;
    }
}

/// <summary>
/// Display-side view of a tech badge: the tech name, its brand color (as background),
/// and a computed foreground color (white for dark backgrounds, ink for light).
/// </summary>
public sealed record TechBadgeVm(string Key, string DisplayName, string ColorHex, string ForegroundHex)
{
    public static TechBadgeVm From(TechInfo info)
    {
        var foreground = ComputeForeground(info.ColorHex);
        return new TechBadgeVm(info.Key, info.DisplayName, info.ColorHex, foreground);
    }

    private static string ComputeForeground(string hex)
    {
        var clean = hex.TrimStart('#');
        if (clean.Length != 6) return "#FFFFFF";
        var r = Convert.ToInt32(clean.Substring(0, 2), 16);
        var g = Convert.ToInt32(clean.Substring(2, 2), 16);
        var b = Convert.ToInt32(clean.Substring(4, 2), 16);
        // Rec. 709 luminance — bright backgrounds get ink text, dark get paper text.
        var luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
        return luminance > 0.55 ? "#1A1A1A" : "#FFFFFF";
    }
}
