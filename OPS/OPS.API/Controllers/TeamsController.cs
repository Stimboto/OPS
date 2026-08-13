using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPS.Application.Interfaces;
using OPS.Application.Models;
using System.Security.Claims;

namespace OPS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeamListDto>>> GetTeams()
    {
        var teams = await _teamService.GetTeamsAsync();
        return Ok(teams);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TeamDetailDto>> GetTeam(int id)
    {
        try
        {
            var team = await _teamService.GetTeamAsync(id);
            return Ok(team);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<TeamDetailDto>> CreateTeam(CreateTeamRequest request)
    {
        var team = await _teamService.CreateTeamAsync(request, CurrentUserId);
        return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, team);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TeamDetailDto>> UpdateTeam(int id, UpdateTeamRequest request)
    {
        try
        {
            var team = await _teamService.UpdateTeamAsync(id, request, CurrentUserId);
            return Ok(team);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        try
        {
            await _teamService.DeleteTeamAsync(id, CurrentUserId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/members")]
    public async Task<ActionResult<IEnumerable<UserTeamDto>>> GetTeamMembers(int id)
    {
        try
        {
            var members = await _teamService.GetTeamMembersAsync(id);
            return Ok(members);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddTeamMember(int id, [FromBody] int userId)
    {
        try
        {
            await _teamService.AddMemberToTeamAsync(id, userId, CurrentUserId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveTeamMember(int id, int userId)
    {
        try
        {
            await _teamService.RemoveMemberFromTeamAsync(id, userId, CurrentUserId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
