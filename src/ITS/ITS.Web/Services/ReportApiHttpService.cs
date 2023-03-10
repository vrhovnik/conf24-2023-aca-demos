using System.Diagnostics;
using HashidsNet;
using ITS.Core;
using ITS.Models;
using ITS.Web.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;

namespace ITS.Web.Services;

public class ReportApiHttpService
{
    private readonly HttpClient client;
    private readonly ILogger<ReportApiHttpService> logger;
    private readonly AuthOptions authOptions;
    private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitBreakerPolicy =
        Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrTransientHttpError()
            .AdvancedCircuitBreakerAsync(0.5, TimeSpan.FromSeconds(10), 10, TimeSpan.FromSeconds(5));

    public ReportApiHttpService(HttpClient client, IOptions<ApiOptions> apiOptionsValue,
        IOptions<AuthOptions> authOptionsValue, ILogger<ReportApiHttpService> logger)
    {
        this.client = client;
        this.logger = logger;
        this.client.BaseAddress = new Uri(apiOptionsValue.Value.ReportApiUrl);
        authOptions = authOptionsValue.Value;
        this.client.DefaultRequestHeaders.Add(AuthOptions.ApiKeyHeaderName, authOptions.ApiKey);
    }

    public async Task<bool> IsServiceAliveAsync()
    {
        try
        {
            var watch = Stopwatch.StartNew();
            logger.LogInformation("Check service health");
            var healthMessage = await circuitBreakerPolicy.ExecuteAsync(() => client.GetAsync("health"));
            watch.Stop();

            logger.LogInformation("Call to health endpoint took {ElapsedInMs} ms.", watch.ElapsedMilliseconds);
            if (!healthMessage.IsSuccessStatusCode)
            {
                logger.LogError("Service is not alived, check connectivity.");
                watch.Stop();
                return false;
            }

            logger.LogInformation("Service up and running, checking connection to database");
            watch.Reset();
            var databaseHealth =
                await circuitBreakerPolicy.ExecuteAsync(() => client.GetAsync("tasks-api-reports/app-health"));
            logger.LogInformation("Call to database endpoint took {ElapsedInMs} ms.", watch.ElapsedMilliseconds);
            if (!databaseHealth.IsSuccessStatusCode)
            {
                logger.LogError("Database is not up and running, check connection string and database service.");
                watch.Stop();
                return false;
            }

            watch.Stop();
            logger.LogInformation("Database is up and running at {DateCalled}.", DateTime.Now);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return false;
        }
    }

    public async Task<UserStats> GetUserStatsAsync(string userId)
    {
        var hashIds = new Hashids(authOptions.HashSalt);
        if (!int.TryParse(userId, out var toBeHashedId))
        {
            logger.LogError("User Id cannot be hashed. Check data. UserID - {UserId}", userId);
            return null;
        }

        var currentUserId = hashIds.Encode(toBeHashedId);
        try
        {
            var watch = new Stopwatch();
            watch.Start();
            logger.LogInformation("Calling API and measuring response");
            var responseMessage =
                await client.GetAsync($"tasks-api-reports/stats/{currentUserId}");
            watch.Stop();

            logger.LogInformation("Api called. Call was {TimeInMs} ms", watch.ElapsedMilliseconds);
            responseMessage.EnsureSuccessStatusCode();
            logger.LogInformation("Getting data, transforming to object.");

            var userStatsData = await responseMessage.Content.ReadAsStringAsync();
            logger.LogInformation("Data received - {RawData}", userStatsData);
            var userStats = JsonConvert.DeserializeObject<UserStats>(userStatsData);
            return userStats;
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            return null;
        }
    }
}