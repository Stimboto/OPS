import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';
import { 
  NotificationDto, 
  IncidentCreatedEvent, 
  IncidentAssignedEvent, 
  IncidentStatusChangedEvent,
  SlaWarningEvent,
  SlaBreachedEvent
} from '../models/events.model';

export type ConnectionStatus = 'Offline' | 'Connecting' | 'Live' | 'Reconnecting';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  
  private connectionStatusSubject = new BehaviorSubject<ConnectionStatus>('Offline');
  public connectionStatus$ = this.connectionStatusSubject.asObservable();

  private notificationCreatedSubject = new Subject<NotificationDto>();
  public notificationCreated$ = this.notificationCreatedSubject.asObservable();

  private incidentCreatedSubject = new Subject<IncidentCreatedEvent>();
  public incidentCreated$ = this.incidentCreatedSubject.asObservable();

  private incidentAssignedSubject = new Subject<IncidentAssignedEvent>();
  public incidentAssigned$ = this.incidentAssignedSubject.asObservable();

  private incidentStatusChangedSubject = new Subject<IncidentStatusChangedEvent>();
  public incidentStatusChanged$ = this.incidentStatusChangedSubject.asObservable();

  private slaWarningSubject = new Subject<any>();
  public slaWarning$ = this.slaWarningSubject.asObservable();

  private slaBreachedSubject = new Subject<any>();
  public slaBreached$ = this.slaBreachedSubject.asObservable();

  private commentCreatedSubject = new Subject<any>();
  public commentCreated$ = this.commentCreatedSubject.asObservable();

  private attachmentUploadedSubject = new Subject<any>();
  public attachmentUploaded$ = this.attachmentUploadedSubject.asObservable();

  constructor(private authService: AuthService) {}

  public startConnection(): void {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      return; // Already connected
    }

    const token = this.authService.getToken();
    if (!token) {
      return; // Can't connect without auth
    }

    this.connectionStatusSubject.next('Connecting');

    const hubUrl = environment.apiUrl.replace('/api', '/hubs/operations');

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000]) // Retry intervals
      .build();

    this.hubConnection
      .start()
      .then(() => {
        this.connectionStatusSubject.next('Live');
        this.registerEventHandlers();
      })
      .catch((err: any) => {
        console.error('Error while starting connection: ' + err);
        this.connectionStatusSubject.next('Offline');
      });

    this.hubConnection.onreconnecting(() => {
      this.connectionStatusSubject.next('Reconnecting');
    });

    this.hubConnection.onreconnected(() => {
      this.connectionStatusSubject.next('Live');
    });

    this.hubConnection.onclose(() => {
      this.connectionStatusSubject.next('Offline');
    });
  }

  public stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop().then(() => {
        this.connectionStatusSubject.next('Offline');
        this.hubConnection = null;
      });
    }
  }

  private registerEventHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('NotificationCreated', (data: NotificationDto) => {
      this.notificationCreatedSubject.next(data);
    });

    this.hubConnection.on('IncidentCreated', (data: IncidentCreatedEvent) => {
      this.incidentCreatedSubject.next(data);
    });

    this.hubConnection.on('IncidentAssigned', (data: IncidentAssignedEvent) => {
      this.incidentAssignedSubject.next(data);
    });

    this.hubConnection.on('IncidentStatusChanged', (data: IncidentStatusChangedEvent) => {
      this.incidentStatusChangedSubject.next(data);
    });

    this.hubConnection.on('SlaWarning', (data) => {
      this.slaWarningSubject.next(data);
    });

    this.hubConnection.on('SlaBreached', (data) => {
      this.slaBreachedSubject.next(data);
    });

    this.hubConnection.on('CommentCreated', (data) => {
      this.commentCreatedSubject.next(data);
    });

    this.hubConnection.on('AttachmentUploaded', (data) => {
      this.attachmentUploadedSubject.next(data);
    });
  }
}
