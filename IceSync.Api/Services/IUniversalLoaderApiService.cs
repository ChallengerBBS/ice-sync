using IceSync.Api.Domain.DTOs;

namespace IceSync.Api.Services
{
    public interface IUniversalLoaderApiService
    {
        Task<List<WorkflowDto>> GetWorkflowsAsync();
        Task<bool> RunWorkflowAsync(string workflowId);
    }
}
