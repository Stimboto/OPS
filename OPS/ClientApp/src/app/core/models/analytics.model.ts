export interface OverviewStatsDto {
  totalIncidents: number;
  activeIncidents: number;
  openIncidents: number;
  assignedIncidents: number;
  investigating: number;
  mitigating: number;
  resolved: number;
  closed: number;
  reopened: number;
  escalated: number;
  critical: number;
  high: number;
  medium: number;
  low: number;
  slaAtRisk: number;
  slaBreached: number;
  totalTeams: number;
  activeTeams: number;
  totalManagers: number;
  totalResponders: number;
}

export interface IncidentVolumeDto {
  period: string;
  incidentCount: number;
}

export interface SeverityDistributionDto {
  critical: number;
  high: number;
  medium: number;
  low: number;
}

export interface StatusDistributionDto {
  open: number;
  assigned: number;
  investigating: number;
  mitigating: number;
  resolved: number;
  closed: number;
  reopened: number;
  escalated: number;
}

export interface TeamPerformanceDto {
  teamName: string;
  totalIncidents: number;
  open: number;
  investigating: number;
  resolved: number;
  closed: number;
  escalated: number;
  slaAtRisk: number;
  slaBreached: number;
  resolutionRate: number;
  averageResponseTimeMinutes: number;
  averageResolutionTimeMinutes: number;
}

export interface ResponderPerformanceDto {
  responderName: string;
  teams: string;
  assignedIncidents: number;
  activeIncidents: number;
  resolvedIncidents: number;
  slaBreaches: number;
  averageResponseTimeMinutes: number;
  averageResolutionTimeMinutes: number;
}

export interface SlaAnalyticsDto {
  responseSlaCompliancePercentage: number;
  resolutionSlaCompliancePercentage: number;
  totalSlaBreaches: number;
  responseSlaBreaches: number;
  resolutionSlaBreaches: number;
  slaAtRisk: number;
  slaMet: number;
}

export interface MttaMttrAnalyticsDto {
  overallMttaMinutes: number;
  overallMttrMinutes: number;
}

export interface EscalationAnalyticsDto {
  totalEscalated: number;
  escalationRate: number;
  escalatedBySeverity: { [key: string]: number };
  escalatedByTeam: { [key: string]: number };
  escalatedByMonth: { [key: string]: number };
}

export interface ReopenedAnalyticsDto {
  totalReopened: number;
  reopenRate: number;
  reopenedByTeam: { [key: string]: number };
  reopenedBySeverity: { [key: string]: number };
  reopenedByMonth: { [key: string]: number };
}
