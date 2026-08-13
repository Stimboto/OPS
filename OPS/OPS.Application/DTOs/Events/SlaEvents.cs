using OPS.Domain.Enums;

namespace OPS.Application.DTOs.Events;

public class SlaWarningEvent
{
    public int IncidentId { get; set; }
    public string TrackingId { get; set; } = string.Empty;
    public string SlaType { get; set; } = string.Empty;
    public DateTime DueAt { get; set; }
    public IncidentSeverity Severity { get; set; }
}

public class SlaBreachedEvent
{
    public int IncidentId { get; set; }
    public string TrackingId { get; set; } = string.Empty;
    public string SlaType { get; set; } = string.Empty;
    public DateTime DueAt { get; set; }
    public DateTime BreachedAt { get; set; }
    public IncidentSeverity Severity { get; set; }
}
