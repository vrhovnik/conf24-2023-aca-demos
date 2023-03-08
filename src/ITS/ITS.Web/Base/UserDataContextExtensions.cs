using System.Security.Claims;
using ITS.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ITS.Web.Base;

public static class UserDataContextExtensions
{
    public static ClaimsPrincipal GenerateClaims(this ItsUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.NameIdentifier, user.ItsUserId),
            new(ClaimTypes.Email, user.Email)
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }
}