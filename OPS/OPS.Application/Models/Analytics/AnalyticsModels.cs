using System;
using System.Collections.Generic;
using OPS.Domain.Enums;

namespace OPS.Application.Models.Analytics;

public class OverviewStatsDto
{
    public int TotalIncidents { get; set; }
    public int ActiveIncidents { get; set; } // Not resolved/closed
    public int OpenIncidents { get; set; }
    public int AssignedIncidents { get; set; }
    public int Investigating { get; set; }
    public int Mitigating { get; set; }
    public int Resolved { get; set; }
    public int Closed { get; set; }
    public int Reopened { get; set; }
    public int Escalated { get; set; }
    
    public int Critical { get; set; }
    public int High { get; set; }
    public int Medium { get; set; }
    public int Low { get; set; }
    
    public int SlaAtRisk { get; set; }
    public int SlaBreached { get; set; }
    
    public int TotalTeams { get; set; }
    public int ActiveTeams { get; set; }
    public int TotalManagers { get; set; }
    public int TotalResponders { get; set; }
}

public class IncidentVolumeDto
{
    public string Period { get; set; } = string.Empty;
    public int IncidentCount { get; set; }
}

public class SeverityDistributionDto
{
    public int Critical { get; set; }
    public int High { get; set; }
    public int Medium { get; set; }
    public int Low { get; set; }
}

public class StatusDistributionDto
{
    public int Open { get; set; }
    public int Assigned { get; set; }
    public int Investigating { get; set; }
    public int Mitigating { get; set; }
    public int Resolved { get; set; }
    public int Closed { get; set; }
    public int Reopened { get; set; }
    public int Escalated { get; set; }
}

public class TeamPerformanceDto
{
    public string TeamName { get; set; } = string.Empty;
    public int TotalIncidents { get; set; }
    public int Open { get; set; }
    public int Investigating { get; set; }
    public int Resolved { get; set; }
    public int Closed { get; set; }
    public int Escalated { get; set; }
    public int SlaAtRisk { get; set; }
    public int SlaBreached { get; set; }
    public double ResolutionRate { get; set; }
    public double AverageResponseTimeMinutes { get; set; }
    public double AverageResolutionTimeMinutes { get; set; }
}

public class ResponderPerformanceDto
{
    public string ResponderName { get; set; } = string.Empty;
    public string Teams { get; set; } = string.Empty;
    public int AssignedIncidents { get; set; }
    public int ActiveIncidents { get; set; }
    public int ResolvedIncidents { get; set; }
    public int SlaBreaches { get; set; }
    public double AverageResponseTimeMinutes { get; set; }
    public double AverageResolutionTimeMinutes { get; set; }
}

public class SlaAnalyticsDto
{
    public double ResponseSlaCompliancePercentage { get; set; }
    public double ResolutionSlaCompliancePercentage { get; set; }
    public int TotalSlaBreaches { get; set; }
    public int ResponseSlaBreaches { get; set; }
    public int ResolutionSlaBreaches { get; set; }
    public int SlaAtRisk { get; set; }
    public int SlaMet { get; set; }
}

public class MttaMttrAnalyticsDto
{
    public double OverallMttaMinutes { get; set; }
    public double OverallMttrMinutes { get; set; }
}

public class EscalationAnalyticsDto
{
    public int TotalEscalated { get; set; }
    public double EscalationRate { get; set; }
    public Dictionary<string, int> EscalatedBySeverity { get; set; } = new();
    public Dictionary<string, int> EscalatedByTeam { get; set; } = new();
    public Dictionary<string, int> EscalatedByMonth { get; set; } = new();
}

public class ReopenedAnalyticsDto
{
    public int TotalReopened { get; set; }
    public double ReopenRate { get; set; }
    public Dictionary<string, int> ReopenedByTeam { get; set; } = new();
    public Dictionary<string, int> ReopenedBySeverity { get; set; } = new();
    public Dictionary<string, int> ReopenedByMonth { get; set; } = new();
}
