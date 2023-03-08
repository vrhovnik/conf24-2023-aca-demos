using ITS.Web.Base;
using Microsoft.AspNetCore.Mvc;

namespace ITS.Web.Pages.User;

public class LogoutPageModel : BasePageModel
{
    private readonly ILogger<LogoutPageModel> logger;
    private readonly IUserDataContext userDataContext;

    public LogoutPageModel(ILogger<LogoutPageModel> logger, IUserDataContext userDataContext)
    {
        this.logger = logger;
        this.userDataContext = userDataContext;
    }

    public async Task<RedirectToPageResult> OnGetAsync()
    {
        var fullName = userDataContext.GetCurrentUser().Fullname;
        logger.LogInformation("Logged out page loaded at {DateLoaded}.", DateTime.Now);
        
        await userDataContext.LogOut();
        
        Message = $"User {fullName} has been logged out";
        logger.LogInformation("User {CurrentUser} logged out at {DateLoaded}.", fullName, DateTime.Now);
        return RedirectToPage("/Info/Index");
    }
}