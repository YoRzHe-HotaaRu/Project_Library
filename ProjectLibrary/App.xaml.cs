using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Data.Sqlite;
using ProjectLibrary.Services;
using ProjectLibrary.ViewModels;

namespace ProjectLibrary;

/// <summary>
/// Application entry point. Initializes SQLite, creates the database schema,
/// and shows the main window wired to its view model.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Surface any unhandled UI-thread exception so we can see what crashed
        // instead of the app silently disappearing.
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        base.OnStartup(e);

        SQLitePCL.Batteries_V2.Init();
        DatabaseService.Initialize();

        var mainWindow = new MainWindow
        {
            DataContext = new MainViewModel()
        };
        mainWindow.Show();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var msg = $"Unhandled exception:\n\n{e.Exception.GetType().Name}: {e.Exception.Message}\n\n{e.Exception.StackTrace}";
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "projectlibrary-crash.log"), msg);
        MessageBox.Show(msg, "Project Library — crash", MessageBoxButton.OK, MessageBoxImage.Error);
        // Keep the app alive so the user can see the error and we can debug.
        e.Handled = true;
    }
}
