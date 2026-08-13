using OPS.Domain.Enums;

namespace OPS.Application.DTOs.Events;

public class IncidentCreatedEvent
{
    public int IncidentId { get; set; }
    public string TrackingId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public IncidentPriority Priority { get; set; }
    public IncidentStatus Status { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class IncidentAssignedEvent
{
    public int IncidentId { get; set; }
    public string TrackingId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int AssignedResponderId { get; set; }
    public string AssignedResponderName { get; set; } = string.Empty;
    public string AssignedBy { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}

public class IncidentStatusChangedEvent
{
    public int IncidentId { get; set; }
    public string TrackingId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public IncidentStatus OldStatus { get; set; }
    public IncidentStatus NewStatus { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string Remarks { get; set; } = string.Empty;
}
