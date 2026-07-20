using Performance.Web.Enums;

namespace Performance.Web.DTOs;

/// <summary>
/// A flat DTO for displaying a single evaluation record in the Reports grid.
/// Contains denormalized names so the UI never has to re-query the database.
/// </summary>
public class EvaluationDto
{
    public Guid     Id              { get; set; }
    public string   EvaluatorName   { get; set; } = string.Empty;
    public Guid     EvaluatorId     { get; set; }
    public string   TargetName      { get; set; } = string.Empty;
    public Guid     TargetEmployeeId { get; set; }
    public EvalType EvalType        { get; set; }

    /// <summary>Localised display label for EvalType.</summary>
    public string EvalTypeLabel => EvalType == EvalType.Behavioral ? "رفتاری" : "عملکردی";

    public string   FeedbackText    { get; set; } = string.Empty;

    /// <summary>The Gregorian date when the behaviour/performance was observed.</summary>
    public DateTime ObservationDate { get; set; }

    /// <summary>
    /// Shamsi (Jalali) representation of ObservationDate, computed on the fly.
    /// </summary>
    public string ObservationDateShamsi
    {
        get
        {
            var pc = new System.Globalization.PersianCalendar();
            return $"{pc.GetYear(ObservationDate)}/{pc.GetMonth(ObservationDate):D2}/{pc.GetDayOfMonth(ObservationDate):D2}";
        }
    }

    public DateTime CreatedAt { get; set; }
}
