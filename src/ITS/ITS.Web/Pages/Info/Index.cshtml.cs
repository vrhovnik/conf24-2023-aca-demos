using ITS.Models;
using ITS.Web.Base;
using ITS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ITS.Web.Pages.Info;

[AllowAnonymous]
public class IndexPageModel : PageModel
{
    private readonly ReportApiHttpService reportApiHttpService;
    private readonly IUserDataContext userDataContext;
    private readonly ILogger<IndexPageModel> logger;

    public IndexPageModel(ReportApiHttpService reportApiHttpService,
        IUserDataContext userDataContext,
        ILogger<IndexPageModel> logger)
    {
        this.reportApiHttpService = reportApiHttpService;
        this.userDataContext = userDataContext;
        this.logger = logger;
    }

    public async Task OnGetAsync()
    {
        logger.LogInformation("Main info page has been loaded at {CurrentDateTime}", DateTime.Now);
        ServerName = Environment.MachineName;
        logger.LogInformation("Received {ServerName}", ServerName);

        if (User.Identity.IsAuthenticated)
        {
            logger.LogInformation("Getting current user");
            var userId = userDataContext.GetCurrentUser().UserId;
            logger.LogInformation("User {userId} has been aquired. Calling API at {DateCalled}.", userId, DateTime.Now);
            var isServiceAlive = await reportApiHttpService.IsServiceAliveAsync();
            if (isServiceAlive)
            {
                logger.LogInformation("Service is alive. Calling endpoint to get stats.");
                CurrentUserStats = await reportApiHttpService.GetUserStatsAsync(userId);
                logger.LogInformation("Api call finished {DateFinished}", DateTime.Now);
            }
            else
                logger.LogInformation("Service is not alive. Check log messages");
        }
    }

    [BindProperty] public string ServerName { get; set; } = "";
    [BindProperty] public UserStats CurrentUserStats { get; set; }
}