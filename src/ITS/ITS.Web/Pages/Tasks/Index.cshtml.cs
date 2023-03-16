using Htmx;
using ITS.Core;
using ITS.Interfaces;
using ITS.Models;
using ITS.Web.Base;
using ITS.Web.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ITS.Web.Pages.Tasks;

public class IndexPageModel : BasePageModel
{
    private readonly ILogger<IndexPageModel> logger;
    private readonly IWorkTaskRepository workTaskRepository;
    private readonly IUserDataContext userDataContext;
    private AppOptions? webOptions;
    private readonly ApiOptions apiOption;

    public IndexPageModel(ILogger<IndexPageModel> logger,
        IWorkTaskRepository workTaskRepository,
        IOptions<AppOptions> webSettingsValue,
        IOptions<ApiOptions> apiOptionsValue,
        IUserDataContext userDataContext)
    {
        this.logger = logger;
        webOptions = webSettingsValue.Value;
        apiOption = apiOptionsValue.Value;
        this.workTaskRepository = workTaskRepository;
        this.userDataContext = userDataContext;
    }

    public async Task<IActionResult> OnGetAsync(int? pageNumber)
    {
        var pageCount = pageNumber ?? 1;
        logger.LogInformation("Task search page loaded {DateStarted}", DateTime.Now);
        WorkTasks = await workTaskRepository.SearchAsync(pageCount, webOptions.PageCount, true, Query);
        logger.LogInformation("Loaded {ItemCount} out of {AllCount} with {Query}. Generate Client URL.", WorkTasks.Count,
            WorkTasks.TotalPages, Query);

        ReportApiPublicTasks = $"{apiOption.ReportApiUrl}/tasks-api-reports/public-stats/pdf";
        logger.LogInformation("Url to download API is {ReportPublicTaskUrl}", ReportApiPublicTasks);
        
        await Task.Delay(2000); //delaying result to prepare the environment
        
        if (!Request.IsHtmx()) return Page();
        
        //Response.Htmx(h => h.Push(Request.GetEncodedUrl()));//set item in browser url textbox
        return Partial("_WorkTasksList", WorkTasks);
    }
    
    [BindProperty(SupportsGet = true)]
    public string Query { get; set; }
    [BindProperty] public PaginatedList<WorkTask> WorkTasks { get; set; }
    [BindProperty]
    public string ReportApiPublicTasks { get; set; }
}