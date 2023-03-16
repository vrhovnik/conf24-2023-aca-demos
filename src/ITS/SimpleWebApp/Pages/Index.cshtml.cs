using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SimpleWebApp.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> logger;
    public IndexModel(ILogger<IndexModel> logger) => this.logger = logger;
    public void OnGet() => logger.LogInformation("Loading info page at {DateLoaded}", DateTime.Now);
}