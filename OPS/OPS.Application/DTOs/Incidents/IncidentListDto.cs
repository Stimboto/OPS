using OPS.Domain.Enums;

namespace OPS.Application.DTOs.Incidents;

public class IncidentListDto
{
    public int Id { get; set; }
    public string TrackingId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public IncidentPriority Priority { get; set; }
    public IncidentStatus Status { get; set; }
    public string? TeamName { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string? AssignedResponderName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolutionDueAt { get; set; }
    public bool ResponseSlaBreached { get; set; }
    public bool ResolutionSlaBreached { get; set; }
}
