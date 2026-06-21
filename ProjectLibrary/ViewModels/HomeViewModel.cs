using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectLibrary.Services;

namespace ProjectLibrary.ViewModels;

/// <summary>
/// Home page view model. Owns the hero carousel state (featured projects, current slide,
/// auto-rotation timer) and the recently-active strip. Hover-pause is wired by the view
/// calling <see cref="PauseCarousel"/> / <see cref="ResumeCarousel"/>.
/// </summary>
public partial class HomeViewModel : ObservableObject
{
    private DispatcherTimer? _carouselTimer;
    private bool _isPaused;

    [ObservableProperty]
    private ObservableCollection<ProjectCardViewModel> _featuredProjects = new();

    [ObservableProperty]
    private ProjectCardViewModel? _currentSlide;

    [ObservableProperty]
    private int _currentSlideIndex;

    [ObservableProperty]
    private ObservableCollection<ProjectCardViewModel> _recentlyActive = new();

    [ObservableProperty] private string _kanji = "ホーム";
    [ObservableProperty] private string _title = "HOME";
    [ObservableProperty] private string _subtitle = "Hero carousel + recently active projects";

    public HomeViewModel()
    {
        LoadProjects();
        StartCarousel();
    }

    /// <summary>Reload projects from DB. Pinned favorites appear first in the carousel.</summary>
    public void LoadProjects()
    {
        var cards = DatabaseService.GetAllProjects()
            .Select(p => new ProjectCardViewModel(p))
            .ToList();

        FeaturedProjects = new ObservableCollection<ProjectCardViewModel>(
            cards
                .OrderByDescending(p => p.IsFavorite)
                .ThenByDescending(p => p.LastActive ?? DateTime.MinValue)
                .Take(5));

        RecentlyActive = new ObservableCollection<ProjectCardViewModel>(
            cards
                .Where(p => p.LastActive.HasValue)
                .OrderByDescending(p => p.LastActive!.Value)
                .Take(8));

        if (FeaturedProjects.Count > 0)
        {
            // Keep current index if still valid; otherwise restart at slide 0.
            if (CurrentSlideIndex >= FeaturedProjects.Count)
                CurrentSlideIndex = 0;
            CurrentSlide = FeaturedProjects[CurrentSlideIndex];
        }
        else
        {
            CurrentSlide = null;
            CurrentSlideIndex = 0;
        }
    }

    public bool HasProjects => FeaturedProjects.Count > 0;
    public bool HasNoProjects => FeaturedProjects.Count == 0;
    public string SlideStatus => HasProjects ? $"{CurrentSlideIndex + 1} / {FeaturedProjects.Count}" : "—";

    // ============================== Carousel ==============================

    private void StartCarousel()
    {
        _carouselTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _carouselTimer.Tick += (_, _) =>
        {
            if (!_isPaused && FeaturedProjects.Count > 1)
                GoToNextSlide();
        };
        _carouselTimer.Start();
    }

    public void PauseCarousel() => _isPaused = true;
    public void ResumeCarousel() => _isPaused = false;

    [RelayCommand]
    private void GoToNextSlide()
    {
        if (FeaturedProjects.Count == 0) return;
        CurrentSlideIndex = (CurrentSlideIndex + 1) % FeaturedProjects.Count;
        CurrentSlide = FeaturedProjects[CurrentSlideIndex];
        OnPropertyChanged(nameof(SlideStatus));
    }

    [RelayCommand]
    private void GoToPreviousSlide()
    {
        if (FeaturedProjects.Count == 0) return;
        CurrentSlideIndex = (CurrentSlideIndex - 1 + FeaturedProjects.Count) % FeaturedProjects.Count;
        CurrentSlide = FeaturedProjects[CurrentSlideIndex];
        OnPropertyChanged(nameof(SlideStatus));
    }

    /// <summary>Refresh the home page. Called when the user navigates here or adds a project.</summary>
    [RelayCommand]
    private void Refresh()
    {
        LoadProjects();
        OnPropertyChanged(nameof(SlideStatus));
    }
}
