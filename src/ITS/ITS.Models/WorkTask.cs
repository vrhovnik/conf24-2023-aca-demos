namespace ITS.Models;

public class WorkTask
{
    public string WorkTaskId { get; set; }
    public ItsUser User { get; set; }
    public string Description { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool IsPublic { get; set; }
    public bool IsCompleted { get; set; }
    public string Duration
    {
        get
        {
            var duration = End - Start;
            var info = duration.Days == 0
                ? duration.Minutes == 0 ? $"{duration.Seconds} seconds" : $"{duration.Minutes} minutes"
                : $"{duration.Days} days";

            return info;
        }
    }

    public List<Tag> Tags { get; set; } = new();
    public Category Category { get; set; } = new();
    public List<WorkTaskComment> Comments { get; set; } = new();
}