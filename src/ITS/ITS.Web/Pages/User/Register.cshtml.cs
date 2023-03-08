using ITS.Interfaces;
using ITS.Models;
using ITS.Web.Base;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITS.Web.Pages.User;

[AllowAnonymous]
public class RegisterPageModel : BasePageModel
{
    private readonly ILogger<RegisterPageModel> logger;
    private readonly IUserRepository userRepository;

    public RegisterPageModel(ILogger<RegisterPageModel> logger, IUserRepository userRepository)
    {
        this.logger = logger;
        this.userRepository = userRepository;
    }

    public void OnGetAsync() => logger.LogInformation("Loaded register page at {DateLoaded}", DateTime.Now);

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(NewUser.Email) || string.IsNullOrEmpty(NewUser.Password))
        {
            Message = "Enter required data: Email and Password";
            logger.LogError("Data is not entered, missing either Email or Password.");
            return Page();
        }

        //check if email is already on
        var user = await userRepository.FindAsync(NewUser.Email);
        if (user != null)
        {
            Message = $"User with email {user.Email} already exists in database, try new one";
            logger.LogWarning(Message);
            return Page();
        }

        var ttaUser = await userRepository.InsertAsync(NewUser);
        
        await HttpContext.SignInAsync(ttaUser.GenerateClaims());

        return RedirectToPage("/User/Dashboard");
    }

    [BindProperty] public ItsUser NewUser { get; set; }
}