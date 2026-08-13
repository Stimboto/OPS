import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatBadgeModule } from '@angular/material/badge';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Subscription } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { SignalRService, ConnectionStatus } from '../../core/services/signalr.service';

@Component({
  selector: 'app-app-layout',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    MatToolbarModule, 
    MatButtonModule, 
    MatIconModule, 
    MatSidenavModule, 
    MatListModule,
    MatBadgeModule,
    MatSnackBarModule
  ],
  templateUrl: './app-layout.component.html',
  styleUrls: ['./app-layout.component.scss']
})
export class AppLayoutComponent implements OnInit, OnDestroy {
  userName: string = '';
  userRole: string = '';
  
  unreadNotifications = 0;
  connectionStatus: ConnectionStatus = 'Offline';
  private subscriptions = new Subscription();

  constructor(
    private authService: AuthService,
    private router: Router,
    private signalRService: SignalRService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    const user = this.authService.getCurrentUser();
    this.userName = user?.fullName || 'User';
    this.userRole = user?.role || '';
    
    // Connect to SignalR
    this.signalRService.startConnection();

    this.subscriptions.add(
      this.signalRService.connectionStatus$.subscribe(status => {
        this.connectionStatus = status;
      })
    );

    this.subscriptions.add(
      this.signalRService.notificationCreated$.subscribe(notification => {
        this.unreadNotifications++;
        this.snackBar.open(notification.message, 'Close', {
          duration: 5000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['success-snackbar']
        });
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
    this.signalRService.stopConnection();
  }

  logout(): void {
    this.signalRService.stopConnection();
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
