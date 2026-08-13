using OPS.Application.DTOs.Events;
using OPS.Application.DTOs.Notifications;

namespace OPS.Application.Interfaces;

public interface IRealtimeNotificationService
{
    Task SendIncidentCreatedAsync(IncidentCreatedEvent @event, IEnumerable<int> targetUserIds);
    Task SendIncidentAssignedAsync(IncidentAssignedEvent @event, IEnumerable<int> targetUserIds);
    Task SendIncidentStatusChangedAsync(IncidentStatusChangedEvent @event, IEnumerable<int> targetUserIds);
    Task SendNotificationCreatedAsync(NotificationDto notification, int targetUserId);
    Task SendSlaWarningAsync(SlaWarningEvent @event, IEnumerable<int> targetUserIds);
    Task SendSlaBreachedAsync(SlaBreachedEvent @event, IEnumerable<int> targetUserIds);
    Task SendCommentCreatedAsync(CommentCreatedEvent @event, IEnumerable<int> targetUserIds);
    Task SendAttachmentUploadedAsync(AttachmentUploadedEvent @event, IEnumerable<int> targetUserIds);
}
