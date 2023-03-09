using System.ComponentModel.DataAnnotations;

namespace ITS.Core;

public class AppOptions : BaseSettings 
{
    public AppOptions() => SectionName = nameof(AppOptions);
    public string DefaultEmailFrom { get; set; }
    [Required(ErrorMessage = "Hash Salt is required in order to call API correctly")]
    public string HashSalt { get; set; }
    public int PageCount { get; set; }
}