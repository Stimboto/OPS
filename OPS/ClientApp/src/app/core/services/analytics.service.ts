import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  OverviewStatsDto,
  IncidentVolumeDto,
  SeverityDistributionDto,
  StatusDistributionDto,
  TeamPerformanceDto,
  ResponderPerformanceDto,
  SlaAnalyticsDto,
  MttaMttrAnalyticsDto,
  EscalationAnalyticsDto,
  ReopenedAnalyticsDto
} from '../models/analytics.model';

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private apiUrl = `${environment.apiUrl}/analytics`;

  constructor(private http: HttpClient) {}

  private buildParams(from?: string, to?: string): HttpParams {
    let params = new HttpParams();
    if (from) params = params.set('from', from);
    if (to) params = params.set('to', to);
    return params;
  }

  // --- ADMIN ENDPOINTS ---

  getAdminOverview(from?: string, to?: string): Observable<OverviewStatsDto> {
    return this.http.get<OverviewStatsDto>(`${this.apiUrl}/admin/overview`, { params: this.buildParams(from, to) });
  }

  getAdminIncidentVolume(period: string = 'daily', from?: string, to?: string): Observable<IncidentVolumeDto[]> {
    let params = this.buildParams(from, to);
    params = params.set('period', period);
    return this.http.get<IncidentVolumeDto[]>(`${this.apiUrl}/admin/incident-volume`, { params });
  }

  getAdminSeverity(from?: string, to?: string): Observable<SeverityDistributionDto> {
    return this.http.get<SeverityDistributionDto>(`${this.apiUrl}/admin/severity`, { params: this.buildParams(from, to) });
  }

  getAdminStatus(from?: string, to?: string): Observable<StatusDistributionDto> {
    return this.http.get<StatusDistributionDto>(`${this.apiUrl}/admin/status`, { params: this.buildParams(from, to) });
  }

  getAdminTeams(from?: string, to?: string): Observable<TeamPerformanceDto[]> {
    return this.http.get<TeamPerformanceDto[]>(`${this.apiUrl}/admin/teams`, { params: this.buildParams(from, to) });
  }

  getAdminResponders(from?: string, to?: string): Observable<ResponderPerformanceDto[]> {
    return this.http.get<ResponderPerformanceDto[]>(`${this.apiUrl}/admin/responders`, { params: this.buildParams(from, to) });
  }

  getAdminSla(from?: string, to?: string): Observable<SlaAnalyticsDto> {
    return this.http.get<SlaAnalyticsDto>(`${this.apiUrl}/admin/sla`, { params: this.buildParams(from, to) });
  }

  getAdminMttaMttr(from?: string, to?: string): Observable<MttaMttrAnalyticsDto> {
    return this.http.get<MttaMttrAnalyticsDto>(`${this.apiUrl}/admin/mtta-mttr`, { params: this.buildParams(from, to) });
  }

  getAdminEscalation(from?: string, to?: string): Observable<EscalationAnalyticsDto> {
    return this.http.get<EscalationAnalyticsDto>(`${this.apiUrl}/admin/escalation`, { params: this.buildParams(from, to) });
  }

  getAdminReopened(from?: string, to?: string): Observable<ReopenedAnalyticsDto> {
    return this.http.get<ReopenedAnalyticsDto>(`${this.apiUrl}/admin/reopened`, { params: this.buildParams(from, to) });
  }

  // --- MANAGER ENDPOINTS ---

  getManagerOverview(from?: string, to?: string): Observable<OverviewStatsDto> {
    return this.http.get<OverviewStatsDto>(`${this.apiUrl}/manager/overview`, { params: this.buildParams(from, to) });
  }

  getManagerTeams(from?: string, to?: string): Observable<TeamPerformanceDto[]> {
    return this.http.get<TeamPerformanceDto[]>(`${this.apiUrl}/manager/teams`, { params: this.buildParams(from, to) });
  }

  // --- RESPONDER ENDPOINTS ---

  getResponderOverview(from?: string, to?: string): Observable<OverviewStatsDto> {
    return this.http.get<OverviewStatsDto>(`${this.apiUrl}/responder/overview`, { params: this.buildParams(from, to) });
  }
}
