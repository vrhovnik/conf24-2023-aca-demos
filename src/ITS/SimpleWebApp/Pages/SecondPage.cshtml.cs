using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SimpleWebApp.Pages;

public class SecondPageModel : PageModel
{
    private readonly ILogger<SecondPageModel> logger;

    public SecondPageModel(ILogger<SecondPageModel> logger) => this.logger = logger;

    public void OnGet() => logger.LogInformation("Second page loaded at {DateLoaded}", DateTime.Now);
}