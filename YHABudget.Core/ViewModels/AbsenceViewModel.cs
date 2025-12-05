using System.Collections.ObjectModel;
using System.Windows.Input;
using YHABudget.Core.Commands;
using YHABudget.Core.Helpers;
using YHABudget.Core.MVVM;
using YHABudget.Core.Services;
using YHABudget.Data.Models;
using YHABudget.Data.Services;

namespace YHABudget.Core.ViewModels;

public class AbsenceViewModel : ViewModelBase
{
    private readonly IAbsenceService _absenceService;
    private readonly IDialogService _dialogService;
    private readonly ISalarySettingsService _salarySettingsService;

    private ObservableCollection<Absence> _absences;
    private DateTime? _selectedMonth;
    private ObservableCollection<MonthOption> _availableMonths;

    public AbsenceViewModel(
        IAbsenceService absenceService, 
        IDialogService dialogService,
        ISalarySettingsService salarySettingsService)
    {
        _absenceService = absenceService;
        _dialogService = dialogService;
        _salarySettingsService = salarySettingsService;

        _absences = new ObservableCollection<Absence>();
        _selectedMonth = null;
        _availableMonths = new ObservableCollection<MonthOption>();

        InitializeMonths();

        LoadDataCommand = new RelayCommand(() => LoadData());
        ClearFilterCommand = new RelayCommand(() => ClearFilter());
        AddAbsenceCommand = new RelayCommand(() => AddAbsence());
        EditAbsenceCommand = new RelayCommand<Absence>((absence) => EditAbsence(absence));
        DeleteAbsenceCommand = new RelayCommand<Absence>((absence) => DeleteAbsence(absence));

        LoadData();
    }

    public ObservableCollection<Absence> Absences
    {
        get => _absences;
        private set => SetProperty(ref _absences, value);
    }

    public ObservableCollection<MonthOption> AvailableMonths
    {
        get => _availableMonths;
        private set => SetProperty(ref _availableMonths, value);
    }

    public DateTime? SelectedMonth
    {
        get => _selectedMonth;
        set
        {
            if (SetProperty(ref _selectedMonth, value))
            {
                LoadData();
            }
        }
    }

    public ICommand LoadDataCommand { get; }
    public ICommand ClearFilterCommand { get; }
    public ICommand AddAbsenceCommand { get; }
    public ICommand EditAbsenceCommand { get; }
    public ICommand DeleteAbsenceCommand { get; }

    private void InitializeMonths()
    {
        // Get months with absences from the service
        var monthsWithAbsences = _absenceService.GetMonthsWithAbsences().ToList();

        // Add current month if not present
        var currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        if (!monthsWithAbsences.Contains(currentMonth))
        {
            monthsWithAbsences.Add(currentMonth);
            monthsWithAbsences = monthsWithAbsences.OrderByDescending(d => d).ToList();
        }

        AvailableMonths.Clear();
        
        // Add "Show All" option
        AvailableMonths.Add(new MonthOption
        {
            Date = null,
            DisplayText = "Visa alla"
        });
        
        foreach (var month in monthsWithAbsences)
        {
            var isCurrentMonth = month == currentMonth;
            var displayText = DateFormatHelper.FormatMonthYear(month);
            AvailableMonths.Add(new MonthOption
            {
                Date = month,
                DisplayText = isCurrentMonth ? $"★ {displayText}" : displayText
            });
        }

        // Set selected month to null to show all by default
        if (!_selectedMonth.HasValue || !monthsWithAbsences.Contains(_selectedMonth.Value))
        {
            _selectedMonth = null;
        }
    }

    private void LoadData()
    {
        var absences = _selectedMonth.HasValue 
            ? _absenceService.GetAbsencesForMonth(_selectedMonth.Value)
            : _absenceService.GetAllAbsences();
            
        Absences.Clear();
        foreach (var absence in absences)
        {
            Absences.Add(absence);
        }
    }

    private void ClearFilter()
    {
        SelectedMonth = null;
    }

    private void AddAbsence()
    {
        var result = _dialogService.ShowAbsenceDialog(null, _absenceService);
        if (result != null)
        {
            var added = _absenceService.AddAbsence(result);
            Absences.Add(added);
            InitializeMonths();
        }
    }

    private void EditAbsence(Absence? absence)
    {
        if (absence == null) return;

        var result = _dialogService.ShowAbsenceDialog(absence, _absenceService);
        if (result != null)
        {
            _absenceService.UpdateAbsence(result);

            // Refresh the item in the collection
            var index = Absences.IndexOf(absence);
            if (index >= 0)
            {
                var updated = _absenceService.GetAbsenceById(result.Id);
                if (updated != null)
                {
                    Absences[index] = updated;
                }
            }
        }
    }

    private void DeleteAbsence(Absence? absence)
    {
        if (absence == null || absence.Id == 0) return;

        _absenceService.DeleteAbsence(absence.Id);
        Absences.Remove(absence);
        InitializeMonths();
    }

    public class MonthOption
    {
        public DateTime? Date { get; set; }
        public string DisplayText { get; set; } = string.Empty;
    }
}
