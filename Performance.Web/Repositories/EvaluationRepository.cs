using Performance.Web.Data;
using Performance.Web.Models;

namespace Performance.Web.Repositories;

public class EvaluationRepository : Repository<Evaluation>, IEvaluationRepository
{
    public EvaluationRepository(AppDbContext context) : base(context) { }
}
