namespace ITS.Models;

public abstract class BaseStats
{
    public int ClosedTasks { get; set; }
    public int PublicTasks { get; set; }
    public WorkTask MostActiveTask { get; set; }
    public int NumberOfComments { get; set; }
}