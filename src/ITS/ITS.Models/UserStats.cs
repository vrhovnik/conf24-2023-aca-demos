namespace ITS.Models;

public class UserStats : BaseStats
{
    public ItsUser User { get; set; }
    public int ActiveTasks { get; set; }
}