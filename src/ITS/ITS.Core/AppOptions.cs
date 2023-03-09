using System.ComponentModel.DataAnnotations;

namespace ITS.Core;

public class AppOptions : BaseSettings 
{
    public AppOptions() => SectionName = nameof(AppOptions);

    public string DefaultEmailFrom { get; set; }
    [Required]
    public string ClientApiUrl { get; set; }
    [Required]
    public string HashSalt { get; set; }
    public int PageCount { get; set; }
}