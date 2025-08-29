using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using IceSync.Api.Controllers;
using IceSync.Api.Domain.Entities;
using IceSync.Api.Services;
using IceSync.Api.Domain.DTOs;

using System.Text.Json;
using NUnit.Framework;

namespace IceSync.Api.Tests.Controllers
{
    [TestFixture]
    public class WorkflowsControllerTests
    {
        private Mock<IWorkflowsService> _mockWorkflowsService = null!;
        private Mock<IUniversalLoaderApiService> _mockApiService = null!;
        private Mock<ILogger<WorkflowsController>> _mockLogger = null!;
        private WorkflowsController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _mockWorkflowsService = new Mock<IWorkflowsService>();
            _mockApiService = new Mock<IUniversalLoaderApiService>();
            _mockLogger = new Mock<ILogger<WorkflowsController>>();

            _controller = new WorkflowsController(
                _mockWorkflowsService.Object,
                _mockApiService.Object,
                _mockLogger.Object);
        }

        [Test]
        public async Task GetWorkflows_ShouldReturnAllWorkflows_WhenSuccessful()
        {
            var workflows = new List<Workflow>
            {
                new Workflow { WorkflowId = 1, WorkflowName = "Test Workflow 1", IsActive = true, MultiExecBehavior = "Allow" },
                new Workflow { WorkflowId = 2, WorkflowName = "Test Workflow 2", IsActive = false, MultiExecBehavior = "Deny" }
            };

            _mockWorkflowsService.Setup(x => x.GetAllWorkflowsAsync()).ReturnsAsync(workflows);

            var result = await _controller.GetWorkflows();

            result.Should().BeOfType<ActionResult<IEnumerable<Workflow>>>();
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result.Result!;
            okResult.Value.Should().BeEquivalentTo(workflows);
        }

        [Test]
        public async Task GetWorkflows_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            _mockWorkflowsService.Setup(x => x.GetAllWorkflowsAsync()).ThrowsAsync(new Exception("Database error"));

            var result = await _controller.GetWorkflows();

