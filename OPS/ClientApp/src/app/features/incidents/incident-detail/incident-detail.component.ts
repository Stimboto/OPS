import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { Subscription } from 'rxjs';
import { IncidentService } from '../../../core/services/incident.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { AuthService } from '../../../core/auth/auth.service';
import { IncidentDetailDto, IncidentStatus, IncidentSeverity, IncidentPriority } from '../../../core/models/incident.model';
import { ActivityFeedComponent } from '../components/activity-feed/activity-feed.component';
import { CommentComposerComponent } from '../components/comment-composer/comment-composer.component';
import { AttachmentUploaderComponent } from '../components/attachment-uploader/attachment-uploader.component';

@Component({
  selector: 'app-incident-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    ActivityFeedComponent,
    CommentComposerComponent,
    AttachmentUploaderComponent
  ],
  templateUrl: './incident-detail.component.html',
  styleUrls: ['./incident-detail.component.scss']
})
export class IncidentDetailComponent implements OnInit {
  incident: IncidentDetailDto | null = null;
  isLoading = true;
  errorMessage: string | null = null;
  
  currentUserRole: string | null = null;
  currentUserId: number | null = null;

  responseCountdown: string = '';
  resolutionCountdown: string = '';
  isResponseBreached: boolean = false;
  isResolutionBreached: boolean = false;
  
  private countdownTimer: any;
  private subscriptions = new Subscription();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private incidentService: IncidentService,
    private authService: AuthService,
    private signalRService: SignalRService
  ) {}

  ngOnInit(): void {
    this.currentUserRole = this.authService.getRole();
    
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.loadIncident(parseInt(idParam, 10));
    }

    this.startCountdownTimer();

    this.subscriptions.add(
      this.signalRService.incidentAssigned$.subscribe(event => {
        if (this.incident && this.incident.id === event.incidentId) {
          this.incident.assignedResponderName = event.assignedResponderName;
          this.incident.assignedToUserId = event.assignedResponderId;
          this.incident.status = IncidentStatus.Assigned;
          this.incident.updatedAt = event.assignedAt;
          this.addHistoryEntry(IncidentStatus.Open, IncidentStatus.Assigned, `Assigned to ${event.assignedResponderName}`, event.assignedBy, event.assignedAt);
        }
      })
    );

    this.subscriptions.add(
      this.signalRService.incidentStatusChanged$.subscribe(event => {
        if (this.incident && this.incident.id === event.incidentId) {
          this.incident.status = event.newStatus;
          this.incident.updatedAt = event.changedAt;
          if (event.newStatus === IncidentStatus.Resolved || event.newStatus === IncidentStatus.Closed) {
            this.incident.resolvedAt = event.changedAt;
            // Also refresh from backend to get precise SLA states if needed
            this.loadIncident(event.incidentId, true);
          } else if (event.newStatus === IncidentStatus.Reopened) {
            this.incident.resolvedAt = null;
          }
          this.addHistoryEntry(event.oldStatus, event.newStatus, event.remarks, event.changedBy, event.changedAt);
        }
      })
    );

    this.subscriptions.add(
      this.signalRService.slaWarning$.subscribe(event => {
        if (this.incident && this.incident.id === event.incidentId) {
           // Reload incident to get updated fields, or just show alert
           this.loadIncident(event.incidentId, true);
        }
      })
    );

    this.subscriptions.add(
      this.signalRService.slaBreached$.subscribe(event => {
        if (this.incident && this.incident.id === event.incidentId) {
           // Reload to pick up escalation status and history
           this.loadIncident(event.incidentId, true);
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
    if (this.countdownTimer) {
      clearInterval(this.countdownTimer);
    }
  }

  loadIncident(id: number, silent: boolean = false): void {
    if (!silent) this.isLoading = true;
    this.incidentService.getIncident(id).subscribe({
      next: (data) => {
        this.incident = data;
        this.updateCountdowns();
        this.isLoading = false;
      },
      error: (err) => {
        if (!silent) this.errorMessage = err.error?.message || 'Failed to load incident.';
        this.isLoading = false;
      }
    });
  }

  private addHistoryEntry(oldStatus: IncidentStatus, newStatus: IncidentStatus, remarks: string, changedBy: string, changedAt: string) {
    if (this.incident) {
       this.incident.history.unshift({ oldStatus, newStatus, remarks, changedByUserName: changedBy, changedAt });
    }
  }

  private startCountdownTimer() {
    this.countdownTimer = setInterval(() => {
      this.updateCountdowns();
    }, 1000);
  }

  private updateCountdowns() {
    if (!this.incident) return;
    
    this.responseCountdown = this.formatTimeDiff(this.incident.responseDueAt, this.incident.responseAt);
    this.isResponseBreached = this.checkBreach(this.incident.responseDueAt, this.incident.responseAt);
    
    this.resolutionCountdown = this.formatTimeDiff(this.incident.resolutionDueAt, this.incident.resolvedAt);
    this.isResolutionBreached = this.checkBreach(this.incident.resolutionDueAt, this.incident.resolvedAt);
  }

  private formatTimeDiff(dueAtStr: string, completedAtStr: string | null): string {
    const dueTime = new Date(dueAtStr).getTime();
    const endTime = completedAtStr ? new Date(completedAtStr).getTime() : Date.now();
    
    const diff = dueTime - endTime;
    const absDiff = Math.abs(diff);
    
    const hours = Math.floor(absDiff / (1000 * 60 * 60));
    const minutes = Math.floor((absDiff % (1000 * 60 * 60)) / (1000 * 60));
    const seconds = Math.floor((absDiff % (1000 * 60)) / 1000);
    
    const formatted = `${hours}h ${minutes}m ${seconds}s`;
    
    if (completedAtStr) {
       return diff >= 0 ? `Met (${formatted} early)` : `Breached by ${formatted}`;
    }
    
    return diff >= 0 ? `${formatted} left` : `Overdue by ${formatted}`;
  }

  private checkBreach(dueAtStr: string, completedAtStr: string | null): boolean {
    if (completedAtStr) {
      return new Date(completedAtStr).getTime() > new Date(dueAtStr).getTime();
    }
    return Date.now() > new Date(dueAtStr).getTime();
  }

  canTimeTravel(): boolean {
    return this.currentUserRole === 'Admin';
  }

  triggerTimeTravel(): void {
    if (!this.incident) return;
    this.isLoading = true;
    this.incidentService.timeTravelSla(this.incident.id, 60).subscribe({
      next: () => {
        // Will refresh either immediately or via SignalR. Let's just reload.
        this.loadIncident(this.incident!.id);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to time travel.';
        this.isLoading = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/incidents']);
  }

  canAssign(): boolean {
    return this.currentUserRole === 'Manager' || this.currentUserRole === 'Admin';
  }

  canUpdateStatus(): boolean {
    if (this.currentUserRole === 'Manager' || this.currentUserRole === 'Admin') return true;
    // Basic UI check. Real check happens on backend.
    if (this.currentUserRole === 'Responder') return true;
    return false;
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
