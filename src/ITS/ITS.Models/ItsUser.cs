namespace ITS.Models;

public class ItsUser
{
    public string ItsUserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public List<WorkTask> Tasks { get; set; }
    public UserSettings UserSettings { get; set; }
}