using System.ComponentModel.DataAnnotations;

namespace ITS.Web.Options;

public class ApiOptions
{
    [Required(ErrorMessage = "Report URI is required")]
    public string ReportApiUrl { get; set; }
}