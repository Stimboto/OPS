namespace OPS.Application.DTOs.Attachments;

public class AttachmentDto
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public int UploadedByUserId { get; set; }
    public string UploadedByUserName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
}
