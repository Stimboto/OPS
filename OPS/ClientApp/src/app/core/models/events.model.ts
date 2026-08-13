export interface NotificationDto {
  id: number;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export interface IncidentCreatedEvent {
  incidentId: number;
  trackingId: string;
  title: string;
  severity: number;
  priority: number;
  status: number;
  reporterName: string;
  createdAt: string;
}

export interface IncidentAssignedEvent {
  incidentId: number;
  trackingId: string;
  title: string;
  assignedResponderId: number;
  assignedResponderName: string;
  assignedBy: string;
  assignedAt: string;
}

export interface IncidentStatusChangedEvent {
  incidentId: number;
  trackingId: string;
  title: string;
  oldStatus: number;
  newStatus: number;
  changedBy: string;
  changedAt: string;
  remarks: string;
}

export interface SlaWarningEvent {
  incidentId: number;
  trackingId: string;
  slaType: string;
  dueAt: string;
  severity: number;
}

export interface SlaBreachedEvent {
  incidentId: number;
  trackingId: string;
  slaType: string;
  dueAt: string;
  breachedAt: string;
  severity: number;
}
