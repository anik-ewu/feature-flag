import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule
  ],
  template: `
    <mat-sidenav-container class="sidenav-container">
      <mat-sidenav #sidenav mode="side" opened class="sidenav">
        <mat-toolbar color="primary">FlagMaster</mat-toolbar>
        <mat-nav-list>
          <a mat-list-item routerLink="/dashboard" routerLinkActive="active">
            <mat-icon matListItemIcon>dashboard</mat-icon>
            <div matListItemTitle>Dashboard</div>
          </a>
          <a mat-list-item routerLink="/projects" routerLinkActive="active">
            <mat-icon matListItemIcon>folder</mat-icon>
            <div matListItemTitle>Projects</div>
          </a>
          <a mat-list-item routerLink="/feature-flags" routerLinkActive="active">
            <mat-icon matListItemIcon>flag</mat-icon>
            <div matListItemTitle>Feature Flags</div>
          </a>
        </mat-nav-list>
      </mat-sidenav>
      <mat-sidenav-content>
        <mat-toolbar>
          <button mat-icon-button (click)="sidenav.toggle()">
            <mat-icon>menu</mat-icon>
          </button>
          <span>Feature Flag Management</span>
          <span class="spacer"></span>
          <button mat-icon-button>
            <mat-icon>account_circle</mat-icon>
          </button>
        </mat-toolbar>
        <main class="content">
          <router-outlet></router-outlet>
        </main>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    .sidenav-container {
      height: 100vh;
    }
    .sidenav {
      width: 250px;
    }
    .spacer {
      flex: 1 1 auto;
    }
    .content {
      padding: 20px;
      max-width: 1200px;
      margin: 0 auto;
    }
    .active {
      background: rgba(0,0,0,0.04);
      border-right: 4px solid #3f51b5;
    }
  `]
})
export class LayoutComponent { }
