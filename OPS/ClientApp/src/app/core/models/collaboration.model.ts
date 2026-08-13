export interface CommentDto {
  id: number;
  incidentId: number;
  userId: number;
  userName: string;
  commentText: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateCommentDto {
  commentText: string;
}

export interface UpdateCommentDto {
  commentText: string;
}

export interface AttachmentDto {
  id: number;
  incidentId: number;
  uploadedByUserId: number;
  uploadedByUserName: string;
  fileName: string;
  contentType: string;
  fileSize: number;
  createdAt: string;
}

export interface ActivityFeedDto {
  type: 'History' | 'Comment' | 'Attachment';
  id?: number;
  actor: string;
  action: string;
  details: string;
  timestamp: string;
}
