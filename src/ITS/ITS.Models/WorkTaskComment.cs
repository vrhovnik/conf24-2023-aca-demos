namespace ITS.Models;

public class WorkTaskComment
{
    public string WorkTaskCommentId { get; set; }
    public string Comment { get; set; }
    public DateTime StartDate { get; set; }
    public WorkTask AssignedTask { get; set; }
    public ItsUser User { get; set; }
}