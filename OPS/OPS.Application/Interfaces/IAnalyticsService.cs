using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OPS.Application.Models.Analytics;

namespace OPS.Application.Interfaces;

public interface IAnalyticsService
{
    // Admin (Global)
    Task<OverviewStatsDto> GetAdminOverviewAsync(DateTime? from, DateTime? to);
    Task<IEnumerable<IncidentVolumeDto>> GetAdminIncidentVolumeAsync(string period, DateTime? from, DateTime? to);
    Task<SeverityDistributionDto> GetAdminSeverityDistributionAsync(DateTime? from, DateTime? to);
    Task<StatusDistributionDto> GetAdminStatusDistributionAsync(DateTime? from, DateTime? to);
    Task<IEnumerable<TeamPerformanceDto>> GetAdminTeamPerformanceAsync(DateTime? from, DateTime? to);
    Task<IEnumerable<ResponderPerformanceDto>> GetAdminResponderPerformanceAsync(DateTime? from, DateTime? to);
    Task<SlaAnalyticsDto> GetAdminSlaAnalyticsAsync(DateTime? from, DateTime? to);
    Task<MttaMttrAnalyticsDto> GetAdminMttaMttrAsync(DateTime? from, DateTime? to);
    Task<EscalationAnalyticsDto> GetAdminEscalationAnalyticsAsync(DateTime? from, DateTime? to);
    Task<ReopenedAnalyticsDto> GetAdminReopenedAnalyticsAsync(DateTime? from, DateTime? to);

    // Manager (Scoped to Manager's Teams)
    Task<OverviewStatsDto> GetManagerOverviewAsync(int managerUserId, DateTime? from, DateTime? to);
    Task<IEnumerable<TeamPerformanceDto>> GetManagerTeamPerformanceAsync(int managerUserId, DateTime? from, DateTime? to);
    
    // Responder (Scoped to Assigned Incidents)
    Task<OverviewStatsDto> GetResponderOverviewAsync(int responderUserId, DateTime? from, DateTime? to);
}
