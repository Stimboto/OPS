import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  CommentDto, 
  CreateCommentDto, 
  UpdateCommentDto, 
  AttachmentDto,
  ActivityFeedDto
} from '../models/collaboration.model';

@Injectable({
  providedIn: 'root'
})
export class CollaborationService {
  private apiUrl = `${environment.apiUrl}/incidents`;

  constructor(private http: HttpClient) {}

  // Comments
  getComments(incidentId: number): Observable<CommentDto[]> {
    return this.http.get<CommentDto[]>(`${this.apiUrl}/${incidentId}/comments`);
  }

  createComment(incidentId: number, dto: CreateCommentDto): Observable<CommentDto> {
    return this.http.post<CommentDto>(`${this.apiUrl}/${incidentId}/comments`, dto);
  }

  updateComment(incidentId: number, commentId: number, dto: UpdateCommentDto): Observable<CommentDto> {
    return this.http.put<CommentDto>(`${this.apiUrl}/${incidentId}/comments/${commentId}`, dto);
  }

  deleteComment(incidentId: number, commentId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${incidentId}/comments/${commentId}`);
  }

  // Attachments
  getAttachments(incidentId: number): Observable<AttachmentDto[]> {
    return this.http.get<AttachmentDto[]>(`${this.apiUrl}/${incidentId}/attachments`);
  }

  uploadAttachment(incidentId: number, file: File): Observable<AttachmentDto> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<AttachmentDto>(`${this.apiUrl}/${incidentId}/attachments`, formData);
  }

  downloadAttachment(attachmentId: number): Observable<Blob> {
    return this.http.get(`${environment.apiUrl}/attachments/${attachmentId}`, { responseType: 'blob' });
  }

  deleteAttachment(attachmentId: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/attachments/${attachmentId}`);
  }

  // Activity Feed
  getActivityFeed(incidentId: number): Observable<ActivityFeedDto[]> {
    return this.http.get<ActivityFeedDto[]>(`${this.apiUrl}/${incidentId}/activity`);
  }
}
