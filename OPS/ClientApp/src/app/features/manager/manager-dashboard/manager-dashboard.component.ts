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

import { OverviewStatsDto, TeamPerformanceDto } from '../../../core/models/analytics.model';

@Component({
  selector: 'app-manager-dashboard',
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
  templateUrl: './manager-dashboard.component.html',
  styleUrls: ['./manager-dashboard.component.scss'] // Re-use styling logic
})
export class ManagerDashboardComponent implements OnInit, OnDestroy {
  isLoading = true;
  currentFilter: AnalyticsFilter = {};

  overview!: OverviewStatsDto;
  teamPerformance: TeamPerformanceDto[] = [];
  
  teamColumns: string[] = ['teamName', 'totalIncidents', 'active', 'resolved', 'slaAtRisk', 'slaBreached', 'mtta', 'mttr', 'resolutionRate'];

  // Chart
  workloadChartData: ChartData<'pie'> = { labels: [], datasets: [] };
  workloadChartOptions: ChartConfiguration['options'] = { responsive: true };

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

    this.loadAnalytics();

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
    
    this.analyticsService.getManagerOverview(from, to).subscribe(data => this.overview = data);
    
    this.analyticsService.getManagerTeams(from, to).subscribe(data => {
      this.teamPerformance = data;
      
      this.workloadChartData = {
        labels: data.map(t => t.teamName),
        datasets: [{
          data: data.map(t => t.totalIncidents),
          backgroundColor: ['#1976d2', '#388e3c', '#fbc02d', '#d32f2f', '#7b1fa2']
        }]
      };

      this.isLoading = false;
    });
  }
}
