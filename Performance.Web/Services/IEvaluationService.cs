using Performance.Web.DTOs;

namespace Performance.Web.Services;

public interface IEvaluationService
{
    /// <summary>
    /// Saves a behavioral or functional evaluation to the Evaluations table.
    /// </summary>
    /// <returns>True on success, false if an error occurred.</returns>
    Task<bool> SubmitEvaluationAsync(Guid evaluatorId, EvaluationRequest request);

    /// <summary>
    /// Returns all evaluations where the given employee is the TARGET (employee/received view).
    /// </summary>
    Task<IEnumerable<EvaluationDto>> GetReceivedEvaluationsAsync(Guid targetEmployeeId);

    /// <summary>
    /// Returns all evaluations submitted BY the given evaluator (manager/given view).
    /// </summary>
    Task<IEnumerable<EvaluationDto>> GetGivenEvaluationsAsync(Guid evaluatorId);
}
