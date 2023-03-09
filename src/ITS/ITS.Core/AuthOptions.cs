using System.ComponentModel.DataAnnotations;

namespace ITS.Core;

public class AuthOptions
{
    public const string ApiKeyHeaderName = "X-Api-Key";
    [Required(ErrorMessage = "Api key must be defined")] 
    public string ApiKey { get; set; }
    [Required(ErrorMessage = "Hash Salt is required in order to call API correctly")]
    public string HashSalt { get; set; }
}