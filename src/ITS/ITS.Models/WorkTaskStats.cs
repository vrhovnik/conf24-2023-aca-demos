namespace ITS.Models;

public class WorkTaskStats : BaseStats
{
    public string WorkTaskStatId { get; set; }
    public int DailyTasks { get; set; }
    public DateTime DateCreated { get; set; }
}