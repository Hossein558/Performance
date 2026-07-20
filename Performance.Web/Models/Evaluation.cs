using Performance.Web.Enums;

namespace Performance.Web.Models;

/// <summary>
/// Stores a single evaluation record submitted by a manager for a subordinate.
/// </summary>
public class Evaluation
{
    public Guid Id { get; set; }

    /// <summary>The manager who submitted this evaluation.</summary>
    public Guid EvaluatorId { get; set; }
    public Employee Evaluator { get; set; } = null!;

    /// <summary>The employee being evaluated.</summary>
    public Guid TargetEmployeeId { get; set; }
    public Employee TargetEmployee { get; set; } = null!;

    public EvalType EvalType { get; set; }
    public string FeedbackText { get; set; } = string.Empty;

    /// <summary>The date the behavior/performance was actually observed.</summary>
    public DateTime ObservationDate { get; set; }

    /// <summary>UTC timestamp of when this record was inserted.</summary>
    public DateTime CreatedAt { get; set; }
}
