using OPS.Application.Interfaces;
using OPS.Domain.Enums;

namespace OPS.Infrastructure.Services;

public class SlaPolicyProvider : ISlaPolicyProvider
{
    public TimeSpan GetResponseSla(IncidentSeverity severity)
    {
        return severity switch
        {
            IncidentSeverity.Critical => TimeSpan.FromMinutes(15),
            IncidentSeverity.High => TimeSpan.FromMinutes(30),
            IncidentSeverity.Medium => TimeSpan.FromHours(2),
            IncidentSeverity.Low => TimeSpan.FromHours(8),
            _ => TimeSpan.FromHours(24)
        };
    }

    public TimeSpan GetResolutionSla(IncidentSeverity severity)
    {
        return severity switch
        {
            IncidentSeverity.Critical => TimeSpan.FromMinutes(60),
            IncidentSeverity.High => TimeSpan.FromHours(4),
            IncidentSeverity.Medium => TimeSpan.FromHours(8),
            IncidentSeverity.Low => TimeSpan.FromHours(24),
            _ => TimeSpan.FromHours(48)
        };
    }
}
