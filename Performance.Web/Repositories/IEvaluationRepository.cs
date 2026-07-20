using Performance.Web.Models;

namespace Performance.Web.Repositories;

/// <summary>Evaluation-specific repository operations.</summary>
public interface IEvaluationRepository : IRepository<Evaluation>
{
    /// <summary>
    /// Returns all evaluations where the given employee is the TARGET (received feedback).
    /// Eagerly loads the Evaluator navigation so callers get the evaluator's name.
    /// </summary>
    Task<IEnumerable<Evaluation>> GetByTargetAsync(Guid targetEmployeeId);

    /// <summary>
    /// Returns all evaluations submitted BY the given evaluator (manager view).
    /// Eagerly loads the TargetEmployee navigation so callers get the target's name.
    /// </summary>
    Task<IEnumerable<Evaluation>> GetByEvaluatorAsync(Guid evaluatorId);
}
