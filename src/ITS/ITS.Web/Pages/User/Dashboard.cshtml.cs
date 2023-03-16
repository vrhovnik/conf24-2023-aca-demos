﻿using Htmx;
using ITS.Core;
using ITS.Interfaces;
using ITS.Models;
using ITS.Web.Base;
using ITS.Web.Options;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ITS.Web.Pages.User;

[Authorize]
public class DashboardPageModel : BasePageModel
{
    private readonly ILogger<DashboardPageModel> logger;
    private readonly IProfileSettingsService profileSettingsService;
    private readonly IWorkTaskRepository workTaskRepository;
    private readonly IUserDataContext userDataContext;
    private readonly TelemetryClient telemetryClient;
    private AppOptions appOptions;
    private readonly ApiOptions apiOptions;

    public DashboardPageModel(ILogger<DashboardPageModel> logger,
        IProfileSettingsService profileSettingsService,
        IWorkTaskRepository workTaskRepository,
        IOptions<AppOptions> webSettingsValue,
        IOptions<ApiOptions> apiSettingsValue,
        IUserDataContext userDataContext,
        TelemetryClient telemetryClient)
    {
        this.logger = logger;
        appOptions = webSettingsValue.Value;
        apiOptions = apiSettingsValue.Value;
        this.profileSettingsService = profileSettingsService;
        this.workTaskRepository = workTaskRepository;
        this.userDataContext = userDataContext;
        this.telemetryClient = telemetryClient;
    }

    public async Task<IActionResult> OnGetAsync(int? pageNumber, string query)
    {
        var currentPageNumber = pageNumber ?? 1;

        var userViewModel = userDataContext.GetCurrentUser();
        logger.LogInformation("Loading dashboard for user {User} - starting at {DateStart}", userViewModel.Fullname,
            DateTime.Now);

        var userId = userViewModel.UserId;

        using (var operation = telemetryClient.StartOperation<RequestTelemetry>($"UserProfile {userId}"))
        {
            try
            {
                telemetryClient.TrackTrace(new TraceTelemetry($"User from storage {userId}",
                    SeverityLevel.Information));
                ProfileSettings = await profileSettingsService.GetAsync(userId);
                operation.Telemetry.Properties.Add("profile-id", userId);
                logger.LogInformation("Got profile for {UniqueSettingsId} - ended at {DateEnd}", userId, DateTime.Now);

                telemetryClient.TrackTrace(new TraceTelemetry($"pdf generation for user {userId}"));
                PdfDownloadUrl = $"{apiOptions.ReportApiUrl}/tasks-api-reports/pdf/{userId}";
                operation.Telemetry.Properties.Add("pdf-url", PdfDownloadUrl);

                UserTasks = await workTaskRepository.WorkTasksForUserAsync(userId, currentPageNumber,
                    appOptions.PageCount, query);
                telemetryClient.TrackMetric(new MetricTelemetry("TaskCount", UserTasks.TotalItems));
                operation.Telemetry.Properties.Add("tasks-number", UserTasks.TotalItems.ToString());

                logger.LogInformation("Loaded {UserTaskNumber} work tasks for user with {Query}", UserTasks.TotalPages,
                    query);

                //set default values
                operation.Telemetry.Success = true;
                operation.Telemetry.ResponseCode = StatusCodes.Status200OK.ToString();

                telemetryClient.StopOperation(operation);
            }
            catch (Exception err)
            {
                logger.LogError(err.Message);
                operation.Telemetry.Success = false;
                telemetryClient.TrackException(err);
                throw;
            }
            finally
            {
                telemetryClient.StopOperation(operation);
            }
        }

        if (!Request.IsHtmx()) return Page();

        //Response.Htmx(h => h.Push(Request.GetEncodedUrl()));
        return Partial("_WorkTasksList", UserTasks);
    }

    [BindProperty] public string PdfDownloadUrl { get; set; }
    [BindProperty] public UserSettings ProfileSettings { get; set; }
    [BindProperty] public PaginatedList<WorkTask> UserTasks { get; set; }
}