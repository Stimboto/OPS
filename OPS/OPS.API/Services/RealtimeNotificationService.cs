using Microsoft.AspNetCore.SignalR;
using OPS.Application.DTOs.Events;
using OPS.Application.DTOs.Notifications;
using OPS.Application.Interfaces;
using OPS.API.Hubs;

namespace OPS.API.Services;

public class RealtimeNotificationService : IRealtimeNotificationService
{
    private readonly IHubContext<OperationsHub> _hubContext;

    public RealtimeNotificationService(IHubContext<OperationsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendIncidentCreatedAsync(IncidentCreatedEvent @event, IEnumerable<int> targetUserIds)
    {
        var userIds = targetUserIds.Select(id => id.ToString()).ToList();
        await _hubContext.Clients.Users(userIds).SendAsync("IncidentCreated", @event);
    }

    public async Task SendIncidentAssignedAsync(IncidentAssignedEvent @event, IEnumerable<int> targetUserIds)
    {
        var userIds = targetUserIds.Select(id => id.ToString()).ToList();
        await _hubContext.Clients.Users(userIds).SendAsync("IncidentAssigned", @event);
    }

    public async Task SendIncidentStatusChangedAsync(IncidentStatusChangedEvent @event, IEnumerable<int> targetUserIds)
    {
        var userIds = targetUserIds.Select(id => id.ToString()).ToList();
        await _hubContext.Clients.Users(userIds).SendAsync("IncidentStatusChanged", @event);
    }

    public async Task SendNotificationCreatedAsync(NotificationDto notification, int targetUserId)
    {
        await _hubContext.Clients.User(targetUserId.ToString()).SendAsync("NotificationCreated", notification);
    }

    public async Task SendSlaWarningAsync(SlaWarningEvent @event, IEnumerable<int> targetUserIds)
    {
        var users = targetUserIds.Select(u => u.ToString()).ToList();
        await _hubContext.Clients.Users(users).SendAsync("SlaWarning", @event);
    }

    public async Task SendSlaBreachedAsync(SlaBreachedEvent @event, IEnumerable<int> targetUserIds)
    {
        var users = targetUserIds.Select(u => u.ToString()).ToList();
        await _hubContext.Clients.Users(users).SendAsync("SlaBreached", @event);
    }

    public async Task SendCommentCreatedAsync(CommentCreatedEvent @event, IEnumerable<int> targetUserIds)
    {
        var users = targetUserIds.Select(u => u.ToString()).ToList();
        await _hubContext.Clients.Users(users).SendAsync("CommentCreated", @event);
    }

    public async Task SendAttachmentUploadedAsync(AttachmentUploadedEvent @event, IEnumerable<int> targetUserIds)
    {
        var users = targetUserIds.Select(u => u.ToString()).ToList();
        await _hubContext.Clients.Users(users).SendAsync("AttachmentUploaded", @event);
    }
}
