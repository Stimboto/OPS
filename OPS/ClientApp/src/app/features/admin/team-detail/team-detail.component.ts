import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TeamService } from '../../../core/services/team.service';
import { TeamDetailDto } from '../../../core/models/team.model';

@Component({
  selector: 'app-team-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatTableModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatChipsModule,
    FormsModule
  ],
  templateUrl: './team-detail.component.html',
  styleUrls: ['./team-detail.component.scss']
})
export class TeamDetailComponent implements OnInit {
  team: TeamDetailDto | null = null;
  memberColumns: string[] = ['fullName', 'email', 'roleName', 'actions'];
  newMemberId: number | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private teamService: TeamService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = Number(params.get('id'));
      if (id) {
        this.loadTeam(id);
      }
    });
  }

  loadTeam(id: number): void {
    this.teamService.getTeam(id).subscribe({
      next: (data) => this.team = data,
      error: (err) => this.snackBar.open(err.error?.message || 'Error loading team', 'Close', { duration: 3000 })
    });
  }

  addMember(): void {
    if (!this.team || !this.newMemberId) return;
    this.teamService.addMemberToTeam(this.team.id, this.newMemberId).subscribe({
      next: () => {
        this.snackBar.open('Member added successfully', 'Close', { duration: 3000 });
        this.newMemberId = null;
        this.loadTeam(this.team!.id);
      },
      error: (err) => this.snackBar.open(err.error?.message || 'Error adding member', 'Close', { duration: 3000 })
    });
  }

  removeMember(userId: number): void {
    if (!this.team) return;
    if (confirm('Are you sure you want to remove this member?')) {
      this.teamService.removeMemberFromTeam(this.team.id, userId).subscribe({
        next: () => {
          this.snackBar.open('Member removed successfully', 'Close', { duration: 3000 });
          this.loadTeam(this.team!.id);
        },
        error: (err) => this.snackBar.open(err.error?.message || 'Error removing member', 'Close', { duration: 3000 })
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/admin/teams']);
  }
}
