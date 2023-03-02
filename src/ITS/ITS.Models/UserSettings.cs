namespace ITS.Models;

public class UserSettings : ContentModel
{
    public bool EmailNotification { get; set; }
    public ItsUser User { get; set; }
}