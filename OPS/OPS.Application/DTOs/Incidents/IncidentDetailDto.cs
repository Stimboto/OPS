using OPS.Domain.Enums;

namespace OPS.Application.DTOs.Incidents;

public class IncidentDetailDto
{
    public int Id { get; set; }
    public string TrackingId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public IncidentPriority Priority { get; set; }
    public IncidentStatus Status { get; set; }
    public string? TeamName { get; set; }
    public int ReportedByUserId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public int? AssignedToUserId { get; set; }
    public string? AssignedResponderName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    
    // SLA Fields
    public DateTime ResponseDueAt { get; set; }
    public DateTime ResolutionDueAt { get; set; }
    public DateTime? ResponseAt { get; set; }
    public bool ResponseSlaBreached { get; set; }
    public bool ResolutionSlaBreached { get; set; }
    public DateTime? EscalatedAt { get; set; }

    public List<IncidentHistoryDto> History { get; set; } = new();
}

public class IncidentHistoryDto
{
    public IncidentStatus OldStatus { get; set; }
    public IncidentStatus NewStatus { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public string ChangedByUserName { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}
