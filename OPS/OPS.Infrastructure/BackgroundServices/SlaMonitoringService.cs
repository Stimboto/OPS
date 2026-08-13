using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using OPS.Application.Interfaces;
using OPS.Domain.Entities;
using OPS.Domain.Enums;
using OPS.Infrastructure.Data;
using OPS.Application.DTOs.Events;

namespace OPS.Infrastructure.BackgroundServices;

public class SlaMonitoringService : BackgroundService
{
    private readonly ILogger<SlaMonitoringService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public SlaMonitoringService(ILogger<SlaMonitoringService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SLA Monitoring Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessSlasAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing SLAs.");
            }

            // Wait 60 seconds before next run
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }

        _logger.LogInformation("SLA Monitoring Service is stopping.");
    }

    private async Task ProcessSlasAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OpsDbContext>();
        var realtime = scope.ServiceProvider.GetRequiredService<IRealtimeNotificationService>();
        var slaPolicy = scope.ServiceProvider.GetRequiredService<ISlaPolicyProvider>();

        var now = DateTime.UtcNow;

        var activeIncidents = await context.Incidents
            .Where(i => i.Status != IncidentStatus.Closed && i.Status != IncidentStatus.Resolved)
            .ToListAsync(stoppingToken);

        foreach (var incident in activeIncidents)
        {
            try
            {
                var responseDuration = slaPolicy.GetResponseSla(incident.Severity);
                var resolutionDuration = slaPolicy.GetResolutionSla(incident.Severity);

                var responseWarningTime = incident.CreatedAt.AddTicks((long)(responseDuration.Ticks * 0.8));
                var resolutionWarningTime = incident.CreatedAt.AddTicks((long)(resolutionDuration.Ticks * 0.8));

                bool needsSave = false;
                var pendingWarnings = new List<SlaWarningEvent>();
                var pendingBreaches = new List<SlaBreachedEvent>();

                // 1. Response Warning
                if (incident.ResponseAt == null && incident.ResponseSlaWarningSentAt == null && now >= responseWarningTime && now < incident.ResponseDueAt)
                {
                    incident.ResponseSlaWarningSentAt = now;
                    needsSave = true;
                    await EnqueueWarning(context, incident, "Response", incident.ResponseDueAt, pendingWarnings);
                }

                // 2. Resolution Warning
                if (incident.ResolvedAt == null && incident.ResolutionSlaWarningSentAt == null && now >= resolutionWarningTime && now < incident.ResolutionDueAt)
                {
                    incident.ResolutionSlaWarningSentAt = now;
                    needsSave = true;
                    await EnqueueWarning(context, incident, "Resolution", incident.ResolutionDueAt, pendingWarnings);
                }

                // 3. Response Breach
                if (incident.ResponseAt == null && !incident.ResponseSlaBreached && now >= incident.ResponseDueAt)
                {
                    incident.ResponseSlaBreached = true;
                    needsSave = true;
                    await EnqueueBreach(context, incident, "Response", incident.ResponseDueAt, now, pendingBreaches);
                }

                // 4. Resolution Breach & Escalation
                if (incident.ResolvedAt == null && !incident.ResolutionSlaBreached && now >= incident.ResolutionDueAt)
                {
                    incident.ResolutionSlaBreached = true;
                    needsSave = true;
                    await EnqueueBreach(context, incident, "Resolution", incident.ResolutionDueAt, now, pendingBreaches);

                    if (incident.Status != IncidentStatus.Escalated)
                    {
                        var oldStatus = incident.Status;
                        incident.Status = IncidentStatus.Escalated;
                        incident.EscalatedAt = now;
                        
                        var history = new IncidentHistory
                        {
                            IncidentId = incident.Id,
                            OldStatus = oldStatus,
                            NewStatus = IncidentStatus.Escalated,
                            Remarks = "System Auto-Escalation: Resolution SLA Breached",
                            ChangedByUserId = null,
                            ChangedAt = now
                        };
                        context.IncidentHistories.Add(history);
                    }
                }

                if (needsSave)
                {
                    // Transactional DB Commit
                    await context.SaveChangesAsync(stoppingToken);

                    // Push SignalR Events
                    var targets = await GetTargetUsersForIncident(context, incident);
                    foreach (var w in pendingWarnings)
                    {
                        try { await realtime.SendSlaWarningAsync(w, targets); } catch { }
                    }
                    foreach (var b in pendingBreaches)
                    {
                        try { await realtime.SendSlaBreachedAsync(b, targets); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SLAs for incident {TrackingId}.", incident.TrackingId);
            }
        }
    }

    private async Task EnqueueWarning(OpsDbContext context, Incident incident, string slaType, DateTime dueAt, List<SlaWarningEvent> warnings)
    {
        var targets = await GetTargetUsersForIncident(context, incident);
        var msg = $"{slaType} SLA approaching for {incident.TrackingId}.";
        
        foreach (var target in targets)
            context.Notifications.Add(new Notification { UserId = target, Title = "SLA Warning", Message = msg, CreatedAt = DateTime.UtcNow });

        warnings.Add(new SlaWarningEvent { IncidentId = incident.Id, TrackingId = incident.TrackingId, SlaType = slaType, DueAt = dueAt, Severity = incident.Severity });
    }

    private async Task EnqueueBreach(OpsDbContext context, Incident incident, string slaType, DateTime dueAt, DateTime now, List<SlaBreachedEvent> breaches)
    {
        var targets = await GetTargetUsersForIncident(context, incident);
        var msg = $"{slaType} SLA breached for {incident.TrackingId}.";

        context.IncidentHistories.Add(new IncidentHistory
        {
            IncidentId = incident.Id,
            OldStatus = incident.Status,
            NewStatus = incident.Status,
            Remarks = $"System SLA Audit: {slaType} SLA breached.",
            ChangedByUserId = null,
            ChangedAt = now
        });

        foreach (var target in targets)
            context.Notifications.Add(new Notification { UserId = target, Title = "SLA Breached", Message = msg, CreatedAt = now });

        breaches.Add(new SlaBreachedEvent { IncidentId = incident.Id, TrackingId = incident.TrackingId, SlaType = slaType, DueAt = dueAt, BreachedAt = now, Severity = incident.Severity });
    }

    private async Task<List<int>> GetTargetUsersForIncident(OpsDbContext context, Incident incident)
    {
        var targets = new HashSet<int> { incident.ReportedByUserId };
        if (incident.AssignedToUserId.HasValue) targets.Add(incident.AssignedToUserId.Value);
        var managersAdmins = await context.Users.Include(u => u.Role).Where(u => u.Role.Name == "Manager" || u.Role.Name == "Admin").Select(u => u.Id).ToListAsync();
        foreach (var id in managersAdmins) targets.Add(id);
        return targets.ToList();
    }
}
