using Microsoft.EntityFrameworkCore;
using OPS.Application.DTOs.Comments;
using OPS.Application.DTOs.Events;
using OPS.Application.DTOs.Notifications;
using OPS.Application.Interfaces;
using OPS.Domain.Entities;
using OPS.Infrastructure.Data;

namespace OPS.Infrastructure.Services;

public class IncidentCommentService : IIncidentCommentService
{
    private readonly OpsDbContext _context;
    private readonly IRealtimeNotificationService _realtimeService;
    private readonly INotificationService _notificationService;

    public IncidentCommentService(OpsDbContext context, IRealtimeNotificationService realtimeService, INotificationService notificationService)
    {
        _context = context;
        _realtimeService = realtimeService;
        _notificationService = notificationService;
    }

    private IQueryable<Incident> GetAuthorizedIncidentQuery(int userId)
    {
        var user = _context.Users
            .Include(u => u.Role)
            .FirstOrDefault(u => u.Id == userId);

        if (user == null) return _context.Incidents.Where(i => false);

        var roleName = user.Role.Name;

        if (roleName == "Admin")
        {
            return _context.Incidents;
        }

        if (roleName == "Manager")
        {
            var managerTeamIds = _context.UserTeams
                .Where(ut => ut.UserId == userId)
                .Select(ut => ut.TeamId)
                .ToList();

            return _context.Incidents.Where(i => i.TeamId.HasValue && managerTeamIds.Contains(i.TeamId.Value));
        }

        if (roleName == "Responder")
        {
            return _context.Incidents.Where(i => i.AssignedToUserId == userId);
        }

        if (roleName == "Reporter")
        {
            return _context.Incidents.Where(i => i.ReportedByUserId == userId);
        }

        return _context.Incidents.Where(i => false);
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsAsync(int incidentId, int userId)
    {
        var isAuthorized = await GetAuthorizedIncidentQuery(userId).AnyAsync(i => i.Id == incidentId);
        if (!isAuthorized)
        {
            throw new UnauthorizedAccessException("You are not authorized to view comments for this incident.");
        }

        return await _context.IncidentComments
            .AsNoTracking()
            .Where(c => c.IncidentId == incidentId && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                IncidentId = c.IncidentId,
                UserId = c.UserId,
                UserName = c.User.FullName,
                CommentText = c.CommentText,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<CommentDto> CreateCommentAsync(int incidentId, int userId, CreateCommentDto dto)
    {
        var incident = await GetAuthorizedIncidentQuery(userId).FirstOrDefaultAsync(i => i.Id == incidentId);
        if (incident == null)
        {
            throw new UnauthorizedAccessException("You are not authorized to comment on this incident.");
        }

        var comment = new IncidentComment
        {
            IncidentId = incidentId,
            UserId = userId,
            CommentText = dto.CommentText,
            CreatedAt = DateTime.UtcNow
        };

        _context.IncidentComments.Add(comment);
        
        var user = await _context.Users.FindAsync(userId);
        
        await _context.SaveChangesAsync();

        var dtoResult = new CommentDto
        {
            Id = comment.Id,
            IncidentId = incidentId,
            UserId = userId,
            UserName = user!.FullName,
            CommentText = comment.CommentText,
            CreatedAt = comment.CreatedAt
        };

        // Notify
        // Find who needs to be notified (Admin, Manager of team, Assigned, Reporter) - excluding the sender
        var targetUserIds = await GetAuthorizedUsersForIncidentAsync(incidentId, userId);
        
        var eventDto = new CommentCreatedEvent
        {
            IncidentId = incidentId,
            CommentId = comment.Id,
            TrackingId = incident.TrackingId,
            UserId = userId,
            UserName = dtoResult.UserName
        };

        foreach (var tUser in targetUserIds)
        {
            var notif = new Notification
            {
                UserId = tUser,
                Title = "New Comment",
                Message = $"{dtoResult.UserName} added a comment to {incident.TrackingId}.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            _context.Notifications.Add(notif);
        }
        await _context.SaveChangesAsync();

        await _realtimeService.SendCommentCreatedAsync(eventDto, targetUserIds.Concat(new[] { userId }));

        return dtoResult;
    }

    public async Task<CommentDto> UpdateCommentAsync(int incidentId, int commentId, int userId, UpdateCommentDto dto)
    {
        var isAuthorized = await GetAuthorizedIncidentQuery(userId).AnyAsync(i => i.Id == incidentId);
        if (!isAuthorized) throw new UnauthorizedAccessException();

        var comment = await _context.IncidentComments.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == commentId && c.IncidentId == incidentId && !c.IsDeleted);
        if (comment == null) throw new Exception("Comment not found.");

        var role = await _context.Users.Include(u => u.Role).Where(u => u.Id == userId).Select(u => u.Role.Name).FirstOrDefaultAsync();

        if (comment.UserId != userId && role != "Admin")
        {
            throw new UnauthorizedAccessException("You can only edit your own comments.");
        }

        comment.CommentText = dto.CommentText;
        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);

        return new CommentDto
        {
            Id = comment.Id,
            IncidentId = incidentId,
            UserId = userId,
            UserName = user!.FullName,
            CommentText = comment.CommentText,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }

    public async Task DeleteCommentAsync(int incidentId, int commentId, int userId)
    {
        var isAuthorized = await GetAuthorizedIncidentQuery(userId).AnyAsync(i => i.Id == incidentId);
        if (!isAuthorized) throw new UnauthorizedAccessException();

        var comment = await _context.IncidentComments.FirstOrDefaultAsync(c => c.Id == commentId && c.IncidentId == incidentId && !c.IsDeleted);
        if (comment == null) throw new Exception("Comment not found.");

        var role = await _context.Users.Include(u => u.Role).Where(u => u.Id == userId).Select(u => u.Role.Name).FirstOrDefaultAsync();

        if (comment.UserId != userId && role != "Admin")
        {
            throw new UnauthorizedAccessException("You can only delete your own comments.");
        }

        comment.IsDeleted = true;
        await _context.SaveChangesAsync();
    }

    private async Task<List<int>> GetAuthorizedUsersForIncidentAsync(int incidentId, int excludeUserId)
    {
        var incident = await _context.Incidents.FindAsync(incidentId);
        if (incident == null) return new List<int>();

        var userIds = new HashSet<int>();

        // Admin
        var admins = await _context.Users.Include(u => u.Role).Where(u => u.Role.Name == "Admin").Select(u => u.Id).ToListAsync();
        foreach (var a in admins) userIds.Add(a);

        // Reporter
        userIds.Add(incident.ReportedByUserId);

        // Assigned
        if (incident.AssignedToUserId.HasValue) userIds.Add(incident.AssignedToUserId.Value);

        // Managers of Team
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
