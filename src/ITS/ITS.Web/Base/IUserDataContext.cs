using ITS.Web.ViewModels;

namespace ITS.Web.Base;

public interface IUserDataContext
{
    UserViewModel GetCurrentUser();
    Task LogOut();
}