using Microsoft.EntityFrameworkCore;
using IceSync.Api.Data;
using IceSync.Api.Domain.Entities;

namespace IceSync.Api.Services;

public class WorkflowsService : IWorkflowsService
{
    private readonly IIceSyncDbContext _dbContext;
    private readonly ILogger<WorkflowsService> _logger;

    public WorkflowsService(IIceSyncDbContext dbContext, ILogger<WorkflowsService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<Workflow>> GetAllWorkflowsAsync()
    {
        try
        {
            return await _dbContext.Workflows.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflows from database");
            throw;
        }
    }

    public async Task<Workflow?> GetWorkflowByIdAsync(int workflowId)
    {
        try
        {
            return await _dbContext.Workflows.FindAsync(workflowId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow {WorkflowId} from database", workflowId);
            throw;
        }
    }

    public async Task<Workflow> CreateWorkflowAsync(Workflow workflow)
    {
        try
        {
            var entity = await _dbContext.Workflows.AddAsync(workflow);
            await _dbContext.SaveChangesAsync();
            return entity.Entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow {WorkflowId} in database", workflow.WorkflowId);
            throw;
        }
    }

    public async Task<Workflow> UpdateWorkflowAsync(Workflow workflow)
    {
        try
        {
            var entity = _dbContext.Workflows.Update(workflow);
            await _dbContext.SaveChangesAsync();
            return entity.Entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workflow {WorkflowId} in database", workflow.WorkflowId);
            throw;
        }
    }

    public async Task DeleteWorkflowAsync(int workflowId)
    {
        try
        {
            var workflow = await _dbContext.Workflows.FindAsync(workflowId);
            if (workflow != null)
            {
                _dbContext.Workflows.Remove(workflow);
                await _dbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workflow {WorkflowId} from database", workflowId);
            throw;
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to database");
            throw;
        }
    }

    public async Task SyncWorkflowsFromApiAsync(IEnumerable<Workflow> workflowsToInsert, IEnumerable<Workflow> workflowsToDelete, IEnumerable<Workflow> workflowsToUpdate)
    {
        try
        {
            if (workflowsToInsert.Any())
            {
                await _dbContext.Workflows.AddRangeAsync(workflowsToInsert);
                _logger.LogInformation("Inserting {Count} new workflows", workflowsToInsert.Count());
            }

            if (workflowsToDelete.Any())
            {
                _dbContext.Workflows.RemoveRange(workflowsToDelete);
                _logger.LogInformation("Deleting {Count} workflows", workflowsToDelete.Count());
            }

            // Note: workflowsToUpdate contains tracked entities that were already modified
            // No need to call UpdateRange since they're already being tracked
            if (workflowsToUpdate.Any())
            {
                _logger.LogInformation("Updating {Count} workflows", workflowsToUpdate.Count());
            }

            if (workflowsToInsert.Any() || workflowsToDelete.Any() || workflowsToUpdate.Any())
            {
                await _dbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing workflows with database");
            throw;
        }
    }
}
