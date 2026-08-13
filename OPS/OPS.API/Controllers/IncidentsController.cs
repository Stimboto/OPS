using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPS.Application.DTOs.Incidents;
using OPS.Application.Interfaces;

namespace OPS.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class IncidentsController : ControllerBase
{
    private readonly IIncidentService _incidentService;

    public IncidentsController(IIncidentService incidentService)
    {
        _incidentService = incidentService;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out int userId))
            return userId;
        throw new UnauthorizedAccessException("Invalid user context.");
    }

    private string GetCurrentUserRole()
    {
        var claim = User.FindFirst(ClaimTypes.Role);
        return claim?.Value ?? string.Empty;
    }

    [HttpPost]
    [Authorize(Policy = "ReporterPolicy")]
    public async Task<ActionResult<IncidentDetailDto>> CreateIncident([FromBody] CreateIncidentRequest request)
    {
        try
        {
            int userId = GetCurrentUserId();
            var incident = await _incidentService.CreateIncidentAsync(request, userId);
            return CreatedAtAction(nameof(GetIncident), new { id = incident.Id }, incident);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<IncidentListDto>>> GetIncidents()
    {
        try
        {
            int userId = GetCurrentUserId();
            string userRole = GetCurrentUserRole();
            var incidents = await _incidentService.GetIncidentsAsync(userId, userRole);
            return Ok(incidents);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IncidentDetailDto>> GetIncident(int id)
    {
        try
        {
            int userId = GetCurrentUserId();
            string userRole = GetCurrentUserRole();
            var incident = await _incidentService.GetIncidentAsync(id, userId, userRole);
            return Ok(incident);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Incident not found." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/assign")]
    [Authorize(Policy = "ManagerPolicy")]
    public async Task<IActionResult> AssignIncident(int id, [FromBody] AssignIncidentRequest request)
    {
        try
        {
            int userId = GetCurrentUserId();
            string userRole = GetCurrentUserRole();
            await _incidentService.AssignIncidentAsync(id, request, userId, userRole);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Incident not found." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateIncidentStatus(int id, [FromBody] UpdateIncidentStatusRequest request)
    {
        try
        {
            int userId = GetCurrentUserId();
            string userRole = GetCurrentUserRole();
            await _incidentService.UpdateIncidentStatusAsync(id, request, userId, userRole);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Incident not found." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/time-travel")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> TimeTravel(int id, [FromQuery] int minutesToAdvance)
    {
        try
        {
            await _incidentService.TimeTravelIncidentSlaAsync(id, minutesToAdvance);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Incident not found." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
