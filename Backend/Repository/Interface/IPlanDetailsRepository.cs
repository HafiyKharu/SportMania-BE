using SportMania.Models;

namespace SportMania.Repository.Interface;

public interface IPlanDetailsRepository
{
    Task UpsertForPlanAsync(Guid PlanDetailsId, IEnumerable<PlanDetails> incomingDetails);
}