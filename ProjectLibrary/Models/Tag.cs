namespace ProjectLibrary.Models;

/// <summary>
/// A user-defined or auto-generated tag.
/// Maps to the <c>tags</c> SQLite table.
/// </summary>
public class Tag
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
}
