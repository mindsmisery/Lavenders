using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Lavenders.UI.ViewModels;

namespace Lavenders.UI;

public partial class EventEditDialog : Window
{
    public EventEditDialog(EventEditViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is EventEditViewModel viewModel && !viewModel.Validate())
            return;

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void EventDatePicker_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not DatePicker picker) return;

        picker.ApplyTemplate();
        if (picker.Template.FindName("PART_Button", picker) is Button calendarButton)
        {
            // Preserve the native popup behavior and hit target, but hide its artwork.
            calendarButton.Visibility = Visibility.Collapsed;
            calendarButton.FocusVisualStyle = null;
        }
    }

    private void OpenDatePicker_Click(object sender, RoutedEventArgs e)
    {
        EventDatePicker.IsDropDownOpen = true;
    }

    private void EventDatePicker_CalendarOpened(object? sender, RoutedEventArgs e)
    {
        if (sender is not DatePicker picker) return;

        picker.Dispatcher.BeginInvoke(() =>
        {
            picker.ApplyTemplate();
            if (picker.Template.FindName("PART_Popup", picker) is not Popup popup)
                return;

            var calendar = FindVisualChild<Calendar>(popup.Child);
            if (calendar is null) return;

            calendar.CalendarDayButtonStyle =
                (Style)FindResource("LavenderCalendarDayButton");
            calendar.UpdateLayout();
            HideDatesOutsideDisplayedMonth(calendar);
        }, DispatcherPriority.Loaded);
    }

    private static void HideDatesOutsideDisplayedMonth(Calendar calendar)
    {
        foreach (var dayButton in FindVisualChildren<CalendarDayButton>(calendar))
        {
            dayButton.Visibility = dayButton.DataContext is DateTime date &&
                                   date.Month == calendar.DisplayDate.Month &&
                                   date.Year == calendar.DisplayDate.Year
                ? Visibility.Visible
                : Visibility.Hidden;
        }
    }

    private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
    {
        return FindVisualChildren<T>(parent).FirstOrDefault();
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject? parent)
        where T : DependencyObject
    {
        if (parent is null) yield break;

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match) yield return match;

            foreach (var descendant in FindVisualChildren<T>(child))
                yield return descendant;
        }
    }

    private void EventTimePicker_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Control picker) return;

        picker.ApplyTemplate();
        ApplyPickerIconTheme(picker);
        if (picker.Template.FindName("PART_Popup", picker) is Popup popup)
        {
            popup.Opened -= TimePickerPopup_Opened;
            popup.Opened += TimePickerPopup_Opened;
        }
    }

    private void ApplyPickerIconTheme(DependencyObject element)
    {
        var iconBrush = (Brush)FindResource("LavenderDeepBrush");
        foreach (var child in FindVisualChildren<DependencyObject>(element))
        {
            if (child.GetType().Name == "PackIcon" && child is Control icon)
                icon.Foreground = iconBrush;
        }
    }

    private void TimePickerPopup_Opened(object? sender, EventArgs e)
    {
        if (sender is not Popup popup) return;

        popup.Dispatcher.BeginInvoke(() =>
        {
            var popupBackground = (Brush)FindResource("LavenderInputSurfaceBrush");
            switch (popup.Child)
            {
                case Border border:
                    border.Background = popupBackground;
                    break;
                case Panel panel:
                    panel.Background = popupBackground;
                    break;
                case Control control:
                    control.Background = popupBackground;
                    break;
            }

            ApplyClockBackground(popup.Child, popupBackground);
        }, DispatcherPriority.Loaded);
    }

    private static void ApplyClockBackground(DependencyObject? element, Brush background)
    {
        if (element is null) return;

        if (element.GetType().Name == "Clock" && element is Control clock)
            clock.Background = background;

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(element); index++)
            ApplyClockBackground(VisualTreeHelper.GetChild(element, index), background);
    }
}
