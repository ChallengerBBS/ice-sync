using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using IceSync.Api.Domain.DTOs;
using IceSync.Api.Infrastructure.Configuration;
using System.Net.Http.Headers;

namespace IceSync.Api.Services;

public class UniversalLoaderApiService : IUniversalLoaderApiService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<UniversalLoaderApiService> _logger;
    private readonly UniversalLoaderConfig _config;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public UniversalLoaderApiService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<UniversalLoaderApiService> logger,
        IOptions<UniversalLoaderConfig> config)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _config = config.Value;
        _httpClient.BaseAddress = new Uri(_config.BaseUrl ?? throw new InvalidOperationException("BaseUrl configuration is required"));
    }

    public async Task<List<WorkflowDto>> GetWorkflowsAsync()
    {
        var token = await GetValidTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync("/workflows");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var workflows = JsonSerializer.Deserialize<List<WorkflowDto>>(content, _jsonOptions);

        return workflows ?? [];
    }

    public async Task<bool> RunWorkflowAsync(string workflowId)
    {
        var token = await GetValidTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.PostAsync($"/workflows/{workflowId}/run", null);
        return response.IsSuccessStatusCode;
    }

    private async Task<string> GetValidTokenAsync()
    {
        const string cacheKey = "universal_loader_token";

        if (_cache.TryGetValue(cacheKey, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
        {
            _logger.LogDebug("Using cached access token");
            return cachedToken;
        }

        _logger.LogInformation("No valid cached token found. Requesting new access token...");
        var tokenResponse = await RequestNewTokenAsync();
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(tokenResponse.ExpiresIn - 300) // Cache for token duration minus 5 minutes buffer
        };
        _cache.Set(cacheKey, tokenResponse.AccessToken, cacheOptions);

        _logger.LogInformation("Access token cached for {CacheDuration} seconds", tokenResponse.ExpiresIn - 300);
        return tokenResponse.AccessToken;
    }

    private async Task<TokenResponse> RequestNewTokenAsync()
    {
        try
        {
            var authRequest = new
            {
                apiCompanyId = _config.CompanyId,
                apiUserId = _config.UserId,
                apiUserSecret = _config.UserSecret
            };

            _logger.LogInformation("Attempting to authenticate with BaseUrl: {BaseUrl}", _config.BaseUrl);
            _logger.LogDebug("Auth request payload: {AuthRequest}", JsonSerializer.Serialize(authRequest));

            var json = JsonSerializer.Serialize(authRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var requestUrl = $"{_config.BaseUrl}/v2/authenticate";
            _logger.LogInformation("Making authentication request to: {RequestUrl}", requestUrl);

            var response = await _httpClient.PostAsync("/v2/authenticate", content);
            
            _logger.LogInformation("Authentication response status: {StatusCode}", response.StatusCode);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Authentication failed with status {StatusCode}. Response: {ErrorContent}", response.StatusCode, errorContent);
                throw new InvalidOperationException($"Authentication failed with status {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Authentication response content: {ResponseContent}", responseContent);

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, _jsonOptions);

            if (tokenResponse == null)
            {
                _logger.LogError("Failed to deserialize token response. Response content: {ResponseContent}", responseContent);
                throw new InvalidOperationException("Failed to deserialize authentication response");
            }

            if (string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                _logger.LogError("Access token is null or empty in response. Full response: {ResponseContent}", responseContent);
                throw new InvalidOperationException("Access token is null or empty in authentication response");
            }

            _logger.LogInformation("Successfully obtained access token. Expires in {ExpiresIn} seconds", tokenResponse.ExpiresIn);

            return tokenResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request exception during authentication");
            throw new InvalidOperationException($"HTTP request failed during authentication: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization exception during authentication");
            throw new InvalidOperationException($"Failed to parse authentication response: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception during authentication");
            throw;
        }
    }
}
