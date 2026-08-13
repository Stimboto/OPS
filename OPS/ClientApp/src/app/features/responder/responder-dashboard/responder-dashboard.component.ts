import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { RouterModule } from '@angular/router';
import { AnalyticsFilterComponent, AnalyticsFilter } from '../../../shared/components/analytics-filter/analytics-filter.component';
import { AnalyticsService } from '../../../core/services/analytics.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { IncidentService } from '../../../core/services/incident.service';
import { BaseChartDirective } from 'ng2-charts';
import { Subscription, Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { ChartConfiguration, ChartData } from 'chart.js';

import { OverviewStatsDto } from '../../../core/models/analytics.model';
import { IncidentListDto, IncidentPriority, IncidentSeverity, IncidentStatus } from '../../../core/models/incident.model';

@Component({
  selector: 'app-responder-dashboard',
  standalone: true,
  imports: [
    CommonModule, 
    MatCardModule, 
    MatTableModule, 
    MatButtonModule, 
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    RouterModule,
    AnalyticsFilterComponent,
    BaseChartDirective
  ],
  templateUrl: './responder-dashboard.component.html',
  styleUrls: ['./responder-dashboard.component.scss']
})
export class ResponderDashboardComponent implements OnInit, OnDestroy {
  isLoading = true;
  currentFilter: AnalyticsFilter = {};

  overview!: OverviewStatsDto;
  prioritizedIncidents: IncidentListDto[] = [];
  
  incidentColumns: string[] = ['trackingId', 'title', 'severity', 'priority', 'status', 'resolutionDueAt', 'actions'];

  // Chart
  statusChartData: ChartData<'doughnut'> = { labels: [], datasets: [] };
  statusChartOptions: ChartConfiguration['options'] = { responsive: true };

  private subscriptions = new Subscription();
  private refreshSubject = new Subject<void>();

  constructor(
    private analyticsService: AnalyticsService,
    private incidentService: IncidentService,
    private signalRService: SignalRService
  ) {}

  ngOnInit(): void {
    this.refreshSubject.pipe(debounceTime(2000)).subscribe(() => {
      this.loadAnalytics();
    });

    this.loadAnalytics();

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
    
    this.analyticsService.getResponderOverview(from, to).subscribe(data => {
      this.overview = data;
      
      this.statusChartData = {
        labels: ['Investigating', 'Mitigating', 'Resolved', 'Active (Other)'],
        datasets: [{
          data: [
            data.investigating, 
            data.mitigating, 
            data.resolved, 
            data.activeIncidents - (data.investigating + data.mitigating)
          ],
          backgroundColor: ['#ff9800', '#2196f3', '#4caf50', '#9e9e9e']
        }]
      };
    });

    // We fetch all assigned incidents for the responder and sort them client-side for priority
    // For large datasets, a dedicated prioritized endpoint is better, but this works for standard queues.
    this.incidentService.getIncidents().subscribe(data => {
      // 1. SLA Breached
      // 2. SLA At Risk
      // 3. Critical
      // 4. High
      // 5. Medium
      // 6. Low

      const myIncidents = data.filter(i => 
        i.status !== IncidentStatus.Resolved && i.status !== IncidentStatus.Closed
      );

      this.prioritizedIncidents = myIncidents.sort((a, b) => {
        if (a.resolutionSlaBreached && !b.resolutionSlaBreached) return -1;
        if (!a.resolutionSlaBreached && b.resolutionSlaBreached) return 1;

        if (a.severity === IncidentSeverity.Critical && b.severity !== IncidentSeverity.Critical) return -1;
        if (a.severity !== IncidentSeverity.Critical && b.severity === IncidentSeverity.Critical) return 1;

        if (a.severity === IncidentSeverity.High && b.severity !== IncidentSeverity.High) return -1;
        if (a.severity !== IncidentSeverity.High && b.severity === IncidentSeverity.High) return 1;

        return 0;
      });

      this.isLoading = false;
    });
  }

  getStatusName(status: IncidentStatus): string {
    return IncidentStatus[status];
  }

  getSeverityName(severity: IncidentSeverity): string {
    return IncidentSeverity[severity];
  }

  getPriorityName(priority: IncidentPriority): string {
    return IncidentPriority[priority];
  }
}
