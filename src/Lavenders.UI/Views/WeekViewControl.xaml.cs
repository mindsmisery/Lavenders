using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Lavenders.UI.ViewModels;

namespace Lavenders.UI.Views;

public partial class WeekViewControl : UserControl
{
    private DateTime _lastWheelNavigation = DateTime.MinValue;
    public WeekViewControl()
    {
        InitializeComponent();
        Loaded += WeekViewControl_Loaded;
    }

    private async void WeekViewControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (DataContext is not WeekViewModel viewModel ||
            DateTime.UtcNow - _lastWheelNavigation < TimeSpan.FromMilliseconds(280))
        {
            e.Handled = true;
            return;
        }

        _lastWheelNavigation = DateTime.UtcNow;
        e.Handled = true;

        if (e.Delta > 0)
            await viewModel.PreviousWeekCommand.ExecuteAsync(null);
        else if (e.Delta < 0)
            await viewModel.NextWeekCommand.ExecuteAsync(null);
    }

    private async void WeekViewControl_Loaded(object sender, RoutedEventArgs e)
    {
        // Executes asynchronously AFTER the visual container is safely on screen
        if (DataContext is WeekViewModel viewModel)
        {
            await viewModel.InitializeViewCommand.ExecuteAsync(null);
        }
    }
}
