using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ProjectLibrary.Services;

namespace ProjectLibrary.ViewModels;

/// <summary>
/// Favorites page view model. Loads only favorited projects and supports a search filter.
/// </summary>
public partial class FavoritesViewModel : ObservableObject
{
    private List<ProjectCardViewModel> _all = new();

    [ObservableProperty]
    private ObservableCollection<ProjectCardViewModel> _visibleProjects = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty] private string _kanji = "星";
    [ObservableProperty] private string _title = "FAVORITES";
    [ObservableProperty] private string _subtitle = "Projects you've pinned";

    public FavoritesViewModel()
    {
        LoadProjects();
    }

    public void LoadProjects()
    {
        _all = DatabaseService.GetAllProjects()
            .Where(p => p.IsFavorite)
            .Select(p => new ProjectCardViewModel(p))
            .ToList();
        ApplyFilter();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        IEnumerable<ProjectCardViewModel> result = _all;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var query = SearchText.Trim();
            result = result.Where(p =>
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.FolderPath.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        // Favorites ordered by most recent activity
        result = result.OrderByDescending(p => p.LastActive ?? DateTime.MinValue);

        VisibleProjects = new ObservableCollection<ProjectCardViewModel>(result);
        IsEmpty = _all.Count == 0;
    }
}
