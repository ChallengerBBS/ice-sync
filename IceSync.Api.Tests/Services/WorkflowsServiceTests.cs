using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using IceSync.Api.Data;
using IceSync.Api.Domain.Entities;
using IceSync.Api.Services;
using NUnit.Framework;

namespace IceSync.Api.Tests.Services
{
    [TestFixture]
    public class WorkflowsServiceTests
    {
        private Mock<IIceSyncDbContext> _mockDbContext = null!;
        private Mock<ILogger<WorkflowsService>> _mockLogger = null!;
        private WorkflowsService _service = null!;
        private Mock<DbSet<Workflow>> _mockWorkflowDbSet = null!;

        [SetUp]
        public void Setup()
        {
            _mockDbContext = new Mock<IIceSyncDbContext>();
            _mockLogger = new Mock<ILogger<WorkflowsService>>();
            _mockWorkflowDbSet = new Mock<DbSet<Workflow>>();

            _mockDbContext.Setup(x => x.Workflows).Returns(_mockWorkflowDbSet.Object);

            _service = new WorkflowsService(_mockDbContext.Object, _mockLogger.Object);
        }



        [Test]
        public async Task GetWorkflowByIdAsync_ShouldReturnWorkflow_WhenWorkflowExists()
        {
            var workflow = new Workflow { WorkflowId = 1, WorkflowName = "Test Workflow", IsActive = true, MultiExecBehavior = "Allow" };
            _mockWorkflowDbSet.Setup(x => x.FindAsync(1)).ReturnsAsync(workflow);

            var result = await _service.GetWorkflowByIdAsync(1);

            result.Should().BeEquivalentTo(workflow);
        }

        [Test]
        public async Task GetWorkflowByIdAsync_ShouldReturnNull_WhenWorkflowDoesNotExist()
        {
            _mockWorkflowDbSet.Setup(x => x.FindAsync(999)).ReturnsAsync((Workflow?)null);

            var result = await _service.GetWorkflowByIdAsync(999);

            result.Should().BeNull();
        }

        [Test]
        public async Task DeleteWorkflowAsync_ShouldDeleteWorkflow_WhenWorkflowExists()
        {
            var workflow = new Workflow { WorkflowId = 1, WorkflowName = "Test Workflow", IsActive = true, MultiExecBehavior = "Allow" };
            _mockWorkflowDbSet.Setup(x => x.FindAsync(1)).ReturnsAsync(workflow);
            _mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            await _service.DeleteWorkflowAsync(1);

            _mockWorkflowDbSet.Verify(x => x.FindAsync(1), Times.Once);
            _mockWorkflowDbSet.Verify(x => x.Remove(workflow), Times.Once);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task DeleteWorkflowAsync_ShouldNotDelete_WhenWorkflowDoesNotExist()
        {
            _mockWorkflowDbSet.Setup(x => x.FindAsync(999)).ReturnsAsync((Workflow?)null);

            await _service.DeleteWorkflowAsync(999);

            _mockWorkflowDbSet.Verify(x => x.FindAsync(999), Times.Once);
            _mockWorkflowDbSet.Verify(x => x.Remove(It.IsAny<Workflow>()), Times.Never);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SaveChangesAsync_ShouldReturnNumberOfAffectedRows_WhenSuccessful()
        {
            _mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(3);

            var result = await _service.SaveChangesAsync();

            result.Should().Be(3);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SyncWorkflowsFromApiAsync_ShouldSyncAllWorkflows_WhenSuccessful()
        {
            var workflowsToInsert = new List<Workflow>
            {
                new Workflow { WorkflowId = 1, WorkflowName = "New Workflow 1", IsActive = true, MultiExecBehavior = "Allow" },
                new Workflow { WorkflowId = 2, WorkflowName = "New Workflow 2", IsActive = false, MultiExecBehavior = "Deny" }
            };

            var workflowsToDelete = new List<Workflow>
            {
                new Workflow { WorkflowId = 3, WorkflowName = "Old Workflow", IsActive = true, MultiExecBehavior = "Allow" }
            };

            var workflowsToUpdate = new List<Workflow>
            {
                new Workflow { WorkflowId = 4, WorkflowName = "Updated Workflow", IsActive = false, MultiExecBehavior = "Deny" }
            };

            _mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(4);

            await _service.SyncWorkflowsFromApiAsync(workflowsToInsert, workflowsToDelete, workflowsToUpdate);

            _mockWorkflowDbSet.Verify(x => x.AddRangeAsync(workflowsToInsert, It.IsAny<CancellationToken>()), Times.Once);
            _mockWorkflowDbSet.Verify(x => x.RemoveRange(workflowsToDelete), Times.Once);
            _mockWorkflowDbSet.Verify(x => x.UpdateRange(workflowsToUpdate), Times.Once);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task SyncWorkflowsFromApiAsync_ShouldNotSaveChanges_WhenNoWorkflowsToSync()
        {
            var emptyList = new List<Workflow>();

            await _service.SyncWorkflowsFromApiAsync(emptyList, emptyList, emptyList);

            _mockWorkflowDbSet.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<Workflow>>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockWorkflowDbSet.Verify(x => x.RemoveRange(It.IsAny<IEnumerable<Workflow>>()), Times.Never);
            _mockWorkflowDbSet.Verify(x => x.UpdateRange(It.IsAny<IEnumerable<Workflow>>()), Times.Never);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
