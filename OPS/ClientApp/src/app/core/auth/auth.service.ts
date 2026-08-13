import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'ops_token';
  private readonly ROLE_KEY = 'ops_role';
  private readonly USER_KEY = 'ops_user';

  constructor() {}

  setSession(token: string, role: string, user: string) {
    localStorage.setItem(this.TOKEN_KEY, token);
    localStorage.setItem(this.ROLE_KEY, role);
    localStorage.setItem(this.USER_KEY, user);
  }

  clearSession() {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.ROLE_KEY);
    localStorage.removeItem(this.USER_KEY);
  }

  logout() {
    this.clearSession();
  }

  getCurrentUser(): { fullName: string, role: string } | null {
    const name = localStorage.getItem(this.USER_KEY);
    const role = this.getRole();
    if (name && role) {
      return { fullName: name, role: role };
    }
    return null;
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRole(): string | null {
    return localStorage.getItem(this.ROLE_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  hasRole(expectedRoles: string[]): boolean {
    const currentRole = this.getRole();
    if (!currentRole) return false;
    return expectedRoles.includes(currentRole);
  }
}
