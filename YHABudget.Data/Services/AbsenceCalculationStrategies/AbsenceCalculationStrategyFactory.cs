using YHABudget.Data.Enums;

namespace YHABudget.Data.Services.AbsenceCalculationStrategies;

public class AbsenceCalculationStrategyFactory
{
    private readonly Dictionary<AbsenceType, IAbsenceCalculationStrategy> _strategies;

    public AbsenceCalculationStrategyFactory()
    {
        _strategies = new Dictionary<AbsenceType, IAbsenceCalculationStrategy>
        {
            { AbsenceType.Sick, new SickLeaveCalculationStrategy() },
            { AbsenceType.VAB, new VABCalculationStrategy() }
        };
    }

    public IAbsenceCalculationStrategy GetStrategy(AbsenceType absenceType)
    {
        if (_strategies.TryGetValue(absenceType, out var strategy))
        {
            return strategy;
        }

        throw new ArgumentException($"No calculation strategy found for absence type: {absenceType}");
    }
}
