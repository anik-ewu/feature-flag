import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiKeysPanelComponent } from '../api-keys/components/api-keys-panel/api-keys-panel.component';
import { ProjectApiService, ProjectDto } from '../../core/http/project-api.service';
import { ProjectContextService } from '../../core/services/project-context.service';
import { ProjectCreateDialogComponent } from './components/project-create-dialog/project-create-dialog.component';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatDialogModule, MatSnackBarModule, ApiKeysPanelComponent],
  template: `
    <div class="projects-container">
      <div class="header-row">
        <div>
          <h2>Projects</h2>
          <p>Select an active project below to manage its Feature Flags and API Keys</p>
        </div>
        <button mat-raised-button color="primary" (click)="openCreateProjectDialog()">
          <mat-icon>add</mat-icon> New Project
        </button>
      </div>
      
      <div class="grid-container" *ngIf="!isLoading; else loading">
        <mat-card *ngFor="let p of projects" 
                  class="project-card" 
                  [class.active]="p.id === activeProjectId"
                  (click)="selectProject(p.id)">
          <mat-card-header>
            <mat-card-title>{{ p.name }}</mat-card-title>
            <mat-card-subtitle>{{ p.id }}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <p *ngIf="p.id === activeProjectId" class="active-label">
              <mat-icon color="primary" inline>check_circle</mat-icon> Active Global Context
            </p>
          </mat-card-content>
        </mat-card>
      </div>

      <!-- SDK Keys Panel (Only shows Keys for the active project) -->
      <mat-card class="api-keys-card mt-4" *ngIf="activeProjectId">
        <mat-card-content>
          <h3>Environment Tokens for Active Project</h3>
          <app-api-keys-panel [projectId]="activeProjectId"></app-api-keys-panel>
        </mat-card-content>
      </mat-card>
    </div>

    <ng-template #loading>
      <mat-spinner diameter="40"></mat-spinner>
    </ng-template>
  `,
  styles: [`
    .projects-container { padding: 20px; max-width: 1400px; margin: 0 auto; }
    .header-row { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 20px; }
    .grid-container { display: grid; grid-template-columns: repeat(auto-fill, minmax(350px, 1fr)); gap: 20px; }
    .project-card { cursor: pointer; transition: all 0.2s; border: 2px solid transparent; }
    .project-card:hover { transform: translateY(-2px); box-shadow: 0 4px 12px rgba(0,0,0,0.1); }
    .project-card.active { border-color: #3f51b5; background-color: #f8f9fa; }
    .active-label { margin-top: 15px; color: #3f51b5; font-weight: 500; display: flex; align-items: center; gap: 5px; }
    .api-keys-card { max-width: 100%; margin-top: 40px; }
  `]
})
export class ProjectsComponent implements OnInit {
  private api = inject(ProjectApiService);
  private context = inject(ProjectContextService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  projects: ProjectDto[] = [];
  activeProjectId: string | null = null;
  isLoading = true;

  // Hardcoded Demo Tenant (in a real app, from Auth/JWT)
  tenantId = '11111111-1111-1111-1111-111111111111';

  ngOnInit() {
    // Subscribe to context so we always highlight the active project if changed elsewhere
    this.context.currentProjectId$.subscribe(id => {
      this.activeProjectId = id;
    });

    this.loadProjects();
  }

  loadProjects() {
    this.isLoading = true;
    this.api.getProjectsByTenant(this.tenantId).subscribe({
      next: (data) => {
        this.projects = data;
        this.isLoading = false;

        // Auto-select first project if we don't have a valid active one mapping to our grid and grid has at least 1 item
        if (this.projects.length > 0 && (!this.activeProjectId || !this.projects.some(p => p.id === this.activeProjectId))) {
          this.selectProject(this.projects[0].id);
        }
      },
      error: () => this.isLoading = false
    });
  }

  selectProject(projectId: string) {
    this.context.setProjectId(projectId);
  }

  openCreateProjectDialog() {
    const dialogRef = this.dialog.open(ProjectCreateDialogComponent, {
      width: '500px'
    });

    dialogRef.afterClosed().subscribe(projectName => {
      if (projectName) {
        this.isLoading = true;
        this.api.createProject({ name: projectName, tenantId: this.tenantId }).subscribe({
          next: () => {
            this.snackBar.open('Project created successfully', 'Close', { duration: 3000 });
            this.loadProjects(); // Reload the grid to fetch the new project
          },
          error: () => {
            this.snackBar.open('Failed to create project', 'Close', { duration: 4000 });
            this.isLoading = false;
          }
        });
      }
    });
  }
}
