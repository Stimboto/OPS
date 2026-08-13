using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPS.Application.DTOs.Activity;
using OPS.Application.Interfaces;
using System.Security.Claims;

namespace OPS.API.Controllers;

[ApiController]
[Authorize]
public class IncidentActivityController : ControllerBase
{
    private readonly IActivityFeedService _activityService;

    public IncidentActivityController(IActivityFeedService activityService)
    {
        _activityService = activityService;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }

    [HttpGet("api/incidents/{incidentId}/activity")]
    public async Task<ActionResult<IEnumerable<ActivityFeedDto>>> GetIncidentActivity(int incidentId)
    {
        try
        {
            var activities = await _activityService.GetIncidentActivityAsync(incidentId, GetCurrentUserId());
            return Ok(activities);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
    }
}
