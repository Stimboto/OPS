import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  IncidentListDto, 
  IncidentDetailDto, 
  CreateIncidentRequest, 
  AssignIncidentRequest, 
  UpdateIncidentStatusRequest 
} from '../models/incident.model';

@Injectable({
  providedIn: 'root'
})
export class IncidentService {
  private apiUrl = `${environment.apiUrl}/incidents`;

  constructor(private http: HttpClient) {}

  getIncidents(): Observable<IncidentListDto[]> {
    return this.http.get<IncidentListDto[]>(this.apiUrl);
  }

  getIncident(id: number): Observable<IncidentDetailDto> {
    return this.http.get<IncidentDetailDto>(`${this.apiUrl}/${id}`);
  }

  createIncident(request: CreateIncidentRequest): Observable<IncidentDetailDto> {
    return this.http.post<IncidentDetailDto>(this.apiUrl, request);
  }

  assignIncident(id: number, request: AssignIncidentRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/assign`, request);
  }

  updateIncidentStatus(id: number, request: UpdateIncidentStatusRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/status`, request);
  }

  timeTravelSla(id: number, minutesToAdvance: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/time-travel?minutesToAdvance=${minutesToAdvance}`, {});
  }
}
