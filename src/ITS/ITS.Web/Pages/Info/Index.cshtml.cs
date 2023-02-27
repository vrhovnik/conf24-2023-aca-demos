using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ITS.Web.Pages.Info;

[AllowAnonymous]
public class IndexPageModel : PageModel
{
    private readonly ILogger<IndexPageModel> logger;
    public IndexPageModel(ILogger<IndexPageModel> logger) => this.logger = logger;

    public void OnGet()
    {
        logger.LogInformation("Main info page has been loaded at {CurrentDateTime}", DateTime.Now);
        ServerName = Environment.MachineName;
        logger.LogInformation("Received {QOTD}", ServerName);
    }

    [BindProperty] public string ServerName { get; set; } = "";
}