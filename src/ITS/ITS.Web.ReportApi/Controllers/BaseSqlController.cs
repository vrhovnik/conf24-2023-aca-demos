using Microsoft.AspNetCore.Mvc;

namespace ITS.Web.ReportApi.Controllers;

public abstract class BaseSqlController : ControllerBase
{
    public abstract IActionResult CheckDbHealth();
}