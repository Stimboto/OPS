using Microsoft.EntityFrameworkCore;
using OPS.Application.Interfaces;
using OPS.Application.Models;
using OPS.Domain.Entities;
using OPS.Domain.Enums;
using OPS.Infrastructure.Data;

namespace OPS.Infrastructure.Services;

public class TeamService : ITeamService
{
    private readonly OpsDbContext _context;

    public TeamService(OpsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TeamListDto>> GetTeamsAsync()
    {
        var teams = await _context.Teams
            .Include(t => t.UserTeams)
                .ThenInclude(ut => ut.User)
                    .ThenInclude(u => u.Role)
            .Include(t => t.Incidents)
            .AsNoTracking()
            .ToListAsync();

        return teams.Select(t => new TeamListDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            MemberCount = t.UserTeams.Count,
            ActiveIncidentCount = t.Incidents.Count(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed),
            ManagerCount = t.UserTeams.Count(ut => ut.User.Role.Name == "Manager"),
            ResponderCount = t.UserTeams.Count(ut => ut.User.Role.Name == "Responder"),
            CreatedAt = t.CreatedAt
        });
    }

    public async Task<TeamDetailDto> GetTeamAsync(int id)
    {
        var team = await _context.Teams
            .Include(t => t.UserTeams)
                .ThenInclude(ut => ut.User)
                    .ThenInclude(u => u.Role)
            .Include(t => t.Incidents)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null)
            throw new Exception("Team not found");

        return new TeamDetailDto
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            MemberCount = team.UserTeams.Count,
            ActiveIncidentCount = team.Incidents.Count(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed),
            ManagerCount = team.UserTeams.Count(ut => ut.User.Role.Name == "Manager"),
            ResponderCount = team.UserTeams.Count(ut => ut.User.Role.Name == "Responder"),
            CreatedAt = team.CreatedAt,
            ResolvedIncidentCount = team.Incidents.Count(i => i.Status == IncidentStatus.Resolved || i.Status == IncidentStatus.Closed),
            SlaBreachedIncidentCount = team.Incidents.Count(i => i.ResponseSlaBreached || i.ResolutionSlaBreached),
            Members = team.UserTeams.Select(ut => new UserTeamDto
            {
                UserId = ut.User.Id,
                FullName = ut.User.FullName,
                Email = ut.User.Email,
                RoleName = ut.User.Role.Name
            }).ToList()
        };
    }

    public async Task<TeamDetailDto> CreateTeamAsync(CreateTeamRequest request, int currentUserId)
    {
        var team = new Team
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Teams.Add(team);

        var audit = new AuditLog
        {
            UserId = currentUserId,
            Action = "TeamCreated",
            EntityType = "Team",
            EntityId = "0",
            Details = $"Created team '{request.Name}'",
            CreatedAt = DateTime.UtcNow
        };
        _context.AuditLogs.Add(audit);

        await _context.SaveChangesAsync();
        
        audit.EntityId = team.Id.ToString();
        await _context.SaveChangesAsync();

        return await GetTeamAsync(team.Id);
    }

    public async Task<TeamDetailDto> UpdateTeamAsync(int id, UpdateTeamRequest request, int currentUserId)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
            throw new Exception("Team not found");

        team.Name = request.Name;
        team.Description = request.Description;

        var audit = new AuditLog
        {
            UserId = currentUserId,
            Action = "TeamUpdated",
            EntityType = "Team",
            EntityId = team.Id.ToString(),
            Details = $"Updated team '{request.Name}'",
            CreatedAt = DateTime.UtcNow
        };
        _context.AuditLogs.Add(audit);

        await _context.SaveChangesAsync();

        return await GetTeamAsync(team.Id);
    }

    public async Task DeleteTeamAsync(int id, int currentUserId)
    {
        var team = await _context.Teams
            .Include(t => t.Incidents)
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (team == null)
            throw new Exception("Team not found");

        if (team.Incidents.Any(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed))
        {
            throw new InvalidOperationException("Cannot delete a team that has active incidents. Reassign or resolve them first.");
        }

        _context.Teams.Remove(team);

        var audit = new AuditLog
        {
            UserId = currentUserId,
            Action = "TeamDeleted",
            EntityType = "Team",
            EntityId = team.Id.ToString(),
            Details = $"Deleted team '{team.Name}'",
            CreatedAt = DateTime.UtcNow
        };
        _context.AuditLogs.Add(audit);

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserTeamDto>> GetTeamMembersAsync(int teamId)
    {
        var team = await _context.Teams
            .Include(t => t.UserTeams)
                .ThenInclude(ut => ut.User)
                    .ThenInclude(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
            throw new Exception("Team not found");

        return team.UserTeams.Select(ut => new UserTeamDto
        {
            UserId = ut.User.Id,
            FullName = ut.User.FullName,
            Email = ut.User.Email,
            RoleName = ut.User.Role.Name
        });
    }

    public async Task AddMemberToTeamAsync(int teamId, int userId, int currentUserId)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team == null) throw new Exception("Team not found");

        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new Exception("User not found");

        var existing = await _context.UserTeams.FirstOrDefaultAsync(ut => ut.TeamId == teamId && ut.UserId == userId);
        if (existing != null) throw new InvalidOperationException("User is already a member of this team");

        var userTeam = new UserTeam
        {
            TeamId = teamId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        _context.UserTeams.Add(userTeam);

        var audit = new AuditLog
        {
            UserId = currentUserId,
            Action = "TeamMemberAdded",
            EntityType = "Team",
            EntityId = teamId.ToString(),
            Details = $"Added user {userId} to team {teamId}",
            CreatedAt = DateTime.UtcNow
        };
        _context.AuditLogs.Add(audit);

        await _context.SaveChangesAsync();
    }

    public async Task RemoveMemberFromTeamAsync(int teamId, int userId, int currentUserId)
    {
        var userTeam = await _context.UserTeams.FirstOrDefaultAsync(ut => ut.TeamId == teamId && ut.UserId == userId);
        if (userTeam == null) throw new Exception("Membership not found");

        _context.UserTeams.Remove(userTeam);

        var audit = new AuditLog
        {
            UserId = currentUserId,
            Action = "TeamMemberRemoved",
            EntityType = "Team",
            EntityId = teamId.ToString(),
            Details = $"Removed user {userId} from team {teamId}",
            CreatedAt = DateTime.UtcNow
        };
        _context.AuditLogs.Add(audit);

        await _context.SaveChangesAsync();
    }
}
