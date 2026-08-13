import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { CollaborationService } from '../../../../core/services/collaboration.service';

@Component({
  selector: 'app-attachment-uploader',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatProgressBarModule],
  template: `
    <div class="uploader-container">
      <input type="file" #fileInput style="display: none" (change)="onFileSelected($event)" accept=".jpg,.jpeg,.png,.pdf,.doc,.docx" />
      <button mat-stroked-button color="primary" (click)="fileInput.click()" [disabled]="isUploading">
        <mat-icon>cloud_upload</mat-icon> Upload File
      </button>
      <div *ngIf="isUploading" class="progress-section">
        <mat-progress-bar mode="indeterminate"></mat-progress-bar>
        <span>Uploading...</span>
      </div>
      <div *ngIf="error" class="error-text">
        <mat-icon>error</mat-icon> {{ error }}
      </div>
    </div>
  `,
  styles: [`
    .uploader-container { margin-bottom: 24px; }
    .progress-section { margin-top: 16px; display: flex; align-items: center; gap: 16px; }
    .progress-section mat-progress-bar { flex: 1; }
    .error-text { color: #f44336; display: flex; align-items: center; gap: 8px; margin-top: 8px; }
  `]
})
export class AttachmentUploaderComponent {
  @Input() incidentId!: number;
  @Output() fileUploaded = new EventEmitter<void>();

  isUploading = false;
  error: string | null = null;

  constructor(private collaborationService: CollaborationService) {}

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      if (file.size > 10 * 1024 * 1024) {
        this.error = "File exceeds 10MB limit.";
        return;
      }
      this.upload(file);
    }
  }

  private upload(file: File) {
    this.error = null;
    this.isUploading = true;
    
    this.collaborationService.uploadAttachment(this.incidentId, file).subscribe({
      next: () => {
        this.isUploading = false;
        this.fileUploaded.emit();
      },
      error: (err) => {
        this.error = err.error?.message || 'Upload failed.';
        this.isUploading = false;
      }
    });
  }
}
