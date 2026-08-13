using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPS.Application.DTOs.Attachments;
using OPS.Application.Interfaces;
using System.Security.Claims;

namespace OPS.API.Controllers;

[ApiController]
[Authorize]
public class IncidentAttachmentsController : ControllerBase
{
    private readonly IAttachmentService _attachmentService;

    public IncidentAttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }

    [HttpGet("api/incidents/{incidentId}/attachments")]
    public async Task<ActionResult<IEnumerable<AttachmentDto>>> GetAttachments(int incidentId)
    {
        try
        {
            var attachments = await _attachmentService.GetAttachmentsAsync(incidentId, GetCurrentUserId());
            return Ok(attachments);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
    }

    [HttpPost("api/incidents/{incidentId}/attachments")]
    public async Task<ActionResult<AttachmentDto>> UploadAttachment(int incidentId, IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var attachment = await _attachmentService.UploadAttachmentAsync(incidentId, GetCurrentUserId(), stream, file.FileName, file.ContentType, file.Length);
            return CreatedAtAction(nameof(GetAttachments), new { incidentId = incidentId }, attachment);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("api/attachments/{attachmentId}")]
    public async Task<IActionResult> DownloadAttachment(int attachmentId)
    {
        try
        {
            var (fileBytes, contentType, fileName) = await _attachmentService.DownloadAttachmentAsync(attachmentId, GetCurrentUserId());
            return File(fileBytes, contentType, fileName);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("api/attachments/{attachmentId}")]
    public async Task<ActionResult> DeleteAttachment(int attachmentId)
    {
        try
        {
            await _attachmentService.DeleteAttachmentAsync(attachmentId, GetCurrentUserId());
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
