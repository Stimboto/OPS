using OPS.Application.DTOs.Activity;

namespace OPS.Application.Interfaces;

public interface IActivityFeedService
{
    Task<IEnumerable<ActivityFeedDto>> GetIncidentActivityAsync(int incidentId, int userId);
}
