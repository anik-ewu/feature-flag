import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { FeatureFlagApiService } from '../../../../core/http/feature-flag-api.service';
import { ProjectContextService } from '../../../../core/services/project-context.service';
import { FeatureFlag, CreateFeatureFlagRequest } from '../../../../core/models/feature-flag.model';
import { FlagCreateDialogComponent } from '../flag-create-dialog/flag-create-dialog.component';

@Component({
  selector: 'app-flag-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatSlideToggleModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  template: `
    <div class="header-actions">
      <h2>Feature Flags</h2>
      <button mat-raised-button color="primary" (click)="openCreateDialog()">
        <mat-icon>add</mat-icon> Create Flag
      </button>
    </div>

    <mat-card>
      <div *ngIf="isLoading" class="loading-shade">
        <mat-spinner></mat-spinner>
      </div>

      <table mat-table [dataSource]="dataSource" matSort class="mat-elevation-z1">
        
        <ng-container matColumnDef="key">
          <th mat-header-cell *matHeaderCellDef mat-sort-header> Flag Key </th>
          <td mat-cell *matCellDef="let flag"> 
            <strong>{{flag.key}}</strong><br>
            <span class="text-muted">{{flag.environment}}</span>
          </td>
        </ng-container>

        <ng-container matColumnDef="description">
          <th mat-header-cell *matHeaderCellDef> Description </th>
          <td mat-cell *matCellDef="let flag"> {{flag.description}} </td>
        </ng-container>

        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef mat-sort-header> Status </th>
          <td mat-cell *matCellDef="let flag"> 
            <mat-slide-toggle 
              color="primary"
              [checked]="flag.isEnabled" 
              (change)="toggleFlag(flag, $event.checked)">
            </mat-slide-toggle>
          </td>
        </ng-container>

        <ng-container matColumnDef="rollout">
          <th mat-header-cell *matHeaderCellDef mat-sort-header> Rollout % </th>
          <td mat-cell *matCellDef="let flag"> {{flag.rolloutPercentage}}% </td>
        </ng-container>

        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef>Actions</th>
          <td mat-cell *matCellDef="let flag">
            <button mat-icon-button color="primary" [routerLink]="['edit', flag.id]" matTooltip="Edit details & rules">
              <mat-icon>settings</mat-icon>
            </button>
            <button mat-icon-button color="warn" (click)="deleteFlag(flag)" matTooltip="Delete flag">
              <mat-icon>delete</mat-icon>
            </button>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        
        <!-- Row shown when there is no matching data. -->
        <tr class="mat-row" *matNoDataRow>
          <td class="mat-cell" colspan="5" style="text-align: center; padding: 2rem;">No feature flags found for this project.</td>
        </tr>
      </table>

      <!-- Pagination Support Added -->
      <mat-paginator [pageSizeOptions]="[10, 25, 100]" aria-label="Select page of flags"></mat-paginator>
    </mat-card>
  `,
  styles: [`
    .header-actions {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }
    .text-muted {
      color: #666;
      font-size: 0.85em;
    }
    table {
      width: 100%;
    }
    .loading-shade {
      position: absolute;
      top: 0;
      left: 0;
      bottom: 0;
      right: 0;
      background: rgba(255, 255, 255, 0.7);
      z-index: 10;
      display: flex;
      align-items: center;
      justify-content: center;
    }
  `]
})
export class FlagListComponent implements OnInit {
  private api = inject(FeatureFlagApiService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  private context = inject(ProjectContextService);

  dataSource: MatTableDataSource<FeatureFlag>;
  displayedColumns: string[] = ['key', 'description', 'status', 'rollout', 'actions'];
  isLoading = true;

  currentProjectId: string | null = null;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor() {
    this.dataSource = new MatTableDataSource<FeatureFlag>([]);
  }

  ngOnInit() {
    this.context.currentProjectId$.subscribe(id => {
      this.currentProjectId = id;
      if (id) {
        this.loadFlags();
      } else {
        this.dataSource.data = [];
      }
    });
  }

  loadFlags() {
    if (!this.currentProjectId) return;

    this.isLoading = true;
    this.api.getFlagsByProject(this.currentProjectId).subscribe({
      next: (data) => {
        this.dataSource = new MatTableDataSource(data);
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.snackBar.open('Failed to load flags', 'Dismiss', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  openCreateDialog() {
    const dialogRef = this.dialog.open(FlagCreateDialogComponent, {
      width: '500px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result && this.currentProjectId) {
        this.isLoading = true;
        const request: CreateFeatureFlagRequest = {
          ...result,
          projectId: this.currentProjectId
        };

        this.api.createFlag(request).subscribe({
          next: () => {
            this.snackBar.open('Flag created successfully', 'Close', { duration: 3000 });
            this.loadFlags(); // Refresh grid
          },
          error: () => {
            this.snackBar.open('Error creating flag', 'Close', { duration: 4000 });
            this.isLoading = false;
          }
        });
      }
    });
  }

  toggleFlag(flag: FeatureFlag, isEnabled: boolean) {
    this.api.updateFlag({
      id: flag.id,
      projectId: flag.projectId,
      description: flag.description,
      isEnabled,
      rolloutPercentage: flag.rolloutPercentage,
      targetingRules: flag.targetingRules
    }).subscribe({
      next: () => {
        flag.isEnabled = isEnabled;
        this.snackBar.open(`Flag ${isEnabled ? 'enabled' : 'disabled'}`, 'Close', { duration: 2000 });
      },
      error: () => {
        flag.isEnabled = !isEnabled; // Revert
        this.snackBar.open('Failed to toggle flag.', 'Close', { duration: 3000 });
      }
    });
  }

  deleteFlag(flag: FeatureFlag) {
    if (confirm(`Are you absolutely sure you want to delete flag ${flag.key}? This might break client apps depending on it.`)) {
      this.api.deleteFlag(flag.id, flag.projectId).subscribe({
        next: () => {
          this.snackBar.open('Flag deleted', 'Close', { duration: 3000 });
          this.loadFlags();
        },
        error: () => this.snackBar.open('Failed to delete flag.', 'Close', { duration: 3000 })
      });
    }
  }
}
