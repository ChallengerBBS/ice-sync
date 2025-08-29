using Microsoft.AspNetCore.Mvc;
using IceSync.Api.Domain.Entities;
using IceSync.Api.Services;

namespace IceSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowsService _workflowsService;
    private readonly IUniversalLoaderApiService _apiService;
    private readonly ILogger<WorkflowsController> _logger;

    public WorkflowsController(
        IWorkflowsService workflowsService,
        IUniversalLoaderApiService apiService,
        ILogger<WorkflowsController> logger)
    {
        _workflowsService = workflowsService;
        _apiService = apiService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Workflow>>> GetWorkflows()
    {
        try
        {
            var workflows = await _workflowsService.GetAllWorkflowsAsync();
            return Ok(workflows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflows from database");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{workflowId}/run")]
    public async Task<ActionResult> RunWorkflow(int workflowId)
    {
        try
        {
            var success = await _apiService.RunWorkflowAsync(workflowId.ToString());
            
            if (success)
            {
                _logger.LogInformation("Successfully triggered workflow {WorkflowId}", workflowId);
                return Ok(new { Message = "Workflow triggered successfully" });
            }
            else
            {
                _logger.LogWarning("Failed to trigger workflow {WorkflowId}", workflowId);
                return BadRequest(new { Message = "Failed to trigger workflow" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running workflow {WorkflowId}", workflowId);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }

    [HttpPost("sync")]
    public async Task<ActionResult> SyncWorkflows()
    {
        try
        {
            var apiWorkflows = await _apiService.GetWorkflowsAsync();
            var apiWorkflowIds = apiWorkflows.Select(w => w.Id).ToHashSet();

            var dbWorkflows = await _workflowsService.GetAllWorkflowsAsync();
            var dbWorkflowIds = dbWorkflows.Select(w => w.WorkflowId).ToHashSet();

            var workflowsToInsert = apiWorkflows
                .Where(api => !dbWorkflowIds.Contains(api.Id))
                .Select(api => new Workflow
                {
                    WorkflowId = api.Id,
                    WorkflowName = api.Name,
                    IsActive = api.IsActive,
                    MultiExecBehavior = api.MultiExecBehavior
                })
                .ToList();

            var workflowsToDelete = dbWorkflows
                .Where(db => !apiWorkflowIds.Contains(db.WorkflowId))
                .ToList();

            var workflowsToUpdate = dbWorkflows
                .Where(db => apiWorkflowIds.Contains(db.WorkflowId))
                .ToList();

            foreach (var dbWorkflow in workflowsToUpdate)
            {
                var apiWorkflow = apiWorkflows.First(api => api.Id == dbWorkflow.WorkflowId);
                dbWorkflow.WorkflowName = apiWorkflow.Name;
                dbWorkflow.IsActive = apiWorkflow.IsActive;
                dbWorkflow.MultiExecBehavior = apiWorkflow.MultiExecBehavior;
            }

            await _workflowsService.SyncWorkflowsFromApiAsync(workflowsToInsert, workflowsToDelete, workflowsToUpdate);

            return Ok(new
            {
                Message = "Synchronization completed successfully",
                Inserted = workflowsToInsert.Count,
                Deleted = workflowsToDelete.Count,
                Updated = workflowsToUpdate.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing workflows");
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }
}
