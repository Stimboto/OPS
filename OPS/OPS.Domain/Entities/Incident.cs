using OPS.Domain.Enums;

namespace OPS.Domain.Entities;

public class Incident
{
    public int Id { get; set; }
    public string TrackingId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public IncidentPriority Priority { get; set; }
    public IncidentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // SLA Fields
    public DateTime ResponseDueAt { get; set; }
    public DateTime ResolutionDueAt { get; set; }
    public DateTime? ResponseAt { get; set; }
    public bool ResponseSlaBreached { get; set; }
    public bool ResolutionSlaBreached { get; set; }
    public DateTime? EscalatedAt { get; set; }
    public DateTime? ResponseSlaWarningSentAt { get; set; }
    public DateTime? ResolutionSlaWarningSentAt { get; set; }

    public int ReportedByUserId { get; set; }
    public User ReportedByUser { get; set; } = null!;

    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    public int? TeamId { get; set; }
    public Team? Team { get; set; }

    public ICollection<IncidentHistory> History { get; set; } = new List<IncidentHistory>();
    public ICollection<IncidentAttachment> Attachments { get; set; } = new List<IncidentAttachment>();
    public ICollection<IncidentComment> Comments { get; set; } = new List<IncidentComment>();
}
