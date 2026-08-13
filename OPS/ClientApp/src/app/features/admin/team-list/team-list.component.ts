import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { TeamService } from '../../../core/services/team.service';
import { TeamListDto } from '../../../core/models/team.model';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-team-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatInputModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './team-list.component.html',
  styleUrls: ['./team-list.component.scss']
})
export class TeamListComponent implements OnInit {
  displayedColumns: string[] = ['name', 'description', 'memberCount', 'managerCount', 'responderCount', 'activeIncidentCount', 'actions'];
  dataSource: MatTableDataSource<TeamListDto>;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private teamService: TeamService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.dataSource = new MatTableDataSource<TeamListDto>([]);
  }

  ngOnInit(): void {
    this.loadTeams();
  }

  loadTeams(): void {
    this.teamService.getTeams().subscribe({
      next: (data) => {
        this.dataSource = new MatTableDataSource(data);
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
      },
      error: (err) => console.error(err)
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  viewTeam(id: number): void {
    this.router.navigate(['/admin/teams', id]);
  }

  deleteTeam(id: number): void {
    if (confirm('Are you sure you want to delete this team?')) {
      this.teamService.deleteTeam(id).subscribe({
        next: () => {
          this.snackBar.open('Team deleted successfully', 'Close', { duration: 3000 });
          this.loadTeams();
        },
        error: (err) => {
          this.snackBar.open(err.error?.message || 'Error deleting team', 'Close', { duration: 5000 });
        }
      });
    }
  }
}
