using Microsoft.EntityFrameworkCore;
using OPS.Application.DTOs.Attachments;
using OPS.Application.DTOs.Events;
using OPS.Application.Interfaces;
using OPS.Domain.Entities;
using OPS.Infrastructure.Data;

namespace OPS.Infrastructure.Services;

public class AttachmentService : IAttachmentService
{
    private readonly OpsDbContext _context;
    private readonly IRealtimeNotificationService _realtimeService;
    private readonly INotificationService _notificationService;
    private readonly string _uploadDirectory = "/app/uploads";

    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };
    private readonly long _maxFileSize = 10 * 1024 * 1024; // 10 MB

    public AttachmentService(OpsDbContext context, IRealtimeNotificationService realtimeService, INotificationService notificationService)
    {
        _context = context;
        _realtimeService = realtimeService;
        _notificationService = notificationService;
        
        if (!Directory.Exists(_uploadDirectory))
        {
            Directory.CreateDirectory(_uploadDirectory);
        }
    }

    private IQueryable<Incident> GetAuthorizedIncidentQuery(int userId)
    {
        var user = _context.Users
            .Include(u => u.Role)
            .FirstOrDefault(u => u.Id == userId);

        if (user == null) return _context.Incidents.Where(i => false);

        var roleName = user.Role.Name;

        if (roleName == "Admin") return _context.Incidents;

        if (roleName == "Manager")
        {
            var managerTeamIds = _context.UserTeams
                .Where(ut => ut.UserId == userId)
                .Select(ut => ut.TeamId)
                .ToList();

            return _context.Incidents.Where(i => i.TeamId.HasValue && managerTeamIds.Contains(i.TeamId.Value));
        }

        if (roleName == "Responder") return _context.Incidents.Where(i => i.AssignedToUserId == userId);

        if (roleName == "Reporter") return _context.Incidents.Where(i => i.ReportedByUserId == userId);

        return _context.Incidents.Where(i => false);
    }

    public async Task<IEnumerable<AttachmentDto>> GetAttachmentsAsync(int incidentId, int userId)
    {
        var isAuthorized = await GetAuthorizedIncidentQuery(userId).AnyAsync(i => i.Id == incidentId);
        if (!isAuthorized) throw new UnauthorizedAccessException("Not authorized to view attachments for this incident.");

        return await _context.IncidentAttachments
            .AsNoTracking()
            .Where(a => a.IncidentId == incidentId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AttachmentDto
            {
                Id = a.Id,
                IncidentId = a.IncidentId,
                UploadedByUserId = a.UploadedByUserId,
                UploadedByUserName = a.UploadedByUser.FullName,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<AttachmentDto> UploadAttachmentAsync(int incidentId, int userId, Stream fileStream, string fileName, string contentType, long fileLength)
    {
        if (fileStream == null || fileLength == 0) throw new ArgumentException("File is empty.");
        if (fileLength > _maxFileSize) throw new ArgumentException("File exceeds maximum allowed size of 10MB.");

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(ext)) throw new ArgumentException("Invalid file extension.");

        var incident = await GetAuthorizedIncidentQuery(userId).FirstOrDefaultAsync(i => i.Id == incidentId);
        if (incident == null) throw new UnauthorizedAccessException("Not authorized to upload to this incident.");

        var safeFileName = Path.GetFileName(fileName); // Prevent directory traversal
        var serverFileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(_uploadDirectory, serverFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream);
        }

        var attachment = new IncidentAttachment
        {
            IncidentId = incidentId,
            UploadedByUserId = userId,
            FileName = safeFileName, // Display name
            FilePath = serverFileName, // Physical filename on disk
            ContentType = contentType,
            FileSize = fileLength,
            CreatedAt = DateTime.UtcNow
        };

        _context.IncidentAttachments.Add(attachment);
        
        var user = await _context.Users.FindAsync(userId);
        
        await _context.SaveChangesAsync();

        var dtoResult = new AttachmentDto
        {
            Id = attachment.Id,
            IncidentId = incidentId,
            UploadedByUserId = userId,
            UploadedByUserName = user!.FullName,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            CreatedAt = attachment.CreatedAt
        };

        // Notify
        var targetUserIds = await GetAuthorizedUsersForIncidentAsync(incidentId, userId);
        
        var eventDto = new AttachmentUploadedEvent
        {
            IncidentId = incidentId,
            AttachmentId = attachment.Id,
            TrackingId = incident.TrackingId,
            UserId = userId,
            UserName = dtoResult.UploadedByUserName,
            FileName = attachment.FileName
        };

        foreach (var tUser in targetUserIds)
        {
            var notif = new Notification
            {
                UserId = tUser,
                Title = "New Evidence",
                Message = $"{dtoResult.UploadedByUserName} uploaded {attachment.FileName} to {incident.TrackingId}.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            _context.Notifications.Add(notif);
        }
        
        // Audit Log
        _context.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "AttachmentUploaded",
            EntityType = "IncidentAttachment",
            EntityId = attachment.Id.ToString(),
            Details = $"Uploaded {safeFileName} to {incident.TrackingId}",
            CreatedAt = DateTime.UtcNow
        });
        
        await _context.SaveChangesAsync();

        await _realtimeService.SendAttachmentUploadedAsync(eventDto, targetUserIds.Concat(new[] { userId }));

        return dtoResult;
    }

    public async Task<(byte[] FileBytes, string ContentType, string FileName)> DownloadAttachmentAsync(int attachmentId, int userId)
    {
        var attachment = await _context.IncidentAttachments.FirstOrDefaultAsync(a => a.Id == attachmentId);
        if (attachment == null) throw new FileNotFoundException("Attachment not found.");

        var isAuthorized = await GetAuthorizedIncidentQuery(userId).AnyAsync(i => i.Id == attachment.IncidentId);
        if (!isAuthorized) throw new UnauthorizedAccessException("Not authorized to download this attachment.");

        var filePath = Path.Combine(_uploadDirectory, attachment.FilePath);
        if (!File.Exists(filePath)) throw new FileNotFoundException("Physical file not found on server.");

        var bytes = await File.ReadAllBytesAsync(filePath);
        return (bytes, attachment.ContentType, attachment.FileName);
    }

    public async Task DeleteAttachmentAsync(int attachmentId, int userId)
    {
        var attachment = await _context.IncidentAttachments.FirstOrDefaultAsync(a => a.Id == attachmentId);
        if (attachment == null) throw new Exception("Attachment not found.");

        var isAuthorized = await GetAuthorizedIncidentQuery(userId).AnyAsync(i => i.Id == attachment.IncidentId);
        if (!isAuthorized) throw new UnauthorizedAccessException();

        var role = await _context.Users.Include(u => u.Role).Where(u => u.Id == userId).Select(u => u.Role.Name).FirstOrDefaultAsync();

        if (attachment.UploadedByUserId != userId && role != "Admin")
        {
            throw new UnauthorizedAccessException("You can only delete your own attachments.");
        }

        var filePath = Path.Combine(_uploadDirectory, attachment.FilePath);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        _context.IncidentAttachments.Remove(attachment);
        
        _context.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "AttachmentDeleted",
            EntityType = "IncidentAttachment",
            EntityId = attachment.Id.ToString(),
            Details = $"Deleted {attachment.FileName} from Incident {attachment.IncidentId}",
            CreatedAt = DateTime.UtcNow
        });
        
        await _context.SaveChangesAsync();
    }

    private async Task<List<int>> GetAuthorizedUsersForIncidentAsync(int incidentId, int excludeUserId)
    {
        var incident = await _context.Incidents.FindAsync(incidentId);
        if (incident == null) return new List<int>();

        var userIds = new HashSet<int>();

        var admins = await _context.Users.Include(u => u.Role).Where(u => u.Role.Name == "Admin").Select(u => u.Id).ToListAsync();
        foreach (var a in admins) userIds.Add(a);

        userIds.Add(incident.ReportedByUserId);
        if (incident.AssignedToUserId.HasValue) userIds.Add(incident.AssignedToUserId.Value);

        if (incident.TeamId.HasValue)
        {
            var managers = await _context.UserTeams
                .Include(ut => ut.User).ThenInclude(u => u.Role)
                .Where(ut => ut.TeamId == incident.TeamId.Value && ut.User.Role.Name == "Manager")
                .Select(ut => ut.UserId)
                .ToListAsync();
            
            foreach (var m in managers) userIds.Add(m);
        }

        userIds.Remove(excludeUserId);
        return userIds.ToList();
    }
}
