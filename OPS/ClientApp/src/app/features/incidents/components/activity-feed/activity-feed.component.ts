import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CollaborationService } from '../../../../core/services/collaboration.service';
import { SignalRService } from '../../../../core/services/signalr.service';
import { ActivityFeedDto } from '../../../../core/models/collaboration.model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-activity-feed',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule, MatButtonModule],
  template: `
    <div class="activity-feed">
      <div class="timeline">
        <div class="timeline-item" *ngFor="let item of activities">
          <div class="timeline-marker" [ngClass]="getMarkerClass(item.type)">
            <mat-icon>{{ getIcon(item.type) }}</mat-icon>
          </div>
          <div class="timeline-content">
            <div class="timeline-header">
              <strong>{{ item.actor }}</strong> {{ item.action }}
              <span class="timeline-date">{{ item.timestamp | date:'medium' }}</span>
            </div>
            
            <div class="timeline-body" *ngIf="item.details">
              <p *ngIf="item.type === 'Comment'" class="comment-text">{{ item.details }}</p>
              <p *ngIf="item.type === 'History'" class="history-remarks">{{ item.details }}</p>
              
              <div *ngIf="item.type === 'Attachment'" class="attachment-preview">
                <mat-icon>insert_drive_file</mat-icon>
                <span>{{ item.details }}</span>
                <button mat-icon-button color="primary" (click)="download(item.id!)">
                  <mat-icon>download</mat-icon>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div *ngIf="activities.length === 0" class="no-activity">
        No activity found.
      </div>
    </div>
  `,
  styles: [`
    .activity-feed { padding: 16px 0; }
    .timeline { position: relative; margin-left: 20px; border-left: 2px solid #e0e0e0; padding-left: 30px; }
    .timeline-item { position: relative; margin-bottom: 24px; }
    .timeline-marker { position: absolute; left: -46px; width: 32px; height: 32px; border-radius: 50%; display: flex; align-items: center; justify-content: center; color: white; background: #9e9e9e; box-shadow: 0 0 0 4px #fafafa; }
    .timeline-marker.history { background: #2196f3; }
    .timeline-marker.comment { background: #4caf50; }
    .timeline-marker.attachment { background: #ff9800; }
    .timeline-marker mat-icon { font-size: 18px; width: 18px; height: 18px; }
    .timeline-header { margin-bottom: 8px; font-size: 14px; color: #333; }
    .timeline-date { margin-left: 8px; font-size: 12px; color: #888; }
    .timeline-body { background: #f5f5f5; padding: 12px; border-radius: 4px; border: 1px solid #e0e0e0; }
    .comment-text { margin: 0; white-space: pre-wrap; font-size: 14px; color: #444; }
    .history-remarks { margin: 0; font-style: italic; color: #666; font-size: 13px; }
    .attachment-preview { display: flex; align-items: center; gap: 8px; }
    .attachment-preview span { flex: 1; font-size: 14px; }
    .no-activity { text-align: center; color: #888; padding: 32px; font-style: italic; }
  `]
})
export class ActivityFeedComponent implements OnInit, OnDestroy {
  @Input() incidentId!: number;
  activities: ActivityFeedDto[] = [];
  private subscriptions = new Subscription();

  constructor(
    private collaborationService: CollaborationService,
    private signalRService: SignalRService
  ) {}

  ngOnInit() {
    this.loadActivity();

    // Listen for real-time events to refresh feed
    this.subscriptions.add(this.signalRService.commentCreated$.subscribe(e => {
      if (e.incidentId === this.incidentId) this.loadActivity();
    }));
    this.subscriptions.add(this.signalRService.attachmentUploaded$.subscribe(e => {
      if (e.incidentId === this.incidentId) this.loadActivity();
    }));
    this.subscriptions.add(this.signalRService.incidentStatusChanged$.subscribe(e => {
      if (e.incidentId === this.incidentId) this.loadActivity();
    }));
    this.subscriptions.add(this.signalRService.incidentAssigned$.subscribe(e => {
      if (e.incidentId === this.incidentId) this.loadActivity();
    }));
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  loadActivity() {
    if (!this.incidentId) return;
    this.collaborationService.getActivityFeed(this.incidentId).subscribe(data => {
      this.activities = data;
    });
  }

  getMarkerClass(type: string) {
    return type.toLowerCase();
  }

  getIcon(type: string) {
    if (type === 'Comment') return 'chat';
    if (type === 'Attachment') return 'attachment';
    return 'history';
  }

  download(attachmentId: number) {
    this.collaborationService.downloadAttachment(attachmentId).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `attachment-${attachmentId}`;
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }
}
