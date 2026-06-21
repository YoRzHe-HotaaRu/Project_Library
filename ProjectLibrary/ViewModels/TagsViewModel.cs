using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using ProjectLibrary.Helpers;
using ProjectLibrary.Services;

namespace ProjectLibrary.ViewModels;

/// <summary>
/// Tags page view model. Aggregates projects by tag — auto-detected tech stacks
/// contribute their display name (e.g. "Python", "React"), and any custom user tags
/// are also grouped. Each tag becomes a <see cref="TagGroupVm"/> with its projects.
/// </summary>
public partial class TagsViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<TagGroupVm> _tagGroups = new();

    [ObservableProperty] private string _kanji = "札";
    [ObservableProperty] private string _title = "TAGS";
    [ObservableProperty] private string _subtitle = "Browse projects by tag and tech";

    [ObservableProperty]
    private bool _isEmpty;

    public TagsViewModel()
    {
        LoadProjects();
    }

    public void LoadProjects()
    {
        var projects = DatabaseService.GetAllProjects();
        var accumulator = new Dictionary<string, TagBuild>(StringComparer.OrdinalIgnoreCase);

        foreach (var project in projects)
        {
            var card = new ProjectCardViewModel(project);

            // Auto-detected tech stacks become tags
            if (!string.IsNullOrWhiteSpace(project.DetectedTech))
            {
                foreach (var key in SafeDeserialize(project.DetectedTech))
                {
                    if (TechRegistry.Find(key) is { } info)
                        AddToTag(accumulator, info.DisplayName, card, info.ColorHex);
                }
            }

            // User-defined tags
            if (!string.IsNullOrWhiteSpace(project.CustomTags))
            {
                foreach (var tag in SafeDeserialize(project.CustomTags))
                    AddToTag(accumulator, tag, card, null);
            }
        }

        TagGroups = new ObservableCollection<TagGroupVm>(
            accumulator
                .Select(kvp => new TagGroupVm(kvp.Key, kvp.Value.Projects, kvp.Value.ColorHex))
                .OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase));

        IsEmpty = TagGroups.Count == 0;
    }

    private static List<string> SafeDeserialize(string json)
    {
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? new(); }
        catch { return new(); }
    }

    private static void AddToTag(
        Dictionary<string, TagBuild> map,
        string tag,
        ProjectCardViewModel card,
        string? colorHex)
    {
        if (string.IsNullOrWhiteSpace(tag)) return;
        if (!map.TryGetValue(tag, out var build))
        {
            build = new TagBuild();
            map[tag] = build;
        }
        if (colorHex is not null && build.ColorHex is null)
            build.ColorHex = colorHex;
        if (!build.Projects.Any(p => p.Id == card.Id))
            build.Projects.Add(card);
    }

    private sealed class TagBuild
    {
        public string? ColorHex { get; set; }
        public List<ProjectCardViewModel> Projects { get; } = new();
    }
}

/// <summary>
/// One tag's group: name, project count, optional brand color, and the projects that share it.
/// </summary>
public sealed class TagGroupVm
{
    public string Name { get; }
    public int Count { get; }
    public string ColorHex { get; }
    public string ForegroundHex { get; }
    public ObservableCollection<ProjectCardViewModel> Projects { get; }

    public TagGroupVm(string name, List<ProjectCardViewModel> projects, string? colorHex)
    {
        Name = name;
        Count = projects.Count;
        ColorHex = colorHex ?? "#7A7A7A";
        ForegroundHex = ComputeForeground(ColorHex);
        Projects = new ObservableCollection<ProjectCardViewModel>(projects);
    }

    private static string ComputeForeground(string hex)
    {
        var clean = hex.TrimStart('#');
        if (clean.Length != 6) return "#FFFFFF";
        var r = Convert.ToInt32(clean.Substring(0, 2), 16);
        var g = Convert.ToInt32(clean.Substring(2, 2), 16);
        var b = Convert.ToInt32(clean.Substring(4, 2), 16);
        var luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
        return luminance > 0.55 ? "#1A1A1A" : "#FFFFFF";
    }
}
