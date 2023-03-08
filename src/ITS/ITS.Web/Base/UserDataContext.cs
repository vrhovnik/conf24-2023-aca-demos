using System.Security.Claims;
using ITS.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;

namespace ITS.Web.Base;

public class UserDataContext : IUserDataContext
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public UserDataContext(IHttpContextAccessor httpContextAccessor) => 
        this.httpContextAccessor = httpContextAccessor;

    public UserViewModel GetCurrentUser()
    {
        var httpContextUser = httpContextAccessor.HttpContext.User;

        var currentUser = new UserViewModel();
        var claimName = httpContextUser.FindFirst(ClaimTypes.Name);
        currentUser.Fullname = claimName.Value;

        var claimId = httpContextUser.FindFirst(ClaimTypes.NameIdentifier);
        currentUser.UserId = claimId.Value;

        var claimEmail = httpContextUser.FindFirst(ClaimTypes.Email);
        currentUser.Email = claimEmail.Value;

        return currentUser;
    }

    public Task LogOut() => httpContextAccessor.HttpContext.SignOutAsync();
}

