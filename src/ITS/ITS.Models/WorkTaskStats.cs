namespace ITS.Models;

public class WorkTaskStats
{
    public string WorkTaskStatId { get; set; }
    public int DailyTasks { get; set; }
    public int PublicTasks { get; set; }
    public int CommentNumber { get; set; }
    public WorkTask MostActiveTask { get; set; }
    public DateTime DateCreated { get; set; }
}