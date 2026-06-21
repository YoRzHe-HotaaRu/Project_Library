using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectLibrary.Models;
using ProjectLibrary.Services;

namespace ProjectLibrary.ViewModels;

/// <summary>
/// View model for the Add Project dialog. Handles folder selection, tech auto-detection,
/// tag entry, and persistence via <see cref="DatabaseService"/>.
/// Notifies the host via <see cref="ProjectAdded"/> / <see cref="RequestClose"/>.
/// </summary>
public partial class AddProjectViewModel : ObservableObject
{
    [ObservableProperty]
    private string _folderPath = string.Empty;

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private string _tagInput = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>Tech detected from the selected folder, shown as colored badges.</summary>
    public ObservableCollection<TechBadgeVm> DetectedTech { get; } = new();

    /// <summary>User-added tags.</summary>
    public ObservableCollection<string> CustomTags { get; } = new();

    public bool HasFolder => !string.IsNullOrWhiteSpace(FolderPath);

    /// <summary>Raised when the user successfully saves a project. Host refreshes library.</summary>
    public event Action? ProjectAdded;

    /// <summary>Raised when the dialog should close (save or cancel).</summary>
    public event Action? RequestClose;

    /// <summary>Reset all fields. Call before opening the dialog.</summary>
    public void Reset()
    {
        FolderPath = string.Empty;
        ProjectName = string.Empty;
        Description = string.Empty;
        Notes = string.Empty;
        TagInput = string.Empty;
        StatusMessage = string.Empty;
        DetectedTech.Clear();
        CustomTags.Clear();
    }

    [RelayCommand]
    private void Browse()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select project folder",
            DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        if (dialog.ShowDialog() == true)
        {
            FolderPath = dialog.FolderName;
            if (string.IsNullOrWhiteSpace(ProjectName))
                ProjectName = Path.GetFileName(FolderPath.TrimEnd(Path.DirectorySeparatorChar));
            ScanFolder();
            OnPropertyChanged(nameof(HasFolder));
        }
    }

    private void ScanFolder()
    {
        DetectedTech.Clear();
        if (string.IsNullOrWhiteSpace(FolderPath) || !Directory.Exists(FolderPath))
            return;

        foreach (var badge in ProjectCardViewModel.BuildBadges(TechDetector.Detect(FolderPath)))
            DetectedTech.Add(badge);

        if (string.IsNullOrWhiteSpace(Description))
        {
            var fromReadme = TechDetector.ExtractReadmeDescription(FolderPath);
            if (!string.IsNullOrWhiteSpace(fromReadme))
                Description = fromReadme;
        }
    }

    [RelayCommand]
    private void AddTag()
    {
        var tag = (TagInput ?? string.Empty).Trim();
        if (!string.IsNullOrEmpty(tag) && !CustomTags.Contains(tag))
            CustomTags.Add(tag);
        TagInput = string.Empty;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        if (!Directory.Exists(FolderPath))
        {
            StatusMessage = "Folder does not exist.";
            return;
        }

        try
        {
            var project = new Project
            {
                Name = string.IsNullOrWhiteSpace(ProjectName)
                    ? Path.GetFileName(FolderPath.TrimEnd(Path.DirectorySeparatorChar))
                    : ProjectName,
                FolderPath = FolderPath,
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description,
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes,
                DetectedTech = JsonSerializer.Serialize(DetectedTech.Select(t => t.Key).ToList()),
                CustomTags = CustomTags.Count > 0
                    ? JsonSerializer.Serialize(CustomTags.ToList())
                    : null,
                LastActive = ProjectScanner.GetLastActive(FolderPath),
                DateAdded = DateTime.Now,
                IsFavorite = false
            };

            var id = DatabaseService.InsertProject(project);
            DatabaseService.LogActivity(id, "added");
            ProjectAdded?.Invoke();
            RequestClose?.Invoke();
        }
        catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            StatusMessage = "This folder is already in your library.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Error: " + ex.Message;
        }
    }

    private bool CanSave => HasFolder;

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke();

    /// <summary>Re-evaluates Save button availability when folder changes.</summary>
    partial void OnFolderPathChanged(string value) => SaveCommand.NotifyCanExecuteChanged();
}
