import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-dashboard',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="dashboard-container">
      <h2>Welcome to the SaaS Admin Dashboard!</h2>
      <p>Navigate to "Projects" or "Feature Flags" from the sidebar to manage your rollout environment.</p>
    </div>
  `,
    styles: [`
    .dashboard-container { padding: 20px; }
  `]
})
export class DashboardComponent { }
