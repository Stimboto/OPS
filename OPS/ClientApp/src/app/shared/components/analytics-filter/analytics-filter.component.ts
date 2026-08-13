import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface AnalyticsFilter {
  from?: string;
  to?: string;
}

@Component({
  selector: 'app-analytics-filter',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './analytics-filter.component.html',
  styleUrls: ['./analytics-filter.component.scss']
})
export class AnalyticsFilterComponent {
  @Output() filterChanged = new EventEmitter<AnalyticsFilter>();
  
  filterForm: FormGroup;

  constructor(private fb: FormBuilder) {
    // Default to last 30 days
    const today = new Date();
    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(today.getDate() - 30);

    this.filterForm = this.fb.group({
      from: [thirtyDaysAgo],
      to: [today]
    });
  }

  applyFilter(): void {
    const fromVal = this.filterForm.value.from as Date;
    const toVal = this.filterForm.value.to as Date;

    const filter: AnalyticsFilter = {};

    if (fromVal) {
      filter.from = fromVal.toISOString(); // Backend expects UTC
    }
    if (toVal) {
      filter.to = toVal.toISOString(); // Backend expects UTC
    }

    this.filterChanged.emit(filter);
  }

  resetFilter(): void {
    this.filterForm.reset();
    this.filterChanged.emit({});
  }
}
