using System.ComponentModel.DataAnnotations;

namespace ITS.Core;

public class SqlOptions 
{
    [Required]
    public string ConnectionString { get; set; }
}