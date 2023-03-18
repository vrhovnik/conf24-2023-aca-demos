using Htmx;
using ITS.Core;
using ITS.Interfaces;
using ITS.Models;
using ITS.Web.Base;
using ITS.Web.Options;
using ITS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ITS.Web.Pages.Tasks;

public class IndexPageModel : BasePageModel
{
    private readonly ILogger<IndexPageModel> logger;
    private readonly IWorkTaskRepository workTaskRepository;
    private readonly ReportApiHttpService reportApiHttpService;
    private readonly IUserDataContext userDataContext;
    private AppOptions? webOptions;
    private readonly ApiOptions apiOption;

    public IndexPageModel(ILogger<IndexPageModel> logger,
        IWorkTaskRepository workTaskRepository,
        IOptions<AppOptions> webSettingsValue,
        IOptions<ApiOptions> apiOptionsValue,
        ReportApiHttpService reportApiHttpService,
        IUserDataContext userDataContext)
    {
        this.logger = logger;
        webOptions = webSettingsValue.Value;
        apiOption = apiOptionsValue.Value;
        this.workTaskRepository = workTaskRepository;
        this.reportApiHttpService = reportApiHttpService;
        this.userDataContext = userDataContext;
    }

    public async Task<IActionResult> OnGetAsync(int? pageNumber)
    {
        var pageCount = pageNumber ?? 1;
        logger.LogInformation("Task search page loaded {DateStarted}", DateTime.Now);
        WorkTasks = await workTaskRepository.SearchAsync(pageCount, webOptions.PageCount, true, Query);
        logger.LogInformation("Loaded {ItemCount} out of {AllCount} with {Query}. Generate Client URL.",
            WorkTasks.Count,
            WorkTasks.TotalPages, Query);

        ReportApiPublicTasks = $"{apiOption.ReportApiUrl}/tasks-api-reports/public-stats/pdf";
        logger.LogInformation("Url to download API is {ReportPublicTaskUrl}", ReportApiPublicTasks);

        try
        {
            logger.LogInformation("Calling HTTP service");
            var task = await reportApiHttpService.GetMostActiveTaskAsync();
            MostActiveTask = $"Most active task is in {task.Category.Name} category";
            logger.LogInformation("Received {WorkTaskId} from http service", task.WorkTaskId);
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }

        if (!Request.IsHtmx()) return Page();

        //Response.Htmx(h => h.Push(Request.GetEncodedUrl()));//set item in browser url textbox
        return Partial("_WorkTasksList", WorkTasks);
    }

    [BindProperty(SupportsGet = true)] public string Query { get; set; }
    [BindProperty] public PaginatedList<WorkTask> WorkTasks { get; set; }
    [BindProperty] public string ReportApiPublicTasks { get; set; }
    [BindProperty] public string? MostActiveTask { get; set; }
}