namespace EventsService.Controllers;

public class UserEvent
{
    public int UserId { get; set; }
    public string Action { get; set; } = string.Empty;
}