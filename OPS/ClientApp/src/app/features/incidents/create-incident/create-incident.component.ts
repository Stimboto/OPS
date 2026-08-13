import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { IncidentService } from '../../../core/services/incident.service';
import { IncidentSeverity, IncidentPriority } from '../../../core/models/incident.model';

@Component({
  selector: 'app-create-incident',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule
  ],
  templateUrl: './create-incident.component.html',
  styleUrls: ['./create-incident.component.scss']
})
export class CreateIncidentComponent implements OnInit {
  incidentForm!: FormGroup;
  isSubmitting = false;
  createdTrackingId: string | null = null;
  createdIncidentId: number | null = null;
  errorMessage: string | null = null;

  severities = [
    { value: IncidentSeverity.Low, label: 'Low' },
    { value: IncidentSeverity.Medium, label: 'Medium' },
    { value: IncidentSeverity.High, label: 'High' },
    { value: IncidentSeverity.Critical, label: 'Critical' }
  ];

  priorities = [
    { value: IncidentPriority.P4, label: 'P4 (Low)' },
    { value: IncidentPriority.P3, label: 'P3 (Medium)' },
    { value: IncidentPriority.P2, label: 'P2 (High)' },
    { value: IncidentPriority.P1, label: 'P1 (Critical)' }
  ];

  // Mock teams for now until we have a Team API
  teams = [
    { id: 1, name: 'Platform Engineering' },
    { id: 2, name: 'Infrastructure' },
    { id: 3, name: 'Security' },
    { id: 4, name: 'Customer Operations' },
    { id: 5, name: 'Payments' }
  ];

  constructor(
    private fb: FormBuilder,
    private incidentService: IncidentService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.incidentForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', Validators.required],
      severity: [IncidentSeverity.Medium, Validators.required],
      priority: [IncidentPriority.P3, Validators.required],
      teamId: [null]
    });
  }

  onSubmit(): void {
    if (this.incidentForm.invalid) {
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = null;

    this.incidentService.createIncident(this.incidentForm.value).subscribe({
      next: (incident) => {
        this.isSubmitting = false;
        this.createdTrackingId = incident.trackingId;
        this.createdIncidentId = incident.id;
      },
      error: (err) => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.message || 'Failed to create incident.';
      }
    });
  }

  viewIncident(): void {
    if (this.createdIncidentId) {
      this.router.navigate(['/incidents', this.createdIncidentId]);
    }
  }

  resetForm(): void {
    this.createdTrackingId = null;
    this.createdIncidentId = null;
    this.incidentForm.reset({
      severity: IncidentSeverity.Medium,
      priority: IncidentPriority.P3,
      teamId: null
    });
  }
}
