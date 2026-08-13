using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPS.Application.DTOs.Comments;
using OPS.Application.Interfaces;
using System.Security.Claims;

namespace OPS.API.Controllers;

[ApiController]
[Authorize]
public class IncidentCommentsController : ControllerBase
{
    private readonly IIncidentCommentService _commentService;

    public IncidentCommentsController(IIncidentCommentService commentService)
    {
        _commentService = commentService;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }

    [HttpGet("api/incidents/{incidentId}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetComments(int incidentId)
    {
        try
        {
            var comments = await _commentService.GetCommentsAsync(incidentId, GetCurrentUserId());
            return Ok(comments);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
    }

    [HttpPost("api/incidents/{incidentId}/comments")]
    public async Task<ActionResult<CommentDto>> CreateComment(int incidentId, [FromBody] CreateCommentDto dto)
    {
        try
        {
            var comment = await _commentService.CreateCommentAsync(incidentId, GetCurrentUserId(), dto);
            return CreatedAtAction(nameof(GetComments), new { incidentId = incidentId }, comment);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
    }

    [HttpPut("api/incidents/{incidentId}/comments/{commentId}")]
    public async Task<ActionResult<CommentDto>> UpdateComment(int incidentId, int commentId, [FromBody] UpdateCommentDto dto)
    {
        try
        {
            var comment = await _commentService.UpdateCommentAsync(incidentId, commentId, GetCurrentUserId(), dto);
            return Ok(comment);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("api/incidents/{incidentId}/comments/{commentId}")]
    public async Task<ActionResult> DeleteComment(int incidentId, int commentId)
    {
        try
        {
            await _commentService.DeleteCommentAsync(incidentId, commentId, GetCurrentUserId());
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
}
