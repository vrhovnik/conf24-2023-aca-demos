using System.ComponentModel.DataAnnotations;
using ITS.Core;

namespace ITS.Web.Options;

public class ApiOptions : BaseSettings
{
    public ApiOptions() => SectionName = nameof(ApiOptions);

    [Required(ErrorMessage = "Report URI is required")]
    public string ReportApiUrl { get; set; }
}