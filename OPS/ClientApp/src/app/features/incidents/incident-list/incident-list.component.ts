import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { IncidentService } from '../../../core/services/incident.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { IncidentListDto, IncidentStatus, IncidentSeverity, IncidentPriority } from '../../../core/models/incident.model';

@Component({
  selector: 'app-incident-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './incident-list.component.html',
  styleUrls: ['./incident-list.component.scss']
})
export class IncidentListComponent implements OnInit {
  displayedColumns: string[] = ['trackingId', 'title', 'status', 'severity', 'priority', 'teamName', 'assignedResponderName', 'createdAt', 'actions'];
  dataSource: MatTableDataSource<IncidentListDto>;
  isLoading = true;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  private subscriptions = new Subscription();

  constructor(
    private incidentService: IncidentService,
    private signalRService: SignalRService,
    private router: Router
  ) {
    this.dataSource = new MatTableDataSource<IncidentListDto>([]);
  }

  ngOnInit(): void {
    this.loadIncidents();

    this.subscriptions.add(
      this.signalRService.incidentCreated$.subscribe(event => {
        const newIncident: IncidentListDto = {
          id: event.incidentId,
          trackingId: event.trackingId,
          title: event.title,
          severity: event.severity,
          priority: event.priority,
          status: event.status,
          teamName: 'Unknown',
          reporterName: event.reporterName,
          assignedResponderName: null,
          createdAt: event.createdAt,
          updatedAt: event.createdAt,
          resolutionDueAt: null,
          responseSlaBreached: false,
          resolutionSlaBreached: false
        };
        const data = this.dataSource.data;
        data.unshift(newIncident);
        this.dataSource.data = [...data];
      })
    );

    this.subscriptions.add(
      this.signalRService.incidentAssigned$.subscribe(event => {
        const data = this.dataSource.data;
        const index = data.findIndex(i => i.id === event.incidentId);
        if (index > -1) {
          data[index].assignedResponderName = event.assignedResponderName;
          data[index].status = IncidentStatus.Assigned;
          data[index].updatedAt = event.assignedAt;
          this.dataSource.data = [...data];
        }
      })
    );

    this.subscriptions.add(
      this.signalRService.incidentStatusChanged$.subscribe(event => {
        const data = this.dataSource.data;
        const index = data.findIndex(i => i.id === event.incidentId);
        if (index > -1) {
          data[index].status = event.newStatus;
          data[index].updatedAt = event.changedAt;
          this.dataSource.data = [...data];
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadIncidents(): void {
    this.isLoading = true;
    this.incidentService.getIncidents().subscribe({
      next: (data) => {
        this.dataSource = new MatTableDataSource(data);
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  viewIncident(id: number): void {
    this.router.navigate(['/incidents', id]);
  }

  reportIncident(): void {
    this.router.navigate(['/incidents/report']);
  }

  getStatusClass(status: IncidentStatus): string {
    const statusClasses = {
      [IncidentStatus.Open]: 'status-open',
      [IncidentStatus.Assigned]: 'status-assigned',
      [IncidentStatus.Investigating]: 'status-investigating',
      [IncidentStatus.Mitigating]: 'status-mitigating',
      [IncidentStatus.Resolved]: 'status-resolved',
      [IncidentStatus.Closed]: 'status-closed',
      [IncidentStatus.Reopened]: 'status-reopened',
      [IncidentStatus.Escalated]: 'status-escalated'
    };
    return statusClasses[status] || '';
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
