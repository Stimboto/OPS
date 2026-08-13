namespace OPS.Application.DTOs.Events;

public class AttachmentUploadedEvent
{
    public int IncidentId { get; set; }
    public int AttachmentId { get; set; }
    public string TrackingId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
