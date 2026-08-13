import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { 
    path: 'login', 
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) 
  },
  { 
    path: 'register', 
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent) 
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/app-layout/app-layout.component').then(m => m.AppLayoutComponent),
    children: [
      {
        path: 'reporter/dashboard',
        canActivate: [roleGuard],
        data: { roles: ['Reporter', 'Responder', 'Manager', 'Admin'] },
        loadComponent: () => import('./features/reporter/reporter-dashboard/reporter-dashboard.component').then(m => m.ReporterDashboardComponent)
      },
      {
        path: 'responder/dashboard',
        canActivate: [roleGuard],
        data: { roles: ['Responder', 'Manager', 'Admin'] },
        loadComponent: () => import('./features/responder/responder-dashboard/responder-dashboard.component').then(m => m.ResponderDashboardComponent)
      },
      {
        path: 'manager/dashboard',
        canActivate: [roleGuard],
        data: { roles: ['Manager', 'Admin'] },
        loadComponent: () => import('./features/manager/manager-dashboard/manager-dashboard.component').then(m => m.ManagerDashboardComponent)
      },
      {
        path: 'admin/dashboard',
        canActivate: [roleGuard],
        data: { roles: ['Admin'] },
        loadComponent: () => import('./features/admin/admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent)
      },
      {
        path: 'admin/teams',
        canActivate: [roleGuard],
        data: { roles: ['Admin'] },
        loadComponent: () => import('./features/admin/team-list/team-list.component').then(m => m.TeamListComponent)
      },
      {
        path: 'admin/teams/:id',
        canActivate: [roleGuard],
        data: { roles: ['Admin'] },
        loadComponent: () => import('./features/admin/team-detail/team-detail.component').then(m => m.TeamDetailComponent)
      },
      {
        path: 'incidents',
        loadComponent: () => import('./features/incidents/incident-list/incident-list.component').then(m => m.IncidentListComponent)
      },
      {
        path: 'incidents/report',
        canActivate: [roleGuard],
        data: { roles: ['Reporter', 'Responder', 'Manager', 'Admin'] },
        loadComponent: () => import('./features/incidents/create-incident/create-incident.component').then(m => m.CreateIncidentComponent)
      },
      {
        path: 'incidents/:id',
        loadComponent: () => import('./features/incidents/incident-detail/incident-detail.component').then(m => m.IncidentDetailComponent)
      }
    ]
  },
  { 
    path: 'unauthorized', 
    loadComponent: () => import('./features/error/unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent) 
  },
  { path: '**', redirectTo: '/login' }
];
