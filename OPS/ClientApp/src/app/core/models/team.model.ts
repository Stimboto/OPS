export interface TeamListDto {
  id: number;
  name: string;
  description: string;
  memberCount: number;
  activeIncidentCount: number;
  managerCount: number;
  responderCount: number;
  createdAt: string;
}

export interface TeamDetailDto extends TeamListDto {
  members: UserTeamDto[];
  resolvedIncidentCount: number;
  slaBreachedIncidentCount: number;
}

export interface UserTeamDto {
  userId: number;
  fullName: string;
  email: string;
  roleName: string;
}

export interface CreateTeamRequest {
  name: string;
  description: string;
}

export interface UpdateTeamRequest {
  name: string;
  description: string;
}
