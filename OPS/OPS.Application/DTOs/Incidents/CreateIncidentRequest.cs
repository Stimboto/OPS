using System.ComponentModel.DataAnnotations;
using OPS.Domain.Enums;

namespace OPS.Application.DTOs.Incidents;

public class CreateIncidentRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public IncidentSeverity Severity { get; set; }

    [Required]
    public IncidentPriority Priority { get; set; }

    public int? TeamId { get; set; }
}
