using OPS.Domain.Enums;

namespace OPS.Application.Interfaces;

public interface ISlaPolicyProvider
{
    TimeSpan GetResponseSla(IncidentSeverity severity);
    TimeSpan GetResolutionSla(IncidentSeverity severity);
}
