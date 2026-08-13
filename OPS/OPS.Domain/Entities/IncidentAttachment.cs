namespace OPS.Domain.Entities;

public class IncidentAttachment
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public Incident Incident { get; set; } = null!;
    
    public int UploadedByUserId { get; set; }
    public User UploadedByUser { get; set; } = null!;
    
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
