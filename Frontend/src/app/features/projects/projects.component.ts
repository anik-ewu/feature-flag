import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';

@Component({
    selector: 'app-projects',
    standalone: true,
    imports: [CommonModule, MatCardModule],
    template: `
    <div class="projects-container">
      <h2>Projects</h2>
      <p>Managing isolated logical workspaces for Feature Flags.</p>
      
      <mat-card class="project-card">
        <mat-card-header>
          <mat-card-title>Demo Project</mat-card-title>
          <mat-card-subtitle>ID: 00000000-0000-0000-0000-000000000000</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <p class="description">This is the default system placeholder project used for establishing Flag endpoints testing.</p>
        </mat-card-content>
      </mat-card>
    </div>
  `,
    styles: [`
    .projects-container { padding: 20px; }
    .description { margin-top: 15px; }
    .project-card { max-width: 450px; margin-top: 15px; }
  `]
})
export class ProjectsComponent { }
