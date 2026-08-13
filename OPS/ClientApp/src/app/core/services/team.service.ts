import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TeamListDto, TeamDetailDto, CreateTeamRequest, UpdateTeamRequest, UserTeamDto } from '../models/team.model';

@Injectable({
  providedIn: 'root'
})
export class TeamService {
  private apiUrl = `${environment.apiUrl}/teams`;

  constructor(private http: HttpClient) {}

  getTeams(): Observable<TeamListDto[]> {
    return this.http.get<TeamListDto[]>(this.apiUrl);
  }

  getTeam(id: number): Observable<TeamDetailDto> {
    return this.http.get<TeamDetailDto>(`${this.apiUrl}/${id}`);
  }

  createTeam(request: CreateTeamRequest): Observable<TeamDetailDto> {
    return this.http.post<TeamDetailDto>(this.apiUrl, request);
  }

  updateTeam(id: number, request: UpdateTeamRequest): Observable<TeamDetailDto> {
    return this.http.put<TeamDetailDto>(`${this.apiUrl}/${id}`, request);
  }

  deleteTeam(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getTeamMembers(teamId: number): Observable<UserTeamDto[]> {
    return this.http.get<UserTeamDto[]>(`${this.apiUrl}/${teamId}/members`);
  }

  addMemberToTeam(teamId: number, userId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${teamId}/members`, userId);
  }

  removeMemberFromTeam(teamId: number, userId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${teamId}/members/${userId}`);
  }
}
