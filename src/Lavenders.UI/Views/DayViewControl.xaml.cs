using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Lavenders.UI.ViewModels;

namespace Lavenders.UI.Views;

public partial class DayViewControl : UserControl
{
    public DayViewControl()
    {
        InitializeComponent();
    }

    private void DaySurface_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (FindAncestor<ButtonBase>(e.OriginalSource as DependencyObject) is not null ||
            FindAncestor<ScrollBar>(e.OriginalSource as DependencyObject) is not null)
            return;

        if (DataContext is not DayViewModel day ||
            FindAncestor<WeekViewControl>(this)?.DataContext is not WeekViewModel week)
            return;

        if (week.AddEventForDateCommand.CanExecute(day.Date))
            week.AddEventForDateCommand.Execute(day.Date);

        e.Handled = true;
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T match) return match;
            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}
