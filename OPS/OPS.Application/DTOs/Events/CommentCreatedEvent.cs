namespace OPS.Application.DTOs.Events;

public class CommentCreatedEvent
{
    public int IncidentId { get; set; }
    public int CommentId { get; set; }
    public string TrackingId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
}
