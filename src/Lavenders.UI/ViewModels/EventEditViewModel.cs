using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Lavenders.Core.Models;
using Lavenders.UI;
using Lavenders.UI.Services;

namespace Lavenders.UI.ViewModels
{
    public class EventEditViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly int _existingEventId = 0;
        private readonly ILocalizationService? _localization;

        public bool IsDeleted { get; private set; } = false;
        public Visibility DeleteButtonVisibility => _existingEventId > 0 ? Visibility.Visible : Visibility.Collapsed;

        public IRelayCommand DeleteCommand { get; }

        private string _title = string.Empty;
        private string _description = string.Empty;
        private string _validationMessage = string.Empty;
        private bool _validationAttempted;

        private DateTime _selectedDate;
        private string _selectedTime = string.Empty;
        private DateTime? _selectedClockTime;
        private bool _isUpdatingClockTime;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
                if (!string.IsNullOrWhiteSpace(value)) ValidationMessage = string.Empty;
            }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                UpdateStartDateTime();
            }
        }

        public string SelectedTime
        {
            get => _selectedTime;
            set
            {
                _selectedTime = value;
                OnPropertyChanged();
                UpdateStartDateTime();
                UpdateClockTime();
            }
        }

        public string ValidationMessage
        {
            get => _validationMessage;
            private set
            {
                _validationMessage = value;
                OnPropertyChanged();
            }
        }

        public DateTime? SelectedClockTime
        {
            get => _selectedClockTime;
            set
            {
                if (_selectedClockTime == value) return;
                _selectedClockTime = value;
                OnPropertyChanged();

                if (!_isUpdatingClockTime && value.HasValue)
                    SelectedTime = value.Value.ToString("HH:mm");
            }
        }

        private DateTime _startDateTime;
        public DateTime StartDateTime
        {
            get => _startDateTime;
            private set
            {
                _startDateTime = value;
                OnPropertyChanged();
            }
        }

        public EventEditViewModel()
            : this(DateTime.Now.Date)
        {
        }

        public EventEditViewModel(DateTime selectedDate, ILocalizationService? localization = null)
        {
            _localization = localization;
            DeleteCommand = new RelayCommand<Window>(ExecuteDelete);

            SelectedDate = selectedDate.Date;

            var now = DateTime.Now;
            var roundedMinutes = (now.Minute / 5) * 5;
            SelectedTime = $"{now.Hour:D2}:{roundedMinutes:D2}";

            UpdateStartDateTime();
        }

        public EventEditViewModel(Event existingEvent, ILocalizationService? localization = null)
        {
            _localization = localization;
            DeleteCommand = new RelayCommand<Window>(ExecuteDelete);

            _existingEventId = existingEvent.Id;
            Title = existingEvent.Title;
            Description = existingEvent.Description;

            var localStart = existingEvent.StartDateTime.ToLocalTime();
            SelectedDate = localStart.Date;
            SelectedTime = $"{localStart.Hour:D2}:{localStart.Minute:D2}";

            UpdateStartDateTime();
        }

        private void UpdateStartDateTime()
        {
            if (string.IsNullOrWhiteSpace(SelectedTime)) return;

            var parts = SelectedTime.Split(':');
            if (parts.Length != 2) return;

            if (!int.TryParse(parts[0], out int hour) ||
                !int.TryParse(parts[1], out int minute) ||
                hour is < 0 or > 23 || minute is < 0 or > 59)
                return;

            StartDateTime = new DateTime(
                SelectedDate.Year,
                SelectedDate.Month,
                SelectedDate.Day,
                hour,
                minute,
                0,
                DateTimeKind.Local);
        }

        private void UpdateClockTime()
        {
            if (_isUpdatingClockTime) return;

            var parts = SelectedTime.Split(':');
            if (parts.Length != 2 ||
                !int.TryParse(parts[0], out var hour) ||
                !int.TryParse(parts[1], out var minute))
                return;

            _isUpdatingClockTime = true;
            _selectedClockTime = SelectedDate.Date.AddHours(hour).AddMinutes(minute);
            OnPropertyChanged(nameof(SelectedClockTime));
            _isUpdatingClockTime = false;
        }

        public Event CreateEvent()
        {
            return new Event
            {
                Id = _existingEventId,
                Title = Title.Trim(),
                Description = Description.Trim(),
                StartDateTime = StartDateTime.ToUniversalTime(),
                EndDateTime = StartDateTime.AddHours(1).ToUniversalTime()
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ExecuteDelete(Window? window)
        {
            var confirmation = new DeleteConfirmationDialog
            {
                Owner = window
            };

            if (confirmation.ShowDialog() == true)
            {
                IsDeleted = true;
                if (window != null) window.DialogResult = true;
            }
        }
        public bool Validate()
        {
            _validationAttempted = true;
            OnPropertyChanged(nameof(Title));

            if (string.IsNullOrWhiteSpace(Title))
            {
                ValidationMessage = Localize("ValidationTitleRequired", "Anna tapahtumalle otsikko.");
                OnPropertyChanged(nameof(Title));
                return false;
            }

            if (SelectedDate == default)
            {
                ValidationMessage = Localize("ValidationDateRequired", "Valitse tapahtumalle päivämäärä.");
                return false;
            }

            if (StartDateTime == default)
            {
                ValidationMessage = Localize("ValidationTimeRequired", "Valitse tapahtumalle kelvollinen aika.");
                return false;
            }

            ValidationMessage = string.Empty;
            return true;
        }

        public string Error => string.Empty;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Title):
                        if (_validationAttempted && string.IsNullOrWhiteSpace(Title))
                            return Localize("ValidationTitleRequired", "Anna tapahtumalle otsikko.");
                        break;

                    case nameof(SelectedDate):
                        if (SelectedDate == default)
                            return Localize("ValidationDateRequired", "Valitse päivämäärä.");
                        break;

                    case nameof(SelectedTime):
                        if (string.IsNullOrWhiteSpace(SelectedTime))
                            return Localize("ValidationTimeRequired", "Valitse aika.");
                        break;
                }

                return string.Empty;
            }
        }

        private string Localize(string key, string fallback)
        {
            if (_localization is null) return fallback;
            var value = _localization.Get(key);
            return value == key ? fallback : value;
        }

    }
}
