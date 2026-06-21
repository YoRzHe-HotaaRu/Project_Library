using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectLibrary.Services;

namespace ProjectLibrary.ViewModels;

/// <summary>
/// Library page view model. Loads real projects from the database, exposes them
/// as manga-styled card view models, and supports sort, filter, and real-time search.
/// </summary>
public partial class LibraryViewModel : ObservableObject
{
    /// <summary>All loaded cards before filtering. Kept so re-filtering doesn't hit the DB.</summary>
    private List<ProjectCardViewModel> _allProjects = new();

    /// <summary>The cards currently shown after applying the filter + sort.</summary>
    [ObservableProperty]
    private ObservableCollection<ProjectCardViewModel> _visibleProjects = new();

    /// <summary>True when there are no projects at all (drives the empty-state UI).</summary>
    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedSort = "Date Added";

    [ObservableProperty]
    private bool _showFavoritesOnly;

    public List<string> SortOptions { get; } = new() { "Date Added", "Name", "Last Active", "Tech" };

    // Header display fields (kept so the LibraryView XAML still binds cleanly)
    [ObservableProperty] private string _kanji = "図書館";
    [ObservableProperty] private string _title = "LIBRARY";
    [ObservableProperty] private string _subtitle = "All projects, grid and list views";

    public LibraryViewModel()
    {
        LoadProjects();
    }

    /// <summary>Reloads all projects from the database and re-applies the filter.</summary>
    public void LoadProjects()
    {
        var projects = DatabaseService.GetAllProjects();
        _allProjects = projects.Select(p => new ProjectCardViewModel(p)).ToList();
        ApplyFilterAndSort();
    }

    [RelayCommand]
    private void Refresh() => LoadProjects();

    // Re-apply the filter/sort whenever inputs change
    partial void OnSearchTextChanged(string value) => ApplyFilterAndSort();
    partial void OnSelectedSortChanged(string value) => ApplyFilterAndSort();
    partial void OnShowFavoritesOnlyChanged(bool value) => ApplyFilterAndSort();

    private void ApplyFilterAndSort()
    {
        IEnumerable<ProjectCardViewModel> result = _allProjects;

        if (ShowFavoritesOnly)
            result = result.Where(p => p.IsFavorite);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var query = SearchText.Trim();
            result = result.Where(p =>
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.FolderPath.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.TechBadges.Any(t => t.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }

        result = SelectedSort switch
        {
            "Name"        => result.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase),
            "Last Active" => result.OrderByDescending(p => p.LastActive ?? DateTime.MinValue),
            "Tech"        => result.OrderBy(p => p.TechBadges.FirstOrDefault()?.DisplayName ?? "zzz"),
            _             => result.OrderByDescending(p => p.DateAdded)
        };

        VisibleProjects = new ObservableCollection<ProjectCardViewModel>(result);
        IsEmpty = _allProjects.Count == 0;
    }
}
