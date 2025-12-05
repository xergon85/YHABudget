using System.Collections.ObjectModel;
using System.Windows.Input;
using YHABudget.Core.Commands;
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
    private DateTime _selectedMonth;

    public AbsenceViewModel(
        IAbsenceService absenceService, 
        IDialogService dialogService,
        ISalarySettingsService salarySettingsService)
    {
        _absenceService = absenceService;
        _dialogService = dialogService;
        _salarySettingsService = salarySettingsService;

        _absences = new ObservableCollection<Absence>();
        _selectedMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        LoadDataCommand = new RelayCommand(() => LoadData());
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

    public DateTime SelectedMonth
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
    public ICommand AddAbsenceCommand { get; }
    public ICommand EditAbsenceCommand { get; }
    public ICommand DeleteAbsenceCommand { get; }

    private void LoadData()
    {
        var absences = _absenceService.GetAbsencesForMonth(SelectedMonth);
        Absences.Clear();
        foreach (var absence in absences)
        {
            Absences.Add(absence);
        }
    }

    private void AddAbsence()
    {
        var result = _dialogService.ShowAbsenceDialog(null, _absenceService);
        if (result != null)
        {
            var added = _absenceService.AddAbsence(result);
            Absences.Add(added);
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
    }
}
