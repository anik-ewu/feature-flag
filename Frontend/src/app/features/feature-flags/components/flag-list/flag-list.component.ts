import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FeatureFlagApiService } from '../../../../core/http/feature-flag-api.service';
import { FeatureFlag } from '../../../../core/models/feature-flag.model';

@Component({
    selector: 'app-flag-list',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        MatTableModule,
        MatButtonModule,
        MatIconModule,
        MatSlideToggleModule,
        MatCardModule,
        MatProgressSpinnerModule
    ],
    template: `
    <div class="header-actions">
      <h2>Feature Flags</h2>
      <button mat-raised-button color="primary" routerLink="new">
        <mat-icon>add</mat-icon> Create Flag
      </button>
    </div>

    <mat-card>
      <div *ngIf="isLoading" class="loading-shade">
        <mat-spinner></mat-spinner>
      </div>

      <table mat-table [dataSource]="flags" class="mat-elevation-z1">
        <!-- Key Column -->
        <ng-container matColumnDef="key">
          <th mat-header-cell *matHeaderCellDef> Flag Key </th>
          <td mat-cell *matCellDef="let flag"> 
            <strong>{{flag.key}}</strong><br>
            <span class="text-muted">{{flag.environment}}</span>
          </td>
        </ng-container>

        <!-- Description Column -->
        <ng-container matColumnDef="description">
          <th mat-header-cell *matHeaderCellDef> Description </th>
          <td mat-cell *matCellDef="let flag"> {{flag.description}} </td>
        </ng-container>

        <!-- Status Column -->
        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef> Status </th>
          <td mat-cell *matCellDef="let flag"> 
            <mat-slide-toggle 
              [checked]="flag.isEnabled" 
              (change)="toggleFlag(flag, $event.checked)">
            </mat-slide-toggle>
          </td>
        </ng-container>

        <!-- Rollout Column -->
        <ng-container matColumnDef="rollout">
          <th mat-header-cell *matHeaderCellDef> Rollout % </th>
          <td mat-cell *matCellDef="let flag"> {{flag.rolloutPercentage}}% </td>
        </ng-container>

        <!-- Actions Column -->
        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef>Actions</th>
          <td mat-cell *matCellDef="let flag">
            <button mat-icon-button color="primary" [routerLink]="['edit', flag.id]">
              <mat-icon>edit</mat-icon>
            </button>
            <button mat-icon-button color="warn" (click)="deleteFlag(flag)">
              <mat-icon>delete</mat-icon>
            </button>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
      </table>
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

    flags: FeatureFlag[] = [];
    displayedColumns: string[] = ['key', 'description', 'status', 'rollout', 'actions'];
    isLoading = true;

    // Dummy Project ID for currently selected project
    currentProjectId = '00000000-0000-0000-0000-000000000000';

    ngOnInit() {
        this.loadFlags();
    }

    loadFlags() {
        this.isLoading = true;
        this.api.getFlagsByProject(this.currentProjectId).subscribe({
            next: (data) => {
                this.flags = data;
                this.isLoading = false;
            },
            error: (err) => {
                console.error('Failed to load flags', err);
                this.isLoading = false;
            }
        });
    }

    toggleFlag(flag: FeatureFlag, isEnabled: boolean) {
        this.api.updateFlag({
            id: flag.id,
            projectId: flag.projectId,
            description: flag.description,
            isEnabled,
            rolloutPercentage: flag.rolloutPercentage
        }).subscribe({
            next: () => flag.isEnabled = isEnabled,
            error: () => {
                // Revert toggle on failure
                flag.isEnabled = !isEnabled;
                alert('Failed to update flag state.');
            }
        });
    }

    deleteFlag(flag: FeatureFlag) {
        if (confirm(`Are you sure you want to delete flag ${flag.key}?`)) {
            this.api.deleteFlag(flag.id, flag.projectId).subscribe({
                next: () => this.loadFlags(),
                error: () => alert('Failed to delete flag.')
            });
        }
    }
}
