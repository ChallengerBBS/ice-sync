using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using FluentAssertions;
using IceSync.Api.Domain.DTOs;
using IceSync.Api.Infrastructure.Configuration;
using IceSync.Api.Services;
using NUnit.Framework;

namespace IceSync.Api.Tests.Services
{
    [TestFixture]
    public class UniversalLoaderApiServiceTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler = null!;
        private Mock<IMemoryCache> _mockCache = null!;
        private Mock<ILogger<UniversalLoaderApiService>> _mockLogger = null!;
        private UniversalLoaderConfig _config = null!;
        private UniversalLoaderApiService _service = null!;
        private HttpClient _httpClient = null!;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockCache = new Mock<IMemoryCache>();
            _mockLogger = new Mock<ILogger<UniversalLoaderApiService>>();
            _config = new UniversalLoaderConfig
            {
                BaseUrl = "https://test-api.example.com",
                CompanyId = "test-company-id",
                UserId = "test-user-id",
                UserSecret = "test-secret-key"
            };

            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockCache.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            
            _service = new UniversalLoaderApiService(
                _httpClient,
                _mockCache.Object,
                _mockLogger.Object,
                Options.Create(_config));
        }

        [Test]
        public async Task GetWorkflowsAsync_ShouldReturnWorkflows_WhenSuccessful()
        {
            var workflows = new List<WorkflowDto>
            {
                new WorkflowDto { Id = 1, Name = "Test Workflow 1", IsActive = true, MultiExecBehavior = "Allow" },
                new WorkflowDto { Id = 2, Name = "Test Workflow 2", IsActive = false, MultiExecBehavior = "Deny" }
            };

            var tokenResponse = new TokenResponse
            {
                AccessToken = "test-token",
                TokenType = "Bearer",
                ExpiresIn = 3600
            };

            SetupAuthenticationResponse(tokenResponse);
            SetupWorkflowsResponse(workflows);

            var result = await _service.GetWorkflowsAsync();

            result.Should().HaveCount(2);
            result[0].Id.Should().Be(1);
            result[0].Name.Should().Be("Test Workflow 1");
            result[0].IsActive.Should().BeTrue();
            result[1].Id.Should().Be(2);
            result[1].Name.Should().Be("Test Workflow 2");
            result[1].IsActive.Should().BeFalse();
        }

        [Test]
        public async Task GetWorkflowsAsync_ShouldReturnEmptyList_WhenApiReturnsNull()
        {
            var tokenResponse = new TokenResponse
            {
                AccessToken = "test-token",
                TokenType = "Bearer",
                ExpiresIn = 3600
            };

            SetupAuthenticationResponse(tokenResponse);
            SetupWorkflowsResponse(null);

            var result = await _service.GetWorkflowsAsync();

            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetWorkflowsAsync_ShouldThrowException_WhenAuthenticationFails()
        {
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/v2/authenticate")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent("Authentication failed")
                });

            await _service.Invoking(x => x.GetWorkflowsAsync())
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Authentication failed*");
        }

        [Test]
        public async Task GetWorkflowsAsync_ShouldThrowException_WhenWorkflowsRequestFails()
        {
            var tokenResponse = new TokenResponse
            {
                AccessToken = "test-token",
                TokenType = "Bearer",
                ExpiresIn = 3600
            };

            SetupAuthenticationResponse(tokenResponse);
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/workflows")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Server error")
                });

            await _service.Invoking(x => x.GetWorkflowsAsync())
                .Should().ThrowAsync<HttpRequestException>();
        }

        [Test]
        public async Task RunWorkflowAsync_ShouldReturnTrue_WhenSuccessful()
        {
            var workflowId = "123";
            var tokenResponse = new TokenResponse
            {
                AccessToken = "test-token",
                TokenType = "Bearer",
                ExpiresIn = 3600
            };

            SetupAuthenticationResponse(tokenResponse);
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains($"/workflows/{workflowId}/run")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var result = await _service.RunWorkflowAsync(workflowId);

            result.Should().BeTrue();
        }

        [Test]
        public async Task RunWorkflowAsync_ShouldReturnFalse_WhenRequestFails()
        {
            var workflowId = "123";
            var tokenResponse = new TokenResponse
            {
                AccessToken = "test-token",
                TokenType = "Bearer",
                ExpiresIn = 3600
            };

            SetupAuthenticationResponse(tokenResponse);
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains($"/workflows/{workflowId}/run")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var result = await _service.RunWorkflowAsync(workflowId);

            result.Should().BeFalse();
        }

        [Test]
        public async Task RunWorkflowAsync_ShouldThrowException_WhenAuthenticationFails()
        {
            var workflowId = "123";
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/v2/authenticate")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent("Authentication failed")
                });

            await _service.Invoking(x => x.RunWorkflowAsync(workflowId))
                .Should().ThrowAsync<InvalidOperationException>();
        }

        [Test]
        public void Constructor_ShouldThrowException_WhenBaseUrlIsNull()
        {
            var invalidConfig = new UniversalLoaderConfig
            {
                BaseUrl = null,
                CompanyId = "test",
                UserId = "test",
                UserSecret = "test"
            };

            Action action = () => new UniversalLoaderApiService(
                _httpClient,
                _mockCache.Object,
                _mockLogger.Object,
                Options.Create(invalidConfig));

            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*BaseUrl configuration is required*");
        }

        [Test]
        public void Constructor_ShouldThrowException_WhenBaseUrlIsEmpty()
        {
            var invalidConfig = new UniversalLoaderConfig
            {
                BaseUrl = "",
                CompanyId = "test",
                UserId = "test",
                UserSecret = "test"
            };

            Action action = () => new UniversalLoaderApiService(
                _httpClient,
                _mockCache.Object,
                _mockLogger.Object,
                Options.Create(invalidConfig));

            action.Should().Throw<UriFormatException>()
                .WithMessage("*URI is empty*");
        }

        [Test]
        public async Task GetWorkflowsAsync_ShouldThrowException_WhenTokenResponseIsNull()
        {
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/v2/authenticate")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("null", Encoding.UTF8, "application/json")
                });

            await _service.Invoking(x => x.GetWorkflowsAsync())
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Failed to deserialize authentication response*");
        }

        [Test]
        public async Task GetWorkflowsAsync_ShouldThrowException_WhenTokenResponseHasEmptyAccessToken()
        {
            var tokenResponse = new TokenResponse
            {
                AccessToken = "",
                TokenType = "Bearer",
                ExpiresIn = 3600
            };

            SetupAuthenticationResponse(tokenResponse);

            await _service.Invoking(x => x.GetWorkflowsAsync())
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Access token is null or empty*");
        }

        [Test]
        public async Task GetWorkflowsAsync_ShouldThrowException_WhenJsonDeserializationFails()
        {
            var tokenResponse = new TokenResponse
            {
                AccessToken = "test-token",
                TokenType = "Bearer",
                ExpiresIn = 3600
            };

            SetupAuthenticationResponse(tokenResponse);
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/workflows")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("invalid json", Encoding.UTF8, "application/json")
                });

            await _service.Invoking(x => x.GetWorkflowsAsync())
                .Should().ThrowAsync<JsonException>();
        }

        [Test]
        public async Task RunWorkflowAsync_ShouldReturnFalse_WhenWorkflowIdIsNull()
        {
            var tokenResponse = new TokenResponse
            {
                AccessToken = "test-token",
                TokenType = "Bearer",
                ExpiresIn = 3600
            };

            SetupAuthenticationResponse(tokenResponse);
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && !req.RequestUri!.ToString().Contains("/v2/authenticate")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var result = await _service.RunWorkflowAsync(null!);

            result.Should().BeFalse();
        }

        [Test]
        public async Task RunWorkflowAsync_ShouldReturnFalse_WhenWorkflowIdIsZero()
        {
            var tokenResponse = new TokenResponse
            {
                AccessToken = "test-token",
                TokenType = "Bearer",
                ExpiresIn = 3600
            };

            SetupAuthenticationResponse(tokenResponse);
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && !req.RequestUri!.ToString().Contains("/v2/authenticate")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var result = await _service.RunWorkflowAsync("0");

            result.Should().BeFalse();
        }

        [Test]
        public async Task GetWorkflowsAsync_ShouldHandleTimeout()
        {
            var tokenResponse = new TokenResponse
            {
                AccessToken = "test-token",
                TokenType = "Bearer",
                ExpiresIn = 3600
            };

            SetupAuthenticationResponse(tokenResponse);
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/workflows")),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException("Request timeout"));

            await _service.Invoking(x => x.GetWorkflowsAsync())
                .Should().ThrowAsync<TaskCanceledException>()
                .WithMessage("*timeout*");
        }

        private void SetupAuthenticationResponse(TokenResponse tokenResponse)
        {
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/v2/authenticate")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(tokenResponse), Encoding.UTF8, "application/json")
                });
        }

        private void SetupWorkflowsResponse(List<WorkflowDto>? workflows)
        {
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("/workflows")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(workflows), Encoding.UTF8, "application/json")
                });
        }
    }
}
