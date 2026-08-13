import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CollaborationService } from '../../../../core/services/collaboration.service';

@Component({
  selector: 'app-comment-composer',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule],
  template: `
    <div class="comment-composer">
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Add a comment...</mat-label>
        <textarea matInput [(ngModel)]="commentText" rows="3" [disabled]="isSubmitting"></textarea>
      </mat-form-field>
      <div class="actions">
        <button mat-raised-button color="primary" [disabled]="!commentText.trim() || isSubmitting" (click)="submit()">
          <mat-icon>send</mat-icon> Post Comment
        </button>
      </div>
    </div>
  `,
  styles: [`
    .comment-composer { margin-bottom: 24px; }
    .full-width { width: 100%; }
    .actions { display: flex; justify-content: flex-end; }
  `]
})
export class CommentComposerComponent {
  @Input() incidentId!: number;
  @Output() commentPosted = new EventEmitter<void>();

  commentText: string = '';
  isSubmitting = false;

  constructor(private collaborationService: CollaborationService) {}

  submit() {
    if (!this.commentText.trim() || !this.incidentId) return;

    this.isSubmitting = true;
    this.collaborationService.createComment(this.incidentId, { commentText: this.commentText }).subscribe({
      next: () => {
        this.commentText = '';
        this.isSubmitting = false;
        this.commentPosted.emit();
      },
      error: (err) => {
        console.error('Failed to post comment', err);
        this.isSubmitting = false;
      }
    });
  }
}
