using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SimpleWebApp.Pages;

public class EnvPageModel : PageModel
{
    private readonly ILogger<EnvPageModel> logger;

    public EnvPageModel(ILogger<EnvPageModel> logger) => this.logger = logger;

    public void OnGet()
    {
        logger.LogInformation("Loaded env page at {DateLoaded}", DateTime.Now);
        var environmentVariable = Environment.GetEnvironmentVariable("MESSAGE");
        EnvValue = string.IsNullOrEmpty(environmentVariable) ? 
            environmentVariable : "Environment variable is not set";
    }

    [BindProperty]
    public string EnvValue { get; set; }
}