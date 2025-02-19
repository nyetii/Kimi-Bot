using Discord.Interactions;

namespace Kimi.Repository.Dtos;

public record PeriodDto
{
    public DateTime Start { get; init; }
    public DateTime End { get; init; }

    public PeriodType Type { get; init; }

    public PeriodDto(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
        Type = PeriodType.Specific;
    }

    public PeriodDto(PeriodType periodType)
    {
        Start = periodType switch
        {
            PeriodType.Daily => DateTime.UtcNow.Date,
            PeriodType.Weekly => DateTime.UtcNow.Date.AddDays(-7),
            PeriodType.Monthly => DateTime.UtcNow.Date.AddMonths(-1),
            PeriodType.Yearly => DateTime.UtcNow.Date.AddYears(-1),
            PeriodType.YearToDate => DateTime.UtcNow.Date.AddDays(-DateTime.UtcNow.DayOfYear),
            PeriodType.Specific => throw new InvalidOperationException("Specified period type is not supported."),
            _ => DateTime.MinValue
        };

        End = DateTime.UtcNow.Date;

        Type = periodType;
    }

    public PeriodDto(DateTime start, DateTime end, PeriodType periodType) : this(start, end)
    {
        Type = periodType;
    }

    public override string ToString()
    {
        return Type switch
        {
            PeriodType.Daily => "Daily",
            PeriodType.Weekly => "Weekly",
            PeriodType.Monthly => "Monthly",
            PeriodType.Yearly => "Yearly",
            PeriodType.YearToDate => "YTD",
            PeriodType.Specific => Start.Date == End.Date
                ? $"{Start:dd/MM/yyyy}"
                : $"{Start:dd/MM/yyyy}–{End:dd/MM/yyyy}",
            _ => "All time"
        };
    }
}

public enum PeriodType
{
    [ChoiceDisplay("all-time")] AllTime = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Yearly = 4,
    [ChoiceDisplay("ytd")] YearToDate = 5,
    [Hide] Specific = 6
}