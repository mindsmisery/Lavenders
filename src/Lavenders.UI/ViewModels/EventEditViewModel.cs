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
        private DateTime? _selectedClockTime;
        private DateTime? _selectedEndClockTime;

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
                RebaseTimesOnSelectedDate();
            }
        }

        // Kept as a text adapter for validation and callers that set HH:mm directly.
        public string SelectedTime
        {
            get => SelectedClockTime?.ToString("HH:mm") ?? string.Empty;
            set
            {
                OnPropertyChanged();
                if (TimeSpan.TryParse(value, out var time))
                    SelectedClockTime = SelectedDate.Date.Add(time);
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
                var previousDuration = _selectedClockTime.HasValue && _selectedEndClockTime.HasValue
                    ? _selectedEndClockTime.Value - _selectedClockTime.Value
                    : (TimeSpan?)null;
                _selectedClockTime = value.HasValue
                    ? SelectedDate.Date.Add(value.Value.TimeOfDay)
                    : null;
                if (_selectedClockTime.HasValue && previousDuration > TimeSpan.Zero)
                {
                    _selectedEndClockTime = _selectedClockTime.Value.Add(previousDuration.Value);
                    OnPropertyChanged(nameof(SelectedEndClockTime));
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedTime));
                UpdateDateTimes();
            }
        }

        public DateTime? SelectedEndClockTime
        {
            get => _selectedEndClockTime;
            set
            {
                if (_selectedEndClockTime == value) return;
                _selectedEndClockTime = value.HasValue
                    ? SelectedDate.Date.Add(value.Value.TimeOfDay)
                    : null;
                OnPropertyChanged();
                UpdateDateTimes();
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

        private DateTime _endDateTime;
        public DateTime EndDateTime
        {
            get => _endDateTime;
            private set
            {
                _endDateTime = value;
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

            var now = DateTime.Now;
            var roundedMinutes = (now.Minute / 5) * 5;
            SelectedDate = selectedDate.Date;
            SelectedClockTime = SelectedDate.AddHours(now.Hour).AddMinutes(roundedMinutes);
            SelectedEndClockTime = SelectedClockTime.Value.AddHours(1);
        }

        public EventEditViewModel(Event existingEvent, ILocalizationService? localization = null)
        {
            _localization = localization;
            DeleteCommand = new RelayCommand<Window>(ExecuteDelete);

            _existingEventId = existingEvent.Id;
            Title = existingEvent.Title;
            Description = existingEvent.Description;

            var localStart = existingEvent.StartDateTime.ToLocalTime();
            var localEnd = existingEvent.EndDateTime.ToLocalTime();
            SelectedDate = localStart.Date;
            SelectedClockTime = localStart;
            SelectedEndClockTime = localEnd;
        }

        private void RebaseTimesOnSelectedDate()
        {
            if (_selectedClockTime.HasValue)
                _selectedClockTime = SelectedDate.Date.Add(_selectedClockTime.Value.TimeOfDay);
            if (_selectedEndClockTime.HasValue)
                _selectedEndClockTime = SelectedDate.Date.Add(_selectedEndClockTime.Value.TimeOfDay);
            OnPropertyChanged(nameof(SelectedClockTime));
            OnPropertyChanged(nameof(SelectedEndClockTime));
            UpdateDateTimes();
        }

        private void UpdateDateTimes()
        {
            StartDateTime = _selectedClockTime.HasValue
                ? DateTime.SpecifyKind(SelectedDate.Date.Add(_selectedClockTime.Value.TimeOfDay), DateTimeKind.Local)
                : default;
            EndDateTime = _selectedEndClockTime.HasValue
                ? DateTime.SpecifyKind(SelectedDate.Date.Add(_selectedEndClockTime.Value.TimeOfDay), DateTimeKind.Local)
                : default;
        }

        public Event CreateEvent()
        {
            return new Event
            {
                Id = _existingEventId,
                Title = Title.Trim(),
                Description = Description.Trim(),
                StartDateTime = StartDateTime.ToUniversalTime(),
                EndDateTime = EndDateTime.ToUniversalTime()
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

            if (EndDateTime == default || EndDateTime <= StartDateTime)
            {
                ValidationMessage = Localize("ValidationEndTimeRequired", "End time must be after start time.");
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
