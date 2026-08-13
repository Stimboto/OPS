using OPS.Application.DTOs.Incidents;

namespace OPS.Application.Interfaces;

public interface IIncidentService
{
    Task<IncidentDetailDto> CreateIncidentAsync(CreateIncidentRequest request, int userId);
    Task<IEnumerable<IncidentListDto>> GetIncidentsAsync(int userId, string userRole);
    Task<IncidentDetailDto> GetIncidentAsync(int incidentId, int userId, string userRole);
    Task AssignIncidentAsync(int incidentId, AssignIncidentRequest request, int userId, string userRole);
    Task UpdateIncidentStatusAsync(int incidentId, UpdateIncidentStatusRequest request, int userId, string userRole);
    Task TimeTravelIncidentSlaAsync(int incidentId, int hoursToAdvance);
}
