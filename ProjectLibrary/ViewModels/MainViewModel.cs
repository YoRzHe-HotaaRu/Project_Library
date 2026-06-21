using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ProjectLibrary.ViewModels;

/// <summary>
/// Root view model for the application shell. Owns the five page view models
/// and tracks the currently visible page. Also drives the Add Project dialog
/// (IsAddDialogOpen) and refreshes the library when a project is added.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableObject? _currentPage;

    [ObservableProperty]
    private bool _isAddDialogOpen;

    /// <summary>
    /// Bound to the top-bar search box. When non-empty, navigates to Library
    /// and propagates the query there. Clearing it clears the Library filter too.
    /// </summary>
    [ObservableProperty]
    private string _globalSearchText = string.Empty;

    public HomeViewModel Home { get; }
    public LibraryViewModel Library { get; }
    public FavoritesViewModel Favorites { get; }
    public TagsViewModel Tags { get; }
    public SettingsViewModel Settings { get; }

    public AddProjectViewModel AddProject { get; }

    public MainViewModel()
    {
        Home = new HomeViewModel();
        Library = new LibraryViewModel();
        Favorites = new FavoritesViewModel();
        Tags = new TagsViewModel();
        Settings = new SettingsViewModel();
        AddProject = new AddProjectViewModel();

        AddProject.RequestClose += () => IsAddDialogOpen = false;
        AddProject.ProjectAdded += OnProjectAdded;

        _currentPage = Home;
    }

    /// <summary>Switches the current page by key ("home", "library", etc.). Bound to sidebar nav.</summary>
    [RelayCommand]
    private void NavigateTo(string pageKey)
    {
        CurrentPage = pageKey switch
        {
            "home" => Home,
            "library" => Library,
            "favorites" => Favorites,
            "tags" => Tags,
            "settings" => Settings,
            _ => CurrentPage
        };
    }

    /// <summary>Opens the Add Project dialog after resetting its fields.</summary>
    [RelayCommand]
    private void OpenAddDialog()
    {
        AddProject.Reset();
        IsAddDialogOpen = true;
    }

    /// <summary>
    /// When the global search box has text, jump to the Library and apply the filter.
    /// Clearing the box clears the filter too. Lets the user search from anywhere.
    /// </summary>
    partial void OnGlobalSearchTextChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            CurrentPage = Library;
        Library.SearchText = value ?? string.Empty;
    }

    private void OnProjectAdded()
    {
        IsAddDialogOpen = false;
        Library.LoadProjects();
        Home.LoadProjects();
        Favorites.LoadProjects();
        Tags.LoadProjects();
        Settings.Refresh();
    }
}
