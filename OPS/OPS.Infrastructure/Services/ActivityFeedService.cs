using Microsoft.EntityFrameworkCore;
using OPS.Application.DTOs.Activity;
using OPS.Application.Interfaces;
using OPS.Domain.Entities;
using OPS.Infrastructure.Data;

namespace OPS.Infrastructure.Services;

public class ActivityFeedService : IActivityFeedService
{
    private readonly OpsDbContext _context;

    public ActivityFeedService(OpsDbContext context)
    {
        _context = context;
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

    public async Task<IEnumerable<ActivityFeedDto>> GetIncidentActivityAsync(int incidentId, int userId)
    {
        var isAuthorized = await GetAuthorizedIncidentQuery(userId).AnyAsync(i => i.Id == incidentId);
        if (!isAuthorized) throw new UnauthorizedAccessException("Not authorized to view activity for this incident.");

        var activities = new List<ActivityFeedDto>();

        // 1. History
        var history = await _context.IncidentHistories
            .Include(h => h.ChangedByUser)
            .Where(h => h.IncidentId == incidentId)
            .AsNoTracking()
            .ToListAsync();

        foreach (var h in history)
        {
            activities.Add(new ActivityFeedDto
            {
                Type = "History",
                Actor = h.ChangedByUser != null ? h.ChangedByUser.FullName : "System",
                Action = $"changed status from {h.OldStatus} to {h.NewStatus}",
                Details = h.Remarks,
                Timestamp = h.ChangedAt
            });
        }

        // 2. Comments
        var comments = await _context.IncidentComments
            .Include(c => c.User)
            .Where(c => c.IncidentId == incidentId && !c.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        foreach (var c in comments)
        {
            activities.Add(new ActivityFeedDto
            {
                Type = "Comment",
                Id = c.Id,
                Actor = c.User.FullName,
                Action = "added a comment",
                Details = c.CommentText, // The UI can truncate or format this
                Timestamp = c.CreatedAt
            });
        }

        // 3. Attachments
        var attachments = await _context.IncidentAttachments
            .Include(a => a.UploadedByUser)
            .Where(a => a.IncidentId == incidentId)
            .AsNoTracking()
            .ToListAsync();

        foreach (var a in attachments)
        {
            activities.Add(new ActivityFeedDto
            {
                Type = "Attachment",
                Id = a.Id,
                Actor = a.UploadedByUser.FullName,
                Action = "uploaded evidence",
                Details = a.FileName,
                Timestamp = a.CreatedAt
            });
        }

        // 4. Creation Event (since it might not be in history if history only tracks 'changes')
        // Check if history already has "Created"
        if (!history.Any(h => h.Remarks != null && h.Remarks.Equals("Incident Created", StringComparison.OrdinalIgnoreCase)))
        {
            var incident = await _context.Incidents.Include(i => i.ReportedByUser).FirstOrDefaultAsync(i => i.Id == incidentId);
            if (incident != null)
            {
                activities.Add(new ActivityFeedDto
                {
                    Type = "History",
                    Actor = incident.ReportedByUser.FullName,
                    Action = "Created",
                    Details = "Incident reported",
                    Timestamp = incident.CreatedAt
                });
            }
        }

        return activities.OrderByDescending(a => a.Timestamp);
    }
}
