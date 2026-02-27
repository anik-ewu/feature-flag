import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { ProjectContextService } from '../../core/services/project-context.service';
import { ProjectApiService, ProjectDto } from '../../core/http/project-api.service';
import { FeatureFlagApiService } from '../../core/http/feature-flag-api.service';
import { ApiKeysService } from '../../core/http/api-keys/api-keys.service';
import { FeatureFlag } from '../../core/models/feature-flag.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatTableModule,
    MatButtonModule,
    MatSelectModule,
    RouterModule,
    FormsModule
  ],
  template: `
    <div class="dashboard-container">
      <div class="header-row">
        <div>
          <h2>Welcome to FlagMaster!</h2>
          <p>Here is an overview of your environment and recent activity.</p>
        </div>
        
        <!-- Quick Jump active project selector -->
        <mat-form-field appearance="outline" class="project-selector">
          <mat-label>Active Project</mat-label>
          <mat-select [ngModel]="activeProjectId" (ngModelChange)="onProjectChange($event)">
            <mat-option *ngFor="let p of projects" [value]="p.id">
              {{ p.name }}
            </mat-option>
          </mat-select>
        </mat-form-field>
      </div>

      <!-- At a glance stats -->
      <div class="stats-grid">
        <mat-card class="stat-card">
          <mat-card-header>
            <mat-icon mat-card-avatar color="primary">folder</mat-icon>
            <mat-card-title>{{ totalProjects }}</mat-card-title>
            <mat-card-subtitle>Total Projects</mat-card-subtitle>
          </mat-card-header>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-header>
            <mat-icon mat-card-avatar color="accent">flag</mat-icon>
            <mat-card-title>{{ totalFlags }}</mat-card-title>
            <mat-card-subtitle>Feature Flags</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content class="stat-content">
            <span class="muted-text">in current project</span>
          </mat-card-content>
        </mat-card>

        <mat-card class="stat-card">
          <mat-card-header>
            <mat-icon mat-card-avatar color="warn">vpn_key</mat-icon>
            <mat-card-title>{{ totalKeys }}</mat-card-title>
            <mat-card-subtitle>Active SDK Keys</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content class="stat-content">
            <span class="muted-text">in current project</span>
          </mat-card-content>
        </mat-card>
      </div>

      <!-- Recent Activity -->
      <mat-card class="activity-card mt-4">
        <mat-card-header>
          <mat-card-title>Recently Updated Flags</mat-card-title>
          <mat-card-subtitle>The 5 most recently modified flags in your active project</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <table mat-table [dataSource]="recentFlags" class="mat-elevation-z0">
            
            <ng-container matColumnDef="key">
              <th mat-header-cell *matHeaderCellDef> Key </th>
              <td mat-cell *matCellDef="let flag"> <strong>{{flag.key}}</strong> </td>
            </ng-container>

            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef> Status </th>
              <td mat-cell *matCellDef="let flag"> 
                <span class="status-badge" [class.enabled]="flag.isEnabled">
                  {{ flag.isEnabled ? 'Enabled' : 'Disabled' }}
                </span>
              </td>
            </ng-container>

            <ng-container matColumnDef="updatedAt">
              <th mat-header-cell *matHeaderCellDef> Last Updated </th>
              <td mat-cell *matCellDef="let flag"> 
                {{ flag.updatedAtUtc ? (flag.updatedAtUtc | date:'short') : 'N/A' }} 
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="['key', 'status', 'updatedAt']"></tr>
            <tr mat-row *matRowDef="let row; columns: ['key', 'status', 'updatedAt'];"></tr>
          </table>

          <div *ngIf="recentFlags.length === 0" class="empty-state">
            <p>No feature flags found in this project.</p>
            <button mat-stroked-button color="primary" routerLink="/feature-flags">Go Create One</button>
          </div>
        </mat-card-content>
      </mat-card>

    </div>
  `,
  styles: [`
    .dashboard-container { padding: 20px; max-width: 1200px; margin: 0 auto; }
    .header-row { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 30px; }
    .project-selector { width: 300px; }
    
    .stats-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 20px; }
    .stat-card mat-card-title { font-size: 2.5rem; line-height: 1.2; margin-bottom: 5px; }
    .stat-content { margin-top: 10px; margin-left: 56px; }
    .muted-text { color: #888; font-size: 0.85rem; }
    
    .mt-4 { margin-top: 40px; }
    .activity-card { padding-top: 10px; }
    table { width: 100%; margin-top: 15px; }
    
    .status-badge { padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 500; background: #ffebee; color: #c62828;}
    .status-badge.enabled { background: #e8f5e9; color: #2e7d32; }
    
    .empty-state { text-align: center; padding: 30px; color: #888; background: #fafafa; border-radius: 8px; margin-top: 20px; }
  `]
})
export class DashboardComponent implements OnInit {
  private projectApi = inject(ProjectApiService);
  private flagApi = inject(FeatureFlagApiService);
  private keysApi = inject(ApiKeysService);
  private context = inject(ProjectContextService);

  activeProjectId: string | null = null;
  projects: ProjectDto[] = [];

  // Hardcoded Demo Tenant (in a real app, from Auth/JWT)
  tenantId = '11111111-1111-1111-1111-111111111111';

  // Stats
  totalProjects = 0;
  totalFlags = 0;
  totalKeys = 0;

  recentFlags: FeatureFlag[] = [];

  ngOnInit() {
    // 1. Load Projects list for select menu & count
    this.projectApi.getProjectsByTenant(this.tenantId).subscribe(data => {
      this.projects = data;
      this.totalProjects = data.length;
    });

    // 2. Subscribe to Global active project
    this.context.currentProjectId$.subscribe(id => {
      this.activeProjectId = id;
      if (id) {
        this.loadProjectStats(id);
      } else {
        this.resetStats();
      }
    });
  }

  onProjectChange(newId: string) {
    this.context.setProjectId(newId);
  }

  resetStats() {
    this.totalFlags = 0;
    this.totalKeys = 0;
    this.recentFlags = [];
  }

  loadProjectStats(projectId: string) {
    // Fire off parallel requests for Flags and Keys
    forkJoin({
      flags: this.flagApi.getFlagsByProject(projectId).pipe(catchError(() => of([]))),
      keys: this.keysApi.getApiKeys(projectId).pipe(catchError(() => of([])))
    }).subscribe(results => {
      this.totalFlags = results.flags.length;
      this.totalKeys = results.keys.length;

      // Calculate recent flags: sort by updatedAtUtc desc, fallback to created if possible
      // Since BaseEntity gives us updatedAtUtc when modified
      const sorted = [...results.flags].sort((a, b) => {
        const timeA = a.updatedAtUtc ? new Date(a.updatedAtUtc).getTime() : 0;
        const timeB = b.updatedAtUtc ? new Date(b.updatedAtUtc).getTime() : 0;
        return timeB - timeA; // Descending
      });

      this.recentFlags = sorted.slice(0, 5);
    });
  }
}
