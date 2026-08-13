import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterModule } from '@angular/router';
import { AnalyticsFilterComponent, AnalyticsFilter } from '../../../shared/components/analytics-filter/analytics-filter.component';
import { AnalyticsService } from '../../../core/services/analytics.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { BaseChartDirective } from 'ng2-charts';
import { Subscription, Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';

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
} from '../../../core/models/analytics.model';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule, 
    MatCardModule, 
    MatTableModule, 
    MatButtonModule, 
    MatIconModule,
    MatProgressSpinnerModule,
    RouterModule,
    AnalyticsFilterComponent,
    BaseChartDirective
  ],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit, OnDestroy {
  isLoading = true;
  currentFilter: AnalyticsFilter = {};

  overview!: OverviewStatsDto;
  slaStats!: SlaAnalyticsDto;
  mttaMttr!: MttaMttrAnalyticsDto;
  escalationStats!: EscalationAnalyticsDto;
  
  teamPerformance: TeamPerformanceDto[] = [];
  responderPerformance: ResponderPerformanceDto[] = [];
  
  teamColumns: string[] = ['teamName', 'totalIncidents', 'active', 'resolved', 'slaAtRisk', 'slaBreached', 'mtta', 'mttr', 'resolutionRate'];
  responderColumns: string[] = ['responderName', 'assigned', 'active', 'resolved', 'slaBreached', 'mtta', 'mttr'];

  // Charts
  volumeChartData: ChartData<'line'> = { labels: [], datasets: [] };
  volumeChartOptions: ChartConfiguration['options'] = { responsive: true };

  severityChartData: ChartData<'doughnut'> = { labels: [], datasets: [] };
  severityChartOptions: ChartConfiguration['options'] = { responsive: true };

  statusChartData: ChartData<'bar'> = { labels: [], datasets: [] };
  statusChartOptions: ChartConfiguration['options'] = { responsive: true };

  private subscriptions = new Subscription();
  private refreshSubject = new Subject<void>();

  constructor(
    private analyticsService: AnalyticsService,
    private signalRService: SignalRService
  ) {}

  ngOnInit(): void {
    this.refreshSubject.pipe(debounceTime(2000)).subscribe(() => {
      this.loadAnalytics();
    });

    // Default load
    this.loadAnalytics();

    // SignalR listeners for auto-refresh
    this.subscriptions.add(this.signalRService.incidentCreated$.subscribe(() => this.triggerRefresh()));
    this.subscriptions.add(this.signalRService.incidentAssigned$.subscribe(() => this.triggerRefresh()));
    this.subscriptions.add(this.signalRService.incidentStatusChanged$.subscribe(() => this.triggerRefresh()));
    this.subscriptions.add(this.signalRService.slaWarning$.subscribe(() => this.triggerRefresh()));
    this.subscriptions.add(this.signalRService.slaBreached$.subscribe(() => this.triggerRefresh()));
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  onFilterChanged(filter: AnalyticsFilter): void {
    this.currentFilter = filter;
    this.triggerRefresh();
  }

  private triggerRefresh(): void {
    this.isLoading = true;
    this.refreshSubject.next();
  }

  private loadAnalytics(): void {
    const { from, to } = this.currentFilter;
    
    // Overview
    this.analyticsService.getAdminOverview(from, to).subscribe(data => this.overview = data);
    this.analyticsService.getAdminSla(from, to).subscribe(data => this.slaStats = data);
    this.analyticsService.getAdminMttaMttr(from, to).subscribe(data => this.mttaMttr = data);
    this.analyticsService.getAdminEscalation(from, to).subscribe(data => this.escalationStats = data);
    
    // Tables
    this.analyticsService.getAdminTeams(from, to).subscribe(data => {
      this.teamPerformance = data;
    });
    this.analyticsService.getAdminResponders(from, to).subscribe(data => {
      this.responderPerformance = data;
    });

    // Charts
    this.analyticsService.getAdminIncidentVolume('daily', from, to).subscribe(data => {
      this.volumeChartData = {
        labels: data.map(d => d.period),
        datasets: [
          { data: data.map(d => d.incidentCount), label: 'Incidents', fill: true, tension: 0.4, borderColor: '#1976d2', backgroundColor: 'rgba(25, 118, 210, 0.2)' }
        ]
      };
    });

    this.analyticsService.getAdminSeverity(from, to).subscribe(data => {
      this.severityChartData = {
        labels: ['Critical', 'High', 'Medium', 'Low'],
        datasets: [{ 
          data: [data.critical, data.high, data.medium, data.low],
          backgroundColor: ['#d32f2f', '#ed6c02', '#ff9800', '#4caf50']
        }]
      };
    });

    this.analyticsService.getAdminStatus(from, to).subscribe(data => {
      this.statusChartData = {
        labels: ['Open', 'Assigned', 'Investigating', 'Mitigating', 'Resolved', 'Closed', 'Escalated'],
        datasets: [{
          data: [data.open, data.assigned, data.investigating, data.mitigating, data.resolved, data.closed, data.escalated],
          label: 'Status Distribution',
          backgroundColor: '#3f51b5'
        }]
      };
      
      this.isLoading = false;
    });
  }
}
