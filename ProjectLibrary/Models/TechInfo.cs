namespace ProjectLibrary.Models;

/// <summary>
/// Metadata for a detected tech stack: stable key, display name, and hex badge color.
/// </summary>
public readonly record struct TechInfo(string Key, string DisplayName, string ColorHex);
