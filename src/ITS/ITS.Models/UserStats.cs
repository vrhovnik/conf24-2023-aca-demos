namespace ITS.Models;

public class UserStats
{
    public ItsUser User { get; set; }
    public int ActiveTasks { get; set; }
    public int ClosedTasks { get; set; }
    public int PublicTasks { get; set; }
    public WorkTask MostActiveTask { get; set; }
    public int NumberOfComments { get; set; }
}