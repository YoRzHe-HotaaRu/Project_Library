namespace ProjectLibrary.Models;

/// <summary>
/// A tracked development project folder.
/// Maps to the <c>projects</c> SQLite table.
/// </summary>
public class Project
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FolderPath { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailPath { get; set; }
    public bool IsFavorite { get; set; }

    /// <summary>JSON-encoded array of auto-detected tech stack keys (e.g. "python", "react").</summary>
    public string? DetectedTech { get; set; }

    /// <summary>JSON-encoded array of user-defined tag strings.</summary>
    public string? CustomTags { get; set; }

    public DateTime? LastActive { get; set; }
    public DateTime DateAdded { get; set; }
    public string? LaunchCommand { get; set; }
    public string? Notes { get; set; }
}
