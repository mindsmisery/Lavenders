using System.Windows;
using Lavenders.UI.ViewModels;

namespace Lavenders.UI.Services;

public interface IDialogService
{
    Core.Models.Event? ShowEventEditDialog();
    Core.Models.Event? ShowEventEditDialog(DateTime selectedDate);
    (Core.Models.Event? Event, bool DeleteRequested) ShowEventEditDialog(Core.Models.Event existingEvent);
}

public class DialogService : IDialogService
{
    private readonly ILocalizationService _localization;

    public DialogService(ILocalizationService localization) => _localization = localization;

    public Core.Models.Event? ShowEventEditDialog()
    {
        var viewModel = new EventEditViewModel(DateTime.Now.Date, _localization);
        var dialog = new EventEditDialog(viewModel);

        return dialog.ShowDialog() == true ? viewModel.CreateEvent() : null;
    }

    public Core.Models.Event? ShowEventEditDialog(DateTime selectedDate)
    {
        var viewModel = new EventEditViewModel(selectedDate, _localization);
        var dialog = new EventEditDialog(viewModel);

        return dialog.ShowDialog() == true ? viewModel.CreateEvent() : null;
    }

    public (Core.Models.Event? Event, bool DeleteRequested) ShowEventEditDialog(Core.Models.Event existingEvent)
    {
        var viewModel = new EventEditViewModel(existingEvent, _localization);
        var dialog = new EventEditDialog(viewModel);

        if (dialog.ShowDialog() == true)
            return (viewModel.CreateEvent(), viewModel.IsDeleted);

        return (null, false);
    }
}
