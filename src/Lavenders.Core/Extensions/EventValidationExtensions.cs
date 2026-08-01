using Lavenders.Core.Models;
namespace Lavenders.Core.Extensions;

public static class EventValidator
{
    public static bool Validate(Event evt, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(evt.Title))
        {
            errorMessage = "Title cannot be empty";
            return false;
        }

        if (evt.StartDateTime >= evt.EndDateTime)
        {
            errorMessage = "Start time must be before end time";
            return false;
        }

        return true;
    }
}