using Microsoft.EntityFrameworkCore;
using OPS.Application.DTOs.Events;
using OPS.Application.DTOs.Incidents;
using OPS.Application.DTOs.Notifications;
using OPS.Application.Interfaces;
using OPS.Domain.Entities;
using OPS.Domain.Enums;
using OPS.Infrastructure.Data;

namespace OPS.Infrastructure.Services;

public class IncidentService : IIncidentService
{
    private readonly OpsDbContext _context;
    private readonly IRealtimeNotificationService _realtime;
    private readonly ISlaPolicyProvider _slaPolicy;

    public IncidentService(OpsDbContext context, IRealtimeNotificationService realtime, ISlaPolicyProvider slaPolicy)
    {
        _context = context;
        _realtime = realtime;
        _slaPolicy = slaPolicy;
    }

    public async Task<IncidentDetailDto> CreateIncidentAsync(CreateIncidentRequest request, int userId)
    {
        var teamExists = await _context.Teams.AnyAsync(t => t.Id == request.TeamId);
        if (!teamExists) throw new ArgumentException("The specified Team does not exist.");

        var user = await _context.Users.FindAsync(userId);
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("User not found or inactive.");

        var year = DateTime.UtcNow.Year;
        string trackingId = string.Empty;
        
        Incident? newIncident = null;
        int retries = 3;

        while (retries > 0)
        {
            var maxIncident = await _context.Incidents
                .Where(i => i.TrackingId.StartsWith($"OPS-{year}-"))
                .OrderByDescending(i => i.TrackingId)
                .FirstOrDefaultAsync();

            int nextSequence = 1;
            if (maxIncident != null)
            {
                var parts = maxIncident.TrackingId.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastSequence))
                {
                    nextSequence = lastSequence + 1;
                }
            }

            trackingId = $"OPS-{year}-{nextSequence:D6}";

            newIncident = new Incident
            {
                TrackingId = trackingId,
                Title = request.Title,
                Description = request.Description,
                Severity = request.Severity,
                Priority = request.Priority,
                TeamId = request.TeamId,
                Status = IncidentStatus.Open,
                ReportedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                ResponseDueAt = DateTime.UtcNow.Add(_slaPolicy.GetResponseSla(request.Severity)),
                ResolutionDueAt = DateTime.UtcNow.Add(_slaPolicy.GetResolutionSla(request.Severity))
            };

            var history = new IncidentHistory
            {
                OldStatus = IncidentStatus.Open,
                NewStatus = IncidentStatus.Open,
                Remarks = "Incident Created",
                ChangedByUserId = userId,
                ChangedAt = DateTime.UtcNow
            };

            newIncident.History.Add(history);
            _context.Incidents.Add(newIncident);

            try
            {
                await _context.SaveChangesAsync();
                break; // Success
            }
            catch (DbUpdateException ex)
            {
                _context.Incidents.Remove(newIncident); // Reset tracking
                retries--;
                if (retries == 0)
                    throw new Exception("Failed to generate a unique Tracking ID after multiple attempts.", ex);
            }
        }

        // Prepare event and target users
        var createdEvent = new IncidentCreatedEvent
        {
            IncidentId = newIncident!.Id,
            TrackingId = newIncident.TrackingId,
            Title = newIncident.Title,
            Severity = newIncident.Severity,
            Priority = newIncident.Priority,
            Status = newIncident.Status,
            ReporterName = user.FullName,
            CreatedAt = newIncident.CreatedAt
        };

        var targetUsers = await GetTargetUsersForIncident(newIncident);
        
        try 
        {
            await _realtime.SendIncidentCreatedAsync(createdEvent, targetUsers);
        } catch { /* SignalR failure must NEVER roll back or prevent the core incident operation */ }

        return await GetIncidentAsync(newIncident!.Id, userId, "Admin");
    }

    public async Task<IEnumerable<IncidentListDto>> GetIncidentsAsync(int userId, string userRole)
    {
        var query = _context.Incidents
            .Include(i => i.Team)
            .Include(i => i.ReportedByUser)
            .Include(i => i.AssignedToUser)
            .AsNoTracking()
            .AsQueryable();

        if (userRole == "Reporter")
            query = query.Where(i => i.ReportedByUserId == userId);
        else if (userRole == "Responder")
            query = query.Where(i => i.AssignedToUserId == userId);
        else if (userRole == "Manager")
        {
            var userTeamIds = await _context.UserTeams.Where(ut => ut.UserId == userId).Select(ut => ut.TeamId).ToListAsync();
            query = query.Where(i => i.TeamId.HasValue && userTeamIds.Contains(i.TeamId.Value));
        }

        var incidents = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();

        return incidents.Select(i => new IncidentListDto
        {
            Id = i.Id,
            TrackingId = i.TrackingId,
            Title = i.Title,
            Severity = i.Severity,
            Priority = i.Priority,
            Status = i.Status,
            TeamName = i.Team != null ? i.Team.Name : null,
            ReporterName = i.ReportedByUser.FullName,
            AssignedResponderName = i.AssignedToUser != null ? i.AssignedToUser.FullName : null,
            CreatedAt = i.CreatedAt,
            UpdatedAt = i.UpdatedAt,
            ResolutionDueAt = i.ResolutionDueAt,
            ResponseSlaBreached = i.ResponseSlaBreached,
            ResolutionSlaBreached = i.ResolutionSlaBreached
        });
    }

    public async Task<IncidentDetailDto> GetIncidentAsync(int incidentId, int userId, string userRole)
    {
        var incident = await _context.Incidents
            .Include(i => i.Team)
            .Include(i => i.ReportedByUser)
            .Include(i => i.AssignedToUser)
            .Include(i => i.History)
                .ThenInclude(h => h.ChangedByUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == incidentId);

        if (incident == null)
            throw new KeyNotFoundException("Incident not found.");

        if (userRole == "Reporter" && incident.ReportedByUserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to view this incident.");
        if (userRole == "Responder" && incident.AssignedToUserId != userId)
            throw new UnauthorizedAccessException("You are not authorized to view this incident.");
        if (userRole == "Manager")
        {
            var isMember = await _context.UserTeams.AnyAsync(ut => ut.UserId == userId && ut.TeamId == incident.TeamId);
            if (!isMember) throw new UnauthorizedAccessException("You are not authorized to view this incident.");
        }

        return new IncidentDetailDto
        {
            Id = incident.Id,
            TrackingId = incident.TrackingId,
            Title = incident.Title,
            Description = incident.Description,
            Severity = incident.Severity,
            Priority = incident.Priority,
            Status = incident.Status,
            TeamName = incident.Team?.Name,
            ReportedByUserId = incident.ReportedByUserId,
            ReporterName = incident.ReportedByUser.FullName,
            AssignedToUserId = incident.AssignedToUserId,
            AssignedResponderName = incident.AssignedToUser?.FullName,
            CreatedAt = incident.CreatedAt,
            UpdatedAt = incident.UpdatedAt,
            ResolvedAt = incident.ResolvedAt,
            ResponseDueAt = incident.ResponseDueAt,
            ResolutionDueAt = incident.ResolutionDueAt,
            ResponseAt = incident.ResponseAt,
            ResponseSlaBreached = incident.ResponseSlaBreached,
            ResolutionSlaBreached = incident.ResolutionSlaBreached,
            EscalatedAt = incident.EscalatedAt,
            History = incident.History.OrderByDescending(h => h.ChangedAt).Select(h => new IncidentHistoryDto
            {
                OldStatus = h.OldStatus,
                NewStatus = h.NewStatus,
                Remarks = h.Remarks,
                ChangedByUserName = h.ChangedByUser?.FullName ?? "System",
                ChangedAt = h.ChangedAt
            }).ToList()
        };
    }

    public async Task AssignIncidentAsync(int incidentId, AssignIncidentRequest request, int userId, string userRole)
    {
        if (userRole != "Admin" && userRole != "Manager")
            throw new UnauthorizedAccessException("Only Managers and Admins can assign incidents.");

        var incident = await _context.Incidents.Include(i => i.ReportedByUser).FirstOrDefaultAsync(i => i.Id == incidentId);
        if (incident == null)
            throw new KeyNotFoundException("Incident not found.");

        var responder = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == request.ResponderId);
        if (responder == null || !responder.IsActive || responder.Role.Name != "Responder")
            throw new ArgumentException("Assigned user must be an active Responder.");

        if (userRole != "Admin")
        {
            if (incident.TeamId == null) throw new InvalidOperationException("Incident does not belong to a team.");
            var isManagerInTeam = await _context.UserTeams.AnyAsync(ut => ut.UserId == userId && ut.TeamId == incident.TeamId.Value);
            if (!isManagerInTeam) throw new UnauthorizedAccessException("You are not authorized to assign this incident.");
        }

        if (incident.TeamId != null)
        {
            var isResponderInTeam = await _context.UserTeams.AnyAsync(ut => ut.UserId == request.ResponderId && ut.TeamId == incident.TeamId.Value);
            if (!isResponderInTeam) throw new InvalidOperationException("The targeted responder does not belong to the incident's team.");
        }

        var assigner = await _context.Users.FindAsync(userId);
        var oldStatus = incident.Status;
        
        if (incident.Status == IncidentStatus.Open)
            incident.Status = IncidentStatus.Assigned;

        incident.AssignedToUserId = responder.Id;
        incident.UpdatedAt = DateTime.UtcNow;

        var history = new IncidentHistory
        {
            IncidentId = incident.Id,
            OldStatus = oldStatus,
            NewStatus = incident.Status,
            Remarks = $"Assigned to {responder.FullName}",
            ChangedByUserId = userId,
            ChangedAt = DateTime.UtcNow
        };
        _context.IncidentHistories.Add(history);

        var notification = new Notification
        {
            UserId = responder.Id,
            Title = "Incident Assigned",
            Message = $"Incident {incident.TrackingId} has been assigned to you.",
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();

        var assignedEvent = new IncidentAssignedEvent
        {
            IncidentId = incident.Id,
            TrackingId = incident.TrackingId,
            Title = incident.Title,
            AssignedResponderId = responder.Id,
            AssignedResponderName = responder.FullName,
            AssignedBy = assigner?.FullName ?? "System",
            AssignedAt = DateTime.UtcNow
        };

        var targetUsers = await GetTargetUsersForIncident(incident);
        
        try 
        {
            await _realtime.SendIncidentAssignedAsync(assignedEvent, targetUsers);
            await _realtime.SendNotificationCreatedAsync(new NotificationDto 
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead
            }, responder.Id);
        } catch { }
    }

    public async Task UpdateIncidentStatusAsync(int incidentId, UpdateIncidentStatusRequest request, int userId, string userRole)
    {
        var incident = await _context.Incidents.FindAsync(incidentId);
        if (incident == null)
            throw new KeyNotFoundException("Incident not found.");

        if (userRole == "Responder" && incident.AssignedToUserId != userId)
            throw new UnauthorizedAccessException("You can only update status for incidents assigned to you.");
        if (userRole == "Reporter")
            throw new UnauthorizedAccessException("Reporters cannot update incident status.");

        var oldStatus = incident.Status;
        var newStatus = request.Status;

        if (oldStatus == newStatus) return;

        if (!IsValidTransition(oldStatus, newStatus))
            throw new ArgumentException($"Invalid status transition from {oldStatus} to {newStatus}.");

        incident.Status = newStatus;
        incident.UpdatedAt = DateTime.UtcNow;

        if (newStatus == IncidentStatus.Investigating && incident.ResponseAt == null)
        {
            incident.ResponseAt = DateTime.UtcNow;
            incident.ResponseSlaBreached = incident.ResponseAt > incident.ResponseDueAt;
        }

        if (newStatus == IncidentStatus.Resolved || newStatus == IncidentStatus.Closed)
        {
            if (incident.ResolvedAt == null)
            {
                incident.ResolvedAt = DateTime.UtcNow;
                incident.ResolutionSlaBreached = incident.ResolvedAt > incident.ResolutionDueAt;
            }
        }
        else if (newStatus == IncidentStatus.Reopened)
        {
            // Do not corrupt historical SLA information as per requirements
        }

        var updater = await _context.Users.FindAsync(userId);
        var history = new IncidentHistory
        {
            IncidentId = incident.Id,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Remarks = request.Remarks,
            ChangedByUserId = userId,
            ChangedAt = DateTime.UtcNow
        };
        _context.IncidentHistories.Add(history);

        // Notify reporter about status change
        var notification = new Notification
        {
            UserId = incident.ReportedByUserId,
            Title = "Incident Status Changed",
            Message = $"Incident {incident.TrackingId} status changed to {newStatus}.",
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();

        var statusEvent = new IncidentStatusChangedEvent
        {
            IncidentId = incident.Id,
            TrackingId = incident.TrackingId,
            Title = incident.Title,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedBy = updater?.FullName ?? "System",
            ChangedAt = DateTime.UtcNow,
            Remarks = request.Remarks
        };

        var targetUsers = await GetTargetUsersForIncident(incident);
        
        try 
        {
            await _realtime.SendIncidentStatusChangedAsync(statusEvent, targetUsers);
            await _realtime.SendNotificationCreatedAsync(new NotificationDto 
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                IsRead = notification.IsRead
            }, incident.ReportedByUserId);
        } catch { }
    }

    private async Task<List<int>> GetTargetUsersForIncident(Incident incident)
    {
        // Reporter
        var targets = new HashSet<int> { incident.ReportedByUserId };
        
        // Responder
        if (incident.AssignedToUserId.HasValue)
            targets.Add(incident.AssignedToUserId.Value);

        // Managers and Admins
        var managersAdmins = await _context.Users
            .Include(u => u.Role)
            .Where(u => u.Role.Name == "Manager" || u.Role.Name == "Admin")
            .Select(u => u.Id)
            .ToListAsync();
            
        foreach(var id in managersAdmins) targets.Add(id);

        return targets.ToList();
    }

    private bool IsValidTransition(IncidentStatus current, IncidentStatus next)
    {
        if (next == IncidentStatus.Escalated) return true; // Can escalate from any operational state

        return current switch
        {
            IncidentStatus.Open => next == IncidentStatus.Assigned,
            IncidentStatus.Assigned => next == IncidentStatus.Investigating,
            IncidentStatus.Investigating => next == IncidentStatus.Mitigating,
            IncidentStatus.Mitigating => next == IncidentStatus.Resolved,
            IncidentStatus.Resolved => next == IncidentStatus.Closed || next == IncidentStatus.Reopened,
            IncidentStatus.Reopened => next == IncidentStatus.Investigating,
            IncidentStatus.Closed => false, // Terminal state
            IncidentStatus.Escalated => next == IncidentStatus.Investigating || next == IncidentStatus.Mitigating || next == IncidentStatus.Resolved,
            _ => false
        };
    }

    public async Task TimeTravelIncidentSlaAsync(int incidentId, int minutesToAdvance)
    {
        var incident = await _context.Incidents.FindAsync(incidentId);
        if (incident == null) throw new KeyNotFoundException("Incident not found.");

        // Fast-forward time for this incident by artificially rewinding its Due dates and CreatedAt
        var advanceTime = TimeSpan.FromMinutes(minutesToAdvance);
        
        incident.CreatedAt = incident.CreatedAt.Subtract(advanceTime);
        incident.ResponseDueAt = incident.ResponseDueAt.Subtract(advanceTime);
        incident.ResolutionDueAt = incident.ResolutionDueAt.Subtract(advanceTime);

        // Also rewind tracking times so warnings can trigger
        if (incident.ResponseSlaWarningSentAt.HasValue) 
            incident.ResponseSlaWarningSentAt = incident.ResponseSlaWarningSentAt.Value.Subtract(advanceTime);
        if (incident.ResolutionSlaWarningSentAt.HasValue) 
            incident.ResolutionSlaWarningSentAt = incident.ResolutionSlaWarningSentAt.Value.Subtract(advanceTime);

        await _context.SaveChangesAsync();
    }
}
