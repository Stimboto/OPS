using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OPS.Application.Interfaces;
using OPS.Application.Models.Analytics;
using OPS.Domain.Enums;
using OPS.Infrastructure.Data;

namespace OPS.Infrastructure.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly OpsDbContext _context;

    public AnalyticsService(OpsDbContext context)
    {
        _context = context;
    }

    private IQueryable<Domain.Entities.Incident> GetBaseQuery(DateTime? from, DateTime? to)
    {
        var query = _context.Incidents.AsNoTracking();

        if (from.HasValue)
        {
            query = query.Where(i => i.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(i => i.CreatedAt <= to.Value);
        }

        return query;
    }

    public async Task<OverviewStatsDto> GetAdminOverviewAsync(DateTime? from, DateTime? to)
    {
        var query = GetBaseQuery(from, to);

        var overview = await query
            .GroupBy(x => 1)
            .Select(g => new OverviewStatsDto
            {
                TotalIncidents = g.Count(),
                ActiveIncidents = g.Count(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed),
                OpenIncidents = g.Count(i => i.Status == IncidentStatus.Open),
                AssignedIncidents = g.Count(i => i.Status == IncidentStatus.Assigned),
                Investigating = g.Count(i => i.Status == IncidentStatus.Investigating),
                Mitigating = g.Count(i => i.Status == IncidentStatus.Mitigating),
                Resolved = g.Count(i => i.Status == IncidentStatus.Resolved),
                Closed = g.Count(i => i.Status == IncidentStatus.Closed),
                Reopened = g.Count(i => i.Status == IncidentStatus.Reopened),
                Escalated = g.Count(i => i.Status == IncidentStatus.Escalated),
                
                Critical = g.Count(i => i.Severity == IncidentSeverity.Critical),
                High = g.Count(i => i.Severity == IncidentSeverity.High),
                Medium = g.Count(i => i.Severity == IncidentSeverity.Medium),
                Low = g.Count(i => i.Severity == IncidentSeverity.Low),
                
                SlaAtRisk = g.Count(i => i.ResponseSlaWarningSentAt != null || i.ResolutionSlaWarningSentAt != null),
                SlaBreached = g.Count(i => i.ResponseSlaBreached || i.ResolutionSlaBreached)
            })
            .FirstOrDefaultAsync() ?? new OverviewStatsDto();

        overview.TotalTeams = await _context.Teams.CountAsync();
        overview.ActiveTeams = await _context.Teams.CountAsync(t => t.IsActive);
        
        overview.TotalManagers = await _context.Users.CountAsync(u => u.Role.Name == "Manager");
        overview.TotalResponders = await _context.Users.CountAsync(u => u.Role.Name == "Responder");

        return overview;
    }

    public async Task<IEnumerable<IncidentVolumeDto>> GetAdminIncidentVolumeAsync(string period, DateTime? from, DateTime? to)
    {
        var query = GetBaseQuery(from, to);
        
        // Using SQL date grouping based on period string. Since EF Core translation for custom date format varies by provider,
        // we'll group by Date components.
        
        // For SQL Server:
        if (period?.ToLower() == "monthly")
        {
            return await query
                .GroupBy(i => new { i.CreatedAt.Year, i.CreatedAt.Month })
                .Select(g => new IncidentVolumeDto
                {
                    Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                    IncidentCount = g.Count()
                })
                .OrderBy(x => x.Period)
                .ToListAsync();
        }
        else // default to daily
        {
            return await query
                .GroupBy(i => i.CreatedAt.Date)
                .Select(g => new IncidentVolumeDto
                {
                    Period = g.Key.ToString("yyyy-MM-dd"),
                    IncidentCount = g.Count()
                })
                .OrderBy(x => x.Period)
                .ToListAsync();
        }
    }

    public async Task<SeverityDistributionDto> GetAdminSeverityDistributionAsync(DateTime? from, DateTime? to)
    {
        var query = GetBaseQuery(from, to);
        return await query
            .GroupBy(x => 1)
            .Select(g => new SeverityDistributionDto
            {
                Critical = g.Count(i => i.Severity == IncidentSeverity.Critical),
                High = g.Count(i => i.Severity == IncidentSeverity.High),
                Medium = g.Count(i => i.Severity == IncidentSeverity.Medium),
                Low = g.Count(i => i.Severity == IncidentSeverity.Low)
            })
            .FirstOrDefaultAsync() ?? new SeverityDistributionDto();
    }

    public async Task<StatusDistributionDto> GetAdminStatusDistributionAsync(DateTime? from, DateTime? to)
    {
        var query = GetBaseQuery(from, to);
        return await query
            .GroupBy(x => 1)
            .Select(g => new StatusDistributionDto
            {
                Open = g.Count(i => i.Status == IncidentStatus.Open),
                Assigned = g.Count(i => i.Status == IncidentStatus.Assigned),
                Investigating = g.Count(i => i.Status == IncidentStatus.Investigating),
                Mitigating = g.Count(i => i.Status == IncidentStatus.Mitigating),
                Resolved = g.Count(i => i.Status == IncidentStatus.Resolved),
                Closed = g.Count(i => i.Status == IncidentStatus.Closed),
                Reopened = g.Count(i => i.Status == IncidentStatus.Reopened),
                Escalated = g.Count(i => i.Status == IncidentStatus.Escalated)
            })
            .FirstOrDefaultAsync() ?? new StatusDistributionDto();
    }

    public async Task<IEnumerable<TeamPerformanceDto>> GetAdminTeamPerformanceAsync(DateTime? from, DateTime? to)
    {
        var query = GetBaseQuery(from, to);
        
        return await query
            .Where(i => i.TeamId != null)
            .GroupBy(i => i.Team)
            .Select(g => new TeamPerformanceDto
            {
                TeamName = g.Key!.Name,
                TotalIncidents = g.Count(),
                Open = g.Count(i => i.Status == IncidentStatus.Open),
                Investigating = g.Count(i => i.Status == IncidentStatus.Investigating),
                Resolved = g.Count(i => i.Status == IncidentStatus.Resolved),
                Closed = g.Count(i => i.Status == IncidentStatus.Closed),
                Escalated = g.Count(i => i.Status == IncidentStatus.Escalated),
                SlaAtRisk = g.Count(i => i.ResponseSlaWarningSentAt != null || i.ResolutionSlaWarningSentAt != null),
                SlaBreached = g.Count(i => i.ResponseSlaBreached || i.ResolutionSlaBreached),
                ResolutionRate = g.Count() > 0 ? Math.Round((double)g.Count(i => i.Status == IncidentStatus.Resolved || i.Status == IncidentStatus.Closed) / g.Count() * 100, 2) : 0,
                AverageResponseTimeMinutes = g.Where(i => i.ResponseAt != null).Any() 
                    ? Math.Round(g.Where(i => i.ResponseAt != null).Average(i => EF.Functions.DateDiffMinute(i.CreatedAt, i.ResponseAt!.Value)), 2) 
                    : 0,
                AverageResolutionTimeMinutes = g.Where(i => i.ResolvedAt != null).Any() 
                    ? Math.Round(g.Where(i => i.ResolvedAt != null).Average(i => EF.Functions.DateDiffMinute(i.CreatedAt, i.ResolvedAt!.Value)), 2) 
                    : 0
            })
            .OrderByDescending(t => t.TotalIncidents)
            .ToListAsync();
    }

    public async Task<IEnumerable<ResponderPerformanceDto>> GetAdminResponderPerformanceAsync(DateTime? from, DateTime? to)
    {
        var query = GetBaseQuery(from, to);
        
        return await query
            .Where(i => i.AssignedToUserId != null)
            .GroupBy(i => i.AssignedToUser)
            .Select(g => new ResponderPerformanceDto
            {
                ResponderName = g.Key!.FullName,
                Teams = "Multiple", // For performance, avoid joining team list here if possible, or leave static for now
                AssignedIncidents = g.Count(),
                ActiveIncidents = g.Count(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed),
                ResolvedIncidents = g.Count(i => i.Status == IncidentStatus.Resolved || i.Status == IncidentStatus.Closed),
                SlaBreaches = g.Count(i => i.ResponseSlaBreached || i.ResolutionSlaBreached),
                AverageResponseTimeMinutes = g.Where(i => i.ResponseAt != null).Any() 
                    ? Math.Round(g.Where(i => i.ResponseAt != null).Average(i => EF.Functions.DateDiffMinute(i.CreatedAt, i.ResponseAt!.Value)), 2) 
                    : 0,
                AverageResolutionTimeMinutes = g.Where(i => i.ResolvedAt != null).Any() 
                    ? Math.Round(g.Where(i => i.ResolvedAt != null).Average(i => EF.Functions.DateDiffMinute(i.CreatedAt, i.ResolvedAt!.Value)), 2) 
                    : 0
            })
            .OrderByDescending(r => r.AssignedIncidents)
            .ToListAsync();
    }

    public async Task<SlaAnalyticsDto> GetAdminSlaAnalyticsAsync(DateTime? from, DateTime? to)
    {
        var query = GetBaseQuery(from, to);
        
        var stats = await query
            .GroupBy(x => 1)
            .Select(g => new
            {
                Total = g.Count(),
                ResponseBreaches = g.Count(i => i.ResponseSlaBreached),
                ResolutionBreaches = g.Count(i => i.ResolutionSlaBreached),
                AtRisk = g.Count(i => i.ResponseSlaWarningSentAt != null || i.ResolutionSlaWarningSentAt != null),
                Met = g.Count(i => !i.ResponseSlaBreached && !i.ResolutionSlaBreached && (i.Status == IncidentStatus.Resolved || i.Status == IncidentStatus.Closed))
            })
            .FirstOrDefaultAsync();

        if (stats == null || stats.Total == 0) return new SlaAnalyticsDto();

        return new SlaAnalyticsDto
        {
            TotalSlaBreaches = stats.ResponseBreaches + stats.ResolutionBreaches,
            ResponseSlaBreaches = stats.ResponseBreaches,
            ResolutionSlaBreaches = stats.ResolutionBreaches,
            SlaAtRisk = stats.AtRisk,
            SlaMet = stats.Met,
            ResponseSlaCompliancePercentage = Math.Round(100.0 - ((double)stats.ResponseBreaches / stats.Total * 100), 2),
            ResolutionSlaCompliancePercentage = Math.Round(100.0 - ((double)stats.ResolutionBreaches / stats.Total * 100), 2)
        };
    }

    public async Task<MttaMttrAnalyticsDto> GetAdminMttaMttrAsync(DateTime? from, DateTime? to)
    {
        var query = GetBaseQuery(from, to);
        
        var mtta = await query
            .Where(i => i.ResponseAt != null)
            .AverageAsync(i => (double?)EF.Functions.DateDiffMinute(i.CreatedAt, i.ResponseAt!.Value)) ?? 0;

        var mttr = await query
            .Where(i => i.ResolvedAt != null)
            .AverageAsync(i => (double?)EF.Functions.DateDiffMinute(i.CreatedAt, i.ResolvedAt!.Value)) ?? 0;

        return new MttaMttrAnalyticsDto
        {
            OverallMttaMinutes = Math.Round(mtta, 2),
            OverallMttrMinutes = Math.Round(mttr, 2)
        };
    }

    public async Task<EscalationAnalyticsDto> GetAdminEscalationAnalyticsAsync(DateTime? from, DateTime? to)
    {
        var query = GetBaseQuery(from, to);
        var total = await query.CountAsync();
        var escalated = await query.CountAsync(i => i.Status == IncidentStatus.Escalated || i.History.Any(h => h.NewStatus == IncidentStatus.Escalated));
        
        return new EscalationAnalyticsDto
        {
            TotalEscalated = escalated,
            EscalationRate = total > 0 ? Math.Round((double)escalated / total * 100, 2) : 0
        };
    }

    public async Task<ReopenedAnalyticsDto> GetAdminReopenedAnalyticsAsync(DateTime? from, DateTime? to)
    {
        var query = GetBaseQuery(from, to);
        var total = await query.CountAsync();
        var reopened = await query.CountAsync(i => i.Status == IncidentStatus.Reopened || i.History.Any(h => h.NewStatus == IncidentStatus.Reopened));
        
        return new ReopenedAnalyticsDto
        {
            TotalReopened = reopened,
            ReopenRate = total > 0 ? Math.Round((double)reopened / total * 100, 2) : 0
        };
    }

    public async Task<OverviewStatsDto> GetManagerOverviewAsync(int managerUserId, DateTime? from, DateTime? to)
    {
        var teamIds = await _context.UserTeams
            .Where(ut => ut.UserId == managerUserId)
            .Select(ut => ut.TeamId)
            .ToListAsync();

        var query = GetBaseQuery(from, to).Where(i => i.TeamId.HasValue && teamIds.Contains(i.TeamId.Value));

        var overview = await query
            .GroupBy(x => 1)
            .Select(g => new OverviewStatsDto
            {
                TotalIncidents = g.Count(),
                ActiveIncidents = g.Count(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed),
                OpenIncidents = g.Count(i => i.Status == IncidentStatus.Open),
                Investigating = g.Count(i => i.Status == IncidentStatus.Investigating),
                Mitigating = g.Count(i => i.Status == IncidentStatus.Mitigating),
                Resolved = g.Count(i => i.Status == IncidentStatus.Resolved),
                SlaAtRisk = g.Count(i => i.ResponseSlaWarningSentAt != null || i.ResolutionSlaWarningSentAt != null),
                SlaBreached = g.Count(i => i.ResponseSlaBreached || i.ResolutionSlaBreached)
            })
            .FirstOrDefaultAsync() ?? new OverviewStatsDto();
            
        return overview;
    }

    public async Task<IEnumerable<TeamPerformanceDto>> GetManagerTeamPerformanceAsync(int managerUserId, DateTime? from, DateTime? to)
    {
        var teamIds = await _context.UserTeams
            .Where(ut => ut.UserId == managerUserId)
            .Select(ut => ut.TeamId)
            .ToListAsync();

        var query = GetBaseQuery(from, to).Where(i => i.TeamId.HasValue && teamIds.Contains(i.TeamId.Value));
        
        return await query
            .GroupBy(i => i.Team)
            .Select(g => new TeamPerformanceDto
            {
                TeamName = g.Key!.Name,
                TotalIncidents = g.Count(),
                Open = g.Count(i => i.Status == IncidentStatus.Open),
                Investigating = g.Count(i => i.Status == IncidentStatus.Investigating),
                Resolved = g.Count(i => i.Status == IncidentStatus.Resolved),
                Closed = g.Count(i => i.Status == IncidentStatus.Closed),
                Escalated = g.Count(i => i.Status == IncidentStatus.Escalated),
                SlaAtRisk = g.Count(i => i.ResponseSlaWarningSentAt != null || i.ResolutionSlaWarningSentAt != null),
                SlaBreached = g.Count(i => i.ResponseSlaBreached || i.ResolutionSlaBreached),
                ResolutionRate = g.Count() > 0 ? Math.Round((double)g.Count(i => i.Status == IncidentStatus.Resolved || i.Status == IncidentStatus.Closed) / g.Count() * 100, 2) : 0,
                AverageResponseTimeMinutes = g.Where(i => i.ResponseAt != null).Any() 
                    ? Math.Round(g.Where(i => i.ResponseAt != null).Average(i => EF.Functions.DateDiffMinute(i.CreatedAt, i.ResponseAt!.Value)), 2) 
                    : 0,
                AverageResolutionTimeMinutes = g.Where(i => i.ResolvedAt != null).Any() 
                    ? Math.Round(g.Where(i => i.ResolvedAt != null).Average(i => EF.Functions.DateDiffMinute(i.CreatedAt, i.ResolvedAt!.Value)), 2) 
                    : 0
            })
            .OrderByDescending(t => t.TotalIncidents)
            .ToListAsync();
    }

    public async Task<OverviewStatsDto> GetResponderOverviewAsync(int responderUserId, DateTime? from, DateTime? to)
    {
        var query = GetBaseQuery(from, to).Where(i => i.AssignedToUserId == responderUserId);

        var overview = await query
            .GroupBy(x => 1)
            .Select(g => new OverviewStatsDto
            {
                AssignedIncidents = g.Count(),
                ActiveIncidents = g.Count(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Closed),
                Investigating = g.Count(i => i.Status == IncidentStatus.Investigating),
                Mitigating = g.Count(i => i.Status == IncidentStatus.Mitigating),
                Resolved = g.Count(i => i.Status == IncidentStatus.Resolved),
                SlaAtRisk = g.Count(i => i.ResponseSlaWarningSentAt != null || i.ResolutionSlaWarningSentAt != null),
                SlaBreached = g.Count(i => i.ResponseSlaBreached || i.ResolutionSlaBreached),
                
                Critical = g.Count(i => i.Severity == IncidentSeverity.Critical),
                High = g.Count(i => i.Severity == IncidentSeverity.High),
                Medium = g.Count(i => i.Severity == IncidentSeverity.Medium),
                Low = g.Count(i => i.Severity == IncidentSeverity.Low)
            })
            .FirstOrDefaultAsync() ?? new OverviewStatsDto();
            
        return overview;
    }
}