            result.Should().BeOfType<ActionResult<IEnumerable<Workflow>>>();
            result.Result.Should().BeOfType<ObjectResult>();
            var errorResult = (ObjectResult)result.Result!;
            errorResult.StatusCode.Should().Be(500);
            errorResult.Value.Should().Be("Internal server error");
        }

        [Test]
        public async Task RunWorkflow_ShouldReturnOk_WhenWorkflowRunsSuccessfully()
        {
            var workflowId = 123;
            _mockApiService.Setup(x => x.RunWorkflowAsync(workflowId.ToString())).ReturnsAsync(true);

            var result = await _controller.RunWorkflow(workflowId);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = JsonSerializer.Deserialize<ApiResponse>(JsonSerializer.Serialize(okResult.Value));
            response!.Message.Should().Be("Workflow triggered successfully");
            _mockApiService.Verify(x => x.RunWorkflowAsync(workflowId.ToString()), Times.Once());
        }

        [Test]
        public async Task RunWorkflow_ShouldReturnBadRequest_WhenWorkflowRunFails()
        {
            var workflowId = 123;
            _mockApiService.Setup(x => x.RunWorkflowAsync(workflowId.ToString())).ReturnsAsync(false);

            var result = await _controller.RunWorkflow(workflowId);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var response = JsonSerializer.Deserialize<ApiResponse>(JsonSerializer.Serialize(badRequestResult.Value));
            response!.Message.Should().Be("Failed to trigger workflow");
            _mockApiService.Verify(x => x.RunWorkflowAsync(workflowId.ToString()), Times.Once());
        }

        [Test]
        public async Task RunWorkflow_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            var workflowId = 123;
            _mockApiService.Setup(x => x.RunWorkflowAsync(workflowId.ToString())).ThrowsAsync(new Exception("API error"));

            var result = await _controller.RunWorkflow(workflowId);

            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
            var response = JsonSerializer.Deserialize<ApiResponse>(JsonSerializer.Serialize(statusResult.Value));
            response!.Message.Should().Be("Internal server error");
        }

        [Test]
        public async Task SyncWorkflows_ShouldReturnOk_WhenSyncIsSuccessful()
        {
            var apiWorkflows = new List<WorkflowDto>
            {
                new WorkflowDto { Id = 1, Name = "API Workflow 1", IsActive = true, MultiExecBehavior = "Allow" },
                new WorkflowDto { Id = 2, Name = "API Workflow 2", IsActive = false, MultiExecBehavior = "Deny" }
            };

            var dbWorkflows = new List<Workflow>
            {
                new Workflow { WorkflowId = 1, WorkflowName = "DB Workflow 1", IsActive = false, MultiExecBehavior = "Old" }
            };

            _mockApiService.Setup(x => x.GetWorkflowsAsync()).ReturnsAsync(apiWorkflows);
            _mockWorkflowsService.Setup(x => x.GetAllWorkflowsAsync()).ReturnsAsync(dbWorkflows);
            _mockWorkflowsService.Setup(x => x.SyncWorkflowsFromApiAsync(It.IsAny<IEnumerable<Workflow>>(), It.IsAny<IEnumerable<Workflow>>(), It.IsAny<IEnumerable<Workflow>>())).Returns(Task.CompletedTask);

            var result = await _controller.SyncWorkflows();

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = JsonSerializer.Deserialize<SyncWorkflowsResponse>(JsonSerializer.Serialize(okResult.Value));
            response!.Message.Should().Be("Synchronization completed successfully");
            response.Inserted.Should().Be(1);
            response.Deleted.Should().Be(0);
            response.Updated.Should().Be(1);
        }

        [Test]
        public async Task SyncWorkflows_ShouldReturnInternalServerError_WhenExceptionOccurs()
        {
            _mockApiService.Setup(x => x.GetWorkflowsAsync()).ThrowsAsync(new Exception("Sync error"));

            var result = await _controller.SyncWorkflows();

            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(500);
            var response = JsonSerializer.Deserialize<ApiResponse>(JsonSerializer.Serialize(statusResult.Value));
            response!.Message.Should().Be("Internal server error");
        }

        [Test]
        public async Task SyncWorkflows_ShouldHandleEmptyApiWorkflows()
        {
            var apiWorkflows = new List<WorkflowDto>();
            var dbWorkflows = new List<Workflow>
            {
                new Workflow { WorkflowId = 1, WorkflowName = "DB Workflow 1", IsActive = true, MultiExecBehavior = "Allow" }
            };

            _mockApiService.Setup(x => x.GetWorkflowsAsync()).ReturnsAsync(apiWorkflows);
            _mockWorkflowsService.Setup(x => x.GetAllWorkflowsAsync()).ReturnsAsync(dbWorkflows);
            _mockWorkflowsService.Setup(x => x.SyncWorkflowsFromApiAsync(It.IsAny<IEnumerable<Workflow>>(), It.IsAny<IEnumerable<Workflow>>(), It.IsAny<IEnumerable<Workflow>>())).Returns(Task.CompletedTask);

            var result = await _controller.SyncWorkflows();

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = JsonSerializer.Deserialize<SyncWorkflowsResponse>(JsonSerializer.Serialize(okResult.Value));
            response!.Message.Should().Be("Synchronization completed successfully");
            response.Inserted.Should().Be(0);
            response.Deleted.Should().Be(1);
            response.Updated.Should().Be(0);
        }

        [Test]
        public async Task SyncWorkflows_ShouldHandleEmptyDatabase()
        {
            var apiWorkflows = new List<WorkflowDto>
            {
                new WorkflowDto { Id = 1, Name = "API Workflow 1", IsActive = true, MultiExecBehavior = "Allow" }
            };

            var dbWorkflows = new List<Workflow>();

            _mockApiService.Setup(x => x.GetWorkflowsAsync()).ReturnsAsync(apiWorkflows);
            _mockWorkflowsService.Setup(x => x.GetAllWorkflowsAsync()).ReturnsAsync(dbWorkflows);
            _mockWorkflowsService.Setup(x => x.SyncWorkflowsFromApiAsync(It.IsAny<IEnumerable<Workflow>>(), It.IsAny<IEnumerable<Workflow>>(), It.IsAny<IEnumerable<Workflow>>())).Returns(Task.CompletedTask);

            var result = await _controller.SyncWorkflows();

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = JsonSerializer.Deserialize<SyncWorkflowsResponse>(JsonSerializer.Serialize(okResult.Value));
            response!.Message.Should().Be("Synchronization completed successfully");
            response.Inserted.Should().Be(1);
            response.Deleted.Should().Be(0);
            response.Updated.Should().Be(0);
        }

        [Test]
        public async Task RunWorkflow_ShouldHandleZeroWorkflowId()
        {
            _mockApiService.Setup(x => x.RunWorkflowAsync("0")).ReturnsAsync(false);

            var result = await _controller.RunWorkflow(0);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var response = JsonSerializer.Deserialize<ApiResponse>(JsonSerializer.Serialize(badRequestResult.Value));
            response!.Message.Should().Be("Failed to trigger workflow");
        }

        [Test]
        public async Task RunWorkflow_ShouldHandleNegativeWorkflowId()
        {
            _mockApiService.Setup(x => x.RunWorkflowAsync("-1")).ReturnsAsync(false);

            var result = await _controller.RunWorkflow(-1);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var response = JsonSerializer.Deserialize<ApiResponse>(JsonSerializer.Serialize(badRequestResult.Value));
            response!.Message.Should().Be("Failed to trigger workflow");
        }

        [Test]
        public async Task RunWorkflow_ShouldHandleSpecialWorkflowId()
        {
            var workflowId = 12345;
            _mockApiService.Setup(x => x.RunWorkflowAsync(workflowId.ToString())).ReturnsAsync(true);

            var result = await _controller.RunWorkflow(workflowId);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = JsonSerializer.Deserialize<ApiResponse>(JsonSerializer.Serialize(okResult.Value));
            response!.Message.Should().Be("Workflow triggered successfully");
            _mockApiService.Verify(x => x.RunWorkflowAsync(workflowId.ToString()), Times.Once());
        }

        [Test]
        public async Task RunWorkflow_ShouldHandleLargeWorkflowId()
        {
            var workflowId = int.MaxValue;
            _mockApiService.Setup(x => x.RunWorkflowAsync(workflowId.ToString())).ReturnsAsync(true);

            var result = await _controller.RunWorkflow(workflowId);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = JsonSerializer.Deserialize<ApiResponse>(JsonSerializer.Serialize(okResult.Value));
            response!.Message.Should().Be("Workflow triggered successfully");
            _mockApiService.Verify(x => x.RunWorkflowAsync(workflowId.ToString()), Times.Once());
        }
    }

    public class ApiResponse
    {
        public string Message { get; set; } = string.Empty;
    }

    public class SyncWorkflowsResponse
    {
        public string Message { get; set; } = string.Empty;
        public int Inserted { get; set; }
        public int Deleted { get; set; }
        public int Updated { get; set; }
    }
}
