using Performance.Web.Enums;

namespace Performance.Web.DTOs;

/// <summary>
/// Carries the data required to submit an evaluation record.
/// </summary>
public class EvaluationRequest
{
    public Guid TargetEmployeeId { get; set; }
    public EvalType EvalType { get; set; }
    public string FeedbackText { get; set; } = string.Empty;
    public DateTime ObservationDate { get; set; } = DateTime.Today;
}
