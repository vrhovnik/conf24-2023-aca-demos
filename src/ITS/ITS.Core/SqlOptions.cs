using System.ComponentModel.DataAnnotations;

namespace ITS.Core;

public class SqlOptions : BaseSettings 
{
    public SqlOptions() => SectionName = nameof(SqlOptions);

    [Required]
    public string ConnectionString { get; set; }
}