namespace OPS.Application.DTOs.Activity;

public class ActivityFeedDto
{
    public string Type { get; set; } = string.Empty; // "History", "Comment", "Attachment"
    public int? Id { get; set; } // The ID of the comment or attachment, if applicable
    public string Actor { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
