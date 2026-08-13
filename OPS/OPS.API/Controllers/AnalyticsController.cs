using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPS.Application.Interfaces;
using OPS.Application.Models.Analytics;
using System.Security.Claims;

namespace OPS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }

    // --- ADMIN ENDPOINTS ---

    [HttpGet("admin/overview")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OverviewStatsDto>> GetAdminOverview([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var stats = await _analyticsService.GetAdminOverviewAsync(from, to);
        return Ok(stats);
    }

    [HttpGet("admin/incident-volume")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<IncidentVolumeDto>>> GetAdminIncidentVolume([FromQuery] string period = "daily", [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var data = await _analyticsService.GetAdminIncidentVolumeAsync(period, from, to);
        return Ok(data);
    }

    [HttpGet("admin/severity")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SeverityDistributionDto>> GetAdminSeverity([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var stats = await _analyticsService.GetAdminSeverityDistributionAsync(from, to);
        return Ok(stats);
    }

    [HttpGet("admin/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<StatusDistributionDto>> GetAdminStatus([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var stats = await _analyticsService.GetAdminStatusDistributionAsync(from, to);
        return Ok(stats);
    }

    [HttpGet("admin/teams")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<TeamPerformanceDto>>> GetAdminTeams([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var stats = await _analyticsService.GetAdminTeamPerformanceAsync(from, to);
        return Ok(stats);
    }

    [HttpGet("admin/responders")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ResponderPerformanceDto>>> GetAdminResponders([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var stats = await _analyticsService.GetAdminResponderPerformanceAsync(from, to);
        return Ok(stats);
    }

    [HttpGet("admin/sla")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SlaAnalyticsDto>> GetAdminSla([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var stats = await _analyticsService.GetAdminSlaAnalyticsAsync(from, to);
        return Ok(stats);
    }

    [HttpGet("admin/mtta-mttr")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MttaMttrAnalyticsDto>> GetAdminMttaMttr([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var stats = await _analyticsService.GetAdminMttaMttrAsync(from, to);
        return Ok(stats);
    }

    [HttpGet("admin/escalation")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EscalationAnalyticsDto>> GetAdminEscalation([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var stats = await _analyticsService.GetAdminEscalationAnalyticsAsync(from, to);
        return Ok(stats);
    }

    [HttpGet("admin/reopened")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ReopenedAnalyticsDto>> GetAdminReopened([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var stats = await _analyticsService.GetAdminReopenedAnalyticsAsync(from, to);
        return Ok(stats);
    }

    // --- MANAGER ENDPOINTS ---

    [HttpGet("manager/overview")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<OverviewStatsDto>> GetManagerOverview([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var stats = await _analyticsService.GetManagerOverviewAsync(GetCurrentUserId(), from, to);
        return Ok(stats);
    }

    [HttpGet("manager/teams")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<TeamPerformanceDto>>> GetManagerTeams([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var stats = await _analyticsService.GetManagerTeamPerformanceAsync(GetCurrentUserId(), from, to);
        return Ok(stats);
    }

    // --- RESPONDER ENDPOINTS ---

    [HttpGet("responder/overview")]
    [Authorize(Roles = "Admin,Manager,Responder")]
    public async Task<ActionResult<OverviewStatsDto>> GetResponderOverview([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var stats = await _analyticsService.GetResponderOverviewAsync(GetCurrentUserId(), from, to);
        return Ok(stats);
    }
}
