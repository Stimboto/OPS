using OPS.Domain.Enums;

namespace OPS.Domain.Entities;

public class IncidentHistory
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public Incident Incident { get; set; } = null!;

    public IncidentStatus OldStatus { get; set; }
    public IncidentStatus NewStatus { get; set; }
    
    public string Remarks { get; set; } = string.Empty;
    
    public int? ChangedByUserId { get; set; }
    public User? ChangedByUser { get; set; }
    
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
