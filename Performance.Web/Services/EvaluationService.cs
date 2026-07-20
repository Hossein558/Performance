using Performance.Web.DTOs;
using Performance.Web.Models;
using Performance.Web.Repositories;

namespace Performance.Web.Services;

public class EvaluationService : IEvaluationService
{
    private readonly IEvaluationRepository _evaluationRepository;
    private readonly ILogger<EvaluationService> _logger;

    public EvaluationService(
        IEvaluationRepository evaluationRepository,
        ILogger<EvaluationService> logger)
    {
        _evaluationRepository = evaluationRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> SubmitEvaluationAsync(Guid evaluatorId, EvaluationRequest request)
    {
        try
        {
            var evaluation = new Evaluation
            {
                Id               = Guid.NewGuid(),
                EvaluatorId      = evaluatorId,
                TargetEmployeeId = request.TargetEmployeeId,
                EvalType         = request.EvalType,
                FeedbackText     = request.FeedbackText.Trim(),
                ObservationDate  = request.ObservationDate,
                CreatedAt        = DateTime.UtcNow
            };

            await _evaluationRepository.AddAsync(evaluation);
            await _evaluationRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Evaluation saved — Id: {EvalId}, EvaluatorId: {EvaluatorId}, " +
                "TargetId: {TargetId}, Type: {EvalType}",
                evaluation.Id, evaluatorId, request.TargetEmployeeId, request.EvalType);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to save evaluation for EvaluatorId: {EvaluatorId}, TargetId: {TargetId}",
                evaluatorId, request.TargetEmployeeId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EvaluationDto>> GetReceivedEvaluationsAsync(Guid targetEmployeeId)
    {
        var evaluations = await _evaluationRepository.GetByTargetAsync(targetEmployeeId);
        return evaluations.Select(MapToDto);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EvaluationDto>> GetGivenEvaluationsAsync(Guid evaluatorId)
    {
        var evaluations = await _evaluationRepository.GetByEvaluatorAsync(evaluatorId);
        return evaluations.Select(MapToDto);
    }

    // ── Private Helpers ──────────────────────────────────────────────────────
    private static EvaluationDto MapToDto(Evaluation e) => new()
    {
        Id                     = e.Id,
        EvaluatorId            = e.EvaluatorId,
        EvaluatorName          = $"{e.Evaluator.FirstName} {e.Evaluator.LastName}".Trim(),
        EvaluatorPersonnelCode = e.Evaluator.PersonnelCode,
        TargetEmployeeId       = e.TargetEmployeeId,
        TargetName             = $"{e.TargetEmployee.FirstName} {e.TargetEmployee.LastName}".Trim(),
        TargetPersonnelCode    = e.TargetEmployee.PersonnelCode,
        EvalType               = e.EvalType,
        FeedbackText           = e.FeedbackText,
        ObservationDate        = e.ObservationDate,
        CreatedAt              = e.CreatedAt
    };
}
