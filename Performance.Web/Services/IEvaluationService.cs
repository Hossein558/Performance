using Performance.Web.DTOs;

namespace Performance.Web.Services;

public interface IEvaluationService
{
    /// <summary>
    /// Saves a behavioral or functional evaluation to the Evaluations table.
    /// </summary>
    /// <returns>True on success, false if an error occurred.</returns>
    Task<bool> SubmitEvaluationAsync(Guid evaluatorId, EvaluationRequest request);
}
