using Microsoft.EntityFrameworkCore;
using IceSync.Api.Services;
using IceSync.Api.Domain.Entities;

namespace IceSync.Api.Services
{
    public class WorkflowSyncService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WorkflowSyncService> _logger;
        private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(30);

        public WorkflowSyncService(
            IServiceProvider serviceProvider,
            ILogger<WorkflowSyncService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WorkflowSyncService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SyncWorkflowsAsync();
                    _logger.LogInformation("Workflow synchronization completed successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during workflow synchronization.");
                }

                await Task.Delay(_syncInterval, stoppingToken);
            }

            _logger.LogInformation("WorkflowSyncService is stopping.");
        }

        private async Task SyncWorkflowsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var apiService = scope.ServiceProvider.GetRequiredService<IUniversalLoaderApiService>();
            var workflowsService = scope.ServiceProvider.GetRequiredService<IWorkflowsService>();

            var apiWorkflows = await apiService.GetWorkflowsAsync();
            var apiWorkflowIds = apiWorkflows.Select(w => w.Id).ToHashSet();

            var dbWorkflows = await workflowsService.GetAllWorkflowsAsync();
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

            await workflowsService.SyncWorkflowsFromApiAsync(workflowsToInsert, workflowsToDelete, workflowsToUpdate);
        }
    }
}
