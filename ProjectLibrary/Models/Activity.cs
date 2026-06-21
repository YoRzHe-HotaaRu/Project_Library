namespace ProjectLibrary.Models;

/// <summary>
/// A recent-activity entry for a project (opened, launched, viewed).
/// Maps to the <c>recent_activity</c> SQLite table.
/// </summary>
public class Activity
{
    public long Id { get; set; }
    public long ProjectId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
