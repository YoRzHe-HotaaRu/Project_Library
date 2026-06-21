using System.Windows.Controls;
using System.Windows.Input;
using ProjectLibrary.ViewModels;

namespace ProjectLibrary.Views;

public partial class AddProjectDialog : UserControl
{
    public AddProjectDialog()
    {
        InitializeComponent();
    }

    /// <summary>Pressing Enter in the tag input adds the tag (without needing to click + ADD).</summary>
    private void TagInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is AddProjectViewModel vm)
        {
            vm.AddTagCommand.Execute(null);
            e.Handled = true;
        }
    }
}
