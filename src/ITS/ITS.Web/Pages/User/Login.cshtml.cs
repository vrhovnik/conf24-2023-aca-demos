using ITS.Interfaces;
using ITS.Web.Base;
using ITS.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITS.Web.Pages.User;

[AllowAnonymous]
public class LoginPageModel : BasePageModel
{
    private readonly ILogger<LoginPageModel> logger;
    private readonly IUserRepository userRepository;

    public LoginPageModel(ILogger<LoginPageModel> logger, IUserRepository userRepository)
    {
        this.logger = logger;
        this.userRepository = userRepository;
    }

    public void OnGet()
    {
        logger.LogInformation("Load page at {DateLoaded}", DateTime.Now);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(LoginData.Email) || string.IsNullOrEmpty(LoginData.Password))
        {
            Message = "Username or password is mandatory";
            logger.LogWarning("Username or password is mandatory - data was empty at {DateLoaded}", DateTime.Now);
            return Page();
        }

        var user = await userRepository.LoginAsync(LoginData.Email, LoginData.Password);

        if (user == null)
        {
            Message = "Check username and password, login was unsuccessful";
            return Page();
        }

        await HttpContext.SignInAsync(user.GenerateClaims());

        return RedirectToPage("/User/Dashboard");
    }


    [BindProperty] public LoginPageViewModel LoginData { get; set; }
}