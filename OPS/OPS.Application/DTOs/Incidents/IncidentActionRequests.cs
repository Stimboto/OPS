using System.ComponentModel.DataAnnotations;
using OPS.Domain.Enums;

namespace OPS.Application.DTOs.Incidents;

public class AssignIncidentRequest
{
    [Required]
    public int ResponderId { get; set; }
}

public class UpdateIncidentStatusRequest
{
    [Required]
    public IncidentStatus Status { get; set; }

    [Required]
    [MaxLength(500)]
    public string Remarks { get; set; } = string.Empty;
}
