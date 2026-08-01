using System.Windows;
using System.Windows.Media;

namespace Lavenders.UI.Services;

public sealed class ThemeService : IThemeService
{
    public void Apply(string theme)
    {
        var dark = theme == "LavenderDark";
        var resources = Application.Current.Resources;

        SetBrush(resources, "LavenderPrimaryBrush", dark ? "#9F95E5" : "#AFA9E8");
        SetBrush(resources, "LavenderDeepBrush", dark ? "#CBC4FF" : "#6F68B5");
        SetBrush(resources, "LavenderAccentBrush", dark ? "#655B8A" : "#C9C5F4");
        SetBrush(resources, "LavenderSoftBrush", dark ? "#292536" : "#FAF9FF");
        SetBrush(resources, "LavenderMistBrush", dark ? "#332E45" : "#F0EEFC");
        SetBrush(resources, "LavenderBorderBrush", dark ? "#4C4562" : "#DDD9F2");
        SetBrush(resources, "LavenderTextBrush", dark ? "#F2EFF9" : "#343149");
        SetBrush(resources, "LavenderMutedTextBrush", dark ? "#B8B1C9" : "#747087");
        SetBrush(resources, "LavenderWeekendBrush", dark ? "#2D283E" : "#F0EBFF");
        SetBrush(resources, "LavenderOutsideMonthBrush", dark ? "#201D2A" : "#F1EFF5");
        SetBrush(resources, "LavenderTodayBrush", dark ? "#3B3453" : "#E8E5FF");
        SetBrush(resources, "LavenderDangerBrush", dark ? "#C9788E" : "#B9657B");
        SetBrush(resources, "LavenderErrorSurfaceBrush", dark ? "#3A252F" : "#FFF2F5");
        SetBrush(resources, "LavenderErrorBorderBrush", dark ? "#815064" : "#E7BCC8");
        SetBrush(resources, "LavenderSuccessBrush", dark ? "#91C7A8" : "#527A64");
        SetBrush(resources, "LavenderSuccessSurfaceBrush", dark ? "#20352B" : "#EFF8F3");
        SetBrush(resources, "LavenderSuccessBorderBrush", dark ? "#47725A" : "#BFDDCB");
        SetBrush(resources, "LavenderButtonHoverBrush", dark ? "#756BB5" : "#9B91DE");
        SetBrush(resources, "LavenderButtonPressedBrush", dark ? "#645A9E" : "#867CCB");
        SetBrush(resources, "LavenderCardBrush", dark ? "#252130" : "#FFFFFF");
        SetBrush(resources, "LavenderInputSurfaceBrush", dark ? "#2B2638" : "#FFFFFF");
        SetBrush(resources, "LavenderEventSurfaceBrush", dark ? "#302A40" : "#FFFFFF");
        SetBrush(resources, "LavenderOnPrimaryBrush", "#FFFFFF");
        SetBrush(resources, "LavenderLoadingOverlayBrush", dark ? "#D91B1823" : "#B8FAF9FF");

        resources["LavenderBackgroundGradient"] = Gradient(
            dark ? ["#181520", "#211D2D", "#29243A"] : ["#FCFBFF", "#F0EEFC", "#E9E7F8"],
            [0d, .55d, 1d], new Point(0, 0), new Point(1, 1));
        resources["LavenderDayGradient"] = Gradient(
            dark ? ["#282331", "#24202D"] : ["#FFFFFF", "#F8F7FE"],
            [0d, 1d], new Point(0, 0), new Point(0, 1));
        resources["LavenderButtonGradient"] = Gradient(
            dark ? ["#655BA4", "#8176BE"] : ["#9D94E0", "#B8B1ED"],
            [0d, 1d], new Point(0, 1), new Point(0, 0));
    }

    private static void SetBrush(ResourceDictionary resources, string key, string value) =>
        resources[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(value));

    private static LinearGradientBrush Gradient(
        IReadOnlyList<string> colors, IReadOnlyList<double> offsets, Point start, Point end)
    {
        var brush = new LinearGradientBrush { StartPoint = start, EndPoint = end };
        for (var index = 0; index < colors.Count; index++)
            brush.GradientStops.Add(new GradientStop(
                (Color)ColorConverter.ConvertFromString(colors[index]), offsets[index]));
        return brush;
    }
}
