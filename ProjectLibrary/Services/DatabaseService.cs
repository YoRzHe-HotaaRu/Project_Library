using System.Data;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;
using ProjectLibrary.Models;

namespace ProjectLibrary.Services;

/// <summary>
/// Owns the SQLite connection and schema. Lazily resolves the DB path under
/// <c>%APPDATA%\ProjectLibrary\library.db</c> and creates the schema on first run.
/// </summary>
public static class DatabaseService
{
    private static readonly Lazy<string> _dbPath = new(ResolveDbPath);

    static DatabaseService()
    {
        // Let Dapper map snake_case columns (folder_path) to PascalCase properties (FolderPath).
        SqlMapper.AddTypeHandler(new SqliteDateTimeHandler());
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public static string DbPath => _dbPath.Value;

    /// <summary>Creates the schema if it does not already exist. Safe to call on every launch.</summary>
    public static void Initialize()
    {
        using var connection = OpenConnection();
        connection.Execute(SchemaSql);
        connection.Execute("PRAGMA foreign_keys = ON;");
    }

    /// <summary>Opens a new connection to the library database. Caller owns the disposal.</summary>
    public static SqliteConnection OpenConnection() =>
        new($"Data Source={DbPath}");

    // ============================== Project CRUD ==============================

    /// <summary>Loads all projects, newest first by default.</summary>
    public static List<Project> GetAllProjects()
    {
        using var conn = OpenConnection();
        return conn.Query<Project>(@"
            SELECT id, name, folder_path, description, thumbnail_path,
                   is_favorite, detected_tech, custom_tags,
                   last_active, date_added, launch_command, notes
            FROM projects
            ORDER BY date_added DESC
        ").ToList();
    }

    /// <summary>Inserts a project and returns the new row id.</summary>
    public static long InsertProject(Project project)
    {
        using var conn = OpenConnection();
        return conn.ExecuteScalar<long>(@"
            INSERT INTO projects
                (name, folder_path, description, thumbnail_path, is_favorite,
                 detected_tech, custom_tags, last_active, launch_command, notes)
            VALUES
                (@Name, @FolderPath, @Description, @ThumbnailPath, @IsFavorite,
                 @DetectedTech, @CustomTags, @LastActive, @LaunchCommand, @Notes);
            SELECT last_insert_rowid();
        ", project);
    }

    public static void UpdateProject(Project project)
    {
        using var conn = OpenConnection();
        conn.Execute(@"
            UPDATE projects SET
                name           = @Name,
                folder_path    = @FolderPath,
                description    = @Description,
                thumbnail_path = @ThumbnailPath,
                is_favorite    = @IsFavorite,
                detected_tech  = @DetectedTech,
                custom_tags    = @CustomTags,
                last_active    = @LastActive,
                launch_command = @LaunchCommand,
                notes          = @Notes
            WHERE id = @Id
        ", project);
    }

    public static void DeleteProject(long id)
    {
        using var conn = OpenConnection();
        conn.Execute("DELETE FROM projects WHERE id = @Id", new { Id = id });
    }

    public static bool ToggleFavorite(long id)
    {
        using var conn = OpenConnection();
        return conn.ExecuteScalar<bool>(@"
            UPDATE projects
               SET is_favorite = NOT is_favorite
             WHERE id = @Id;
            SELECT is_favorite FROM projects WHERE id = @Id;
        ", new { Id = id });
    }

    public static void LogActivity(long projectId, string action)
    {
        using var conn = OpenConnection();
        conn.Execute(
            "INSERT INTO recent_activity (project_id, action) VALUES (@ProjectId, @Action)",
            new { ProjectId = projectId, Action = action });
    }

    // ============================== Path resolution ==============================

    private static string ResolveDbPath()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ProjectLibrary");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "library.db");
    }

    private const string SchemaSql = @"
CREATE TABLE IF NOT EXISTS projects (
    id              INTEGER PRIMARY KEY AUTOINCREMENT,
    name            TEXT    NOT NULL,
    folder_path     TEXT    NOT NULL UNIQUE,
    description     TEXT,
    thumbnail_path  TEXT,
    is_favorite     INTEGER NOT NULL DEFAULT 0,
    detected_tech   TEXT,
    custom_tags     TEXT,
    last_active     TEXT,
    date_added      TEXT    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    launch_command  TEXT,
    notes           TEXT
);

CREATE TABLE IF NOT EXISTS recent_activity (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    project_id  INTEGER,
    action      TEXT,
    timestamp   TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS tags (
    id     INTEGER PRIMARY KEY AUTOINCREMENT,
    name   TEXT NOT NULL UNIQUE,
    color  TEXT
);

CREATE TABLE IF NOT EXISTS project_tags (
    project_id  INTEGER,
    tag_id      INTEGER,
    PRIMARY KEY (project_id, tag_id),
    FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE,
    FOREIGN KEY (tag_id)     REFERENCES tags(id)     ON DELETE CASCADE
);
";
}

/// <summary>
/// Custom Dapper type handler so SQLite TEXT datetimes round-trip cleanly into
/// nullable <see cref="DateTime"/> properties.
/// </summary>
internal sealed class SqliteDateTimeHandler : SqlMapper.TypeHandler<DateTime>
{
    public override void SetValue(IDbDataParameter parameter, DateTime value) =>
        parameter.Value = value.ToString("yyyy-MM-dd HH:mm:ss");

    public override DateTime Parse(object value) =>
        DateTime.TryParse(value.ToString(), out var dt) ? dt : DateTime.MinValue;
}

