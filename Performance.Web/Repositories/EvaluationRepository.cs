using Microsoft.EntityFrameworkCore;
using Performance.Web.Data;
using Performance.Web.Models;

namespace Performance.Web.Repositories;

public class EvaluationRepository : Repository<Evaluation>, IEvaluationRepository
{
    public EvaluationRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc />
    public async Task<IEnumerable<Evaluation>> GetByTargetAsync(Guid targetEmployeeId)
    {
        return await _context.Evaluations
            .Include(e => e.Evaluator)
            .Include(e => e.TargetEmployee)
            .Where(e => e.TargetEmployeeId == targetEmployeeId)
            .OrderByDescending(e => e.ObservationDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Evaluation>> GetByEvaluatorAsync(Guid evaluatorId)
    {
        return await _context.Evaluations
            .Include(e => e.Evaluator)
            .Include(e => e.TargetEmployee)
            .Where(e => e.EvaluatorId == evaluatorId)
            .OrderByDescending(e => e.ObservationDate)
            .ToListAsync();
    }
}
