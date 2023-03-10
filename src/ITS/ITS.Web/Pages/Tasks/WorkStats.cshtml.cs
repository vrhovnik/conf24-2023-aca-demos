using ITS.Core;
using ITS.Interfaces;
using ITS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace ITS.Web.Pages.Tasks;

public class WorkStatsPageModel : PageModel
{
    private readonly ILogger<WorkStatsPageModel> logger;
    private readonly IWorkStatsRepository workStatsRepository;
    private AppOptions appOptions;

    public WorkStatsPageModel(ILogger<WorkStatsPageModel> logger,
        IOptions<AppOptions> appOptionsValue,
        IWorkStatsRepository workStatsRepository)
    {
        this.logger = logger;
        appOptions = appOptionsValue.Value;
        this.workStatsRepository = workStatsRepository;
    }

    public async Task OnGetAsync(int? pageNumber)
    {
        var currentPageNumber = pageNumber ?? 1;
        logger.LogInformation("Loading page workstats at {DateLoaded} with page number {PageNumber}.",
            DateTime.Now, currentPageNumber);
        var statsList = await workStatsRepository.GetAllAsync();
        logger.LogInformation("Loaded {NumberOfStats} from file.", statsList.Count);
        WorkStatsList = PaginatedList<WorkTaskStats>.Create(statsList, currentPageNumber, appOptions.PageCount);
        logger.LogInformation("Loaded paginated list with {Pages}", WorkStatsList.TotalPages);
    }

    [BindProperty] public PaginatedList<WorkTaskStats> WorkStatsList { get; set; }
}