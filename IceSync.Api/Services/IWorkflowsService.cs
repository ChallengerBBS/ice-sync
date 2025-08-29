using IceSync.Api.Domain.Entities;

namespace IceSync.Api.Services
{
    public interface IWorkflowsService
    {
        Task<IEnumerable<Workflow>> GetAllWorkflowsAsync();
        Task<Workflow?> GetWorkflowByIdAsync(int workflowId);
        Task<Workflow> CreateWorkflowAsync(Workflow workflow);
        Task<Workflow> UpdateWorkflowAsync(Workflow workflow);
        Task DeleteWorkflowAsync(int workflowId);
        Task<int> SaveChangesAsync();
        Task SyncWorkflowsFromApiAsync(IEnumerable<Workflow> workflowsToInsert, IEnumerable<Workflow> workflowsToDelete, IEnumerable<Workflow> workflowsToUpdate);
    }
}
