using System.Windows.Controls;
using System.Windows.Input;
using ProjectLibrary.ViewModels;

namespace ProjectLibrary.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
    }

    private void HeroCard_MouseEnter(object sender, MouseEventArgs e) =>
        (DataContext as HomeViewModel)?.PauseCarousel();

    private void HeroCard_MouseLeave(object sender, MouseEventArgs e) =>
        (DataContext as HomeViewModel)?.ResumeCarousel();
}
