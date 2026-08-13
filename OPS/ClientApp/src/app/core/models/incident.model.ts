export enum IncidentSeverity {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3
}

export enum IncidentPriority {
  P4 = 0,
  P3 = 1,
  P2 = 2,
  P1 = 3
}

export enum IncidentStatus {
  Open = 0,
  Assigned = 1,
  Investigating = 2,
  Mitigating = 3,
  Resolved = 4,
  Closed = 5,
  Reopened = 6,
  Escalated = 7
}

export interface IncidentListDto {
  id: number;
  trackingId: string;
  title: string;
  severity: IncidentSeverity;
  priority: IncidentPriority;
  status: IncidentStatus;
  teamName: string | null;
  reporterName: string;
  assignedResponderName: string | null;
  createdAt: string;
  updatedAt: string | null;
  resolutionDueAt: string | null;
  responseSlaBreached: boolean;
  resolutionSlaBreached: boolean;
}

export interface IncidentHistoryDto {
  oldStatus: IncidentStatus;
  newStatus: IncidentStatus;
  remarks: string;
  changedByUserName: string;
  changedAt: string;
}

export interface IncidentDetailDto {
  id: number;
  trackingId: string;
  title: string;
  description: string;
  severity: IncidentSeverity;
  priority: IncidentPriority;
  status: IncidentStatus;
  teamName: string | null;
  reportedByUserId: number;
  reporterName: string;
  assignedToUserId: number | null;
  assignedResponderName: string | null;
  createdAt: string;
  updatedAt: string | null;
  resolvedAt: string | null;
  
  responseDueAt: string;
  resolutionDueAt: string;
  responseAt: string | null;
  responseSlaBreached: boolean;
  resolutionSlaBreached: boolean;
  escalatedAt: string | null;

  history: IncidentHistoryDto[];
}

export interface CreateIncidentRequest {
  title: string;
  description: string;
  severity: IncidentSeverity;
  priority: IncidentPriority;
  teamId: number | null;
}

export interface AssignIncidentRequest {
  responderId: number;
}

export interface UpdateIncidentStatusRequest {
  status: IncidentStatus;
  remarks: string;
}
