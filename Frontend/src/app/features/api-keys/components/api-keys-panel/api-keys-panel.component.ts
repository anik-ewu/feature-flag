import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ApiKeysService, ApiKeyDto } from '../../../../core/http/api-keys/api-keys.service';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-api-keys-panel',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule
  ],
  template: `
    <div class="api-keys-container">
      <h3>SDK Keys / Environment Tokens</h3>
      <p class="subtitle">Use these keys to authenticate evaluate requests from your applications.</p>

      <div class="create-key-form">
        <mat-form-field appearance="outline">
          <mat-label>Key Name</mat-label>
          <input matInput [(ngModel)]="newKeyName" placeholder="e.g. Backend Production Service">
        </mat-form-field>

        <mat-form-field appearance="outline">
          <mat-label>Environment</mat-label>
          <mat-select [(ngModel)]="newKeyEnvironment">
            <mat-option [value]="0">Development</mat-option>
            <mat-option [value]="1">Staging</mat-option>
            <mat-option [value]="2">Production</mat-option>
          </mat-select>
        </mat-form-field>

        <button mat-raised-button color="primary" (click)="createKey()" [disabled]="!newKeyName">
          Generate New Key
        </button>
      </div>

      <table mat-table [dataSource]="apiKeys" class="mat-elevation-z2 mt-3">
        
        <ng-container matColumnDef="name">
          <th mat-header-cell *matHeaderCellDef> Name </th>
          <td mat-cell *matCellDef="let key"> {{key.name}} </td>
        </ng-container>

        <ng-container matColumnDef="environment">
          <th mat-header-cell *matHeaderCellDef> Env </th>
          <td mat-cell *matCellDef="let key"> 
            <span class="env-badge env-{{key.environment.toLowerCase()}}">{{key.environment}}</span> 
          </td>
        </ng-container>

        <ng-container matColumnDef="key">
          <th mat-header-cell *matHeaderCellDef> Token (SDK Key) </th>
          <td mat-cell *matCellDef="let key"> 
            <code class="token-code">{{formatToken(key.key)}}</code>
            <button mat-icon-button (click)="copyToClipboard(key.key)" title="Copy">
              <mat-icon>content_copy</mat-icon>
            </button>
          </td>
        </ng-container>
        
        <ng-container matColumnDef="createdAt">
          <th mat-header-cell *matHeaderCellDef> Created </th>
          <td mat-cell *matCellDef="let key"> {{key.createdAt | date:'short'}} </td>
        </ng-container>

        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef> Actions </th>
          <td mat-cell *matCellDef="let key">
            <button mat-button color="warn" (click)="revokeKey(key.id)">Revoke</button>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
      </table>
      
      <div *ngIf="apiKeys.length === 0" class="empty-state">
        <p>No SDK Keys have been created for this project yet.</p>
      </div>
    </div>
  `,
  styles: [`
    .api-keys-container { padding: 15px 0; }
    .subtitle { color: #666; margin-bottom: 20px; }
    .create-key-form { display: flex; gap: 15px; align-items: center; margin-bottom: 20px; flex-wrap: wrap; }
    .mt-3 { margin-top: 15px; }
    .token-code { background: #f5f5f5; padding: 4px 8px; border-radius: 4px; font-family: monospace; user-select: all; }
    table { width: 100%; }
    .env-badge { padding: 4px 8px; border-radius: 12px; font-size: 12px; font-weight: 500; }
    .env-development { background: #e3f2fd; color: #1976d2; }
    .env-staging { background: #fff3e0; color: #f57c00; }
    .env-production { background: #ffebee; color: #d32f2f; }
    .empty-state { text-align: center; padding: 30px; color: #888; background: #fafafa; border-radius: 8px; margin-top: 20px; }
  `]
})
export class ApiKeysPanelComponent implements OnInit, OnChanges {
  @Input() projectId!: string;

  apiKeys: ApiKeyDto[] = [];
  displayedColumns: string[] = ['name', 'environment', 'key', 'createdAt', 'actions'];

  newKeyName: string = '';
  newKeyEnvironment: number = 0; // default Dev

  constructor(private apiKeysService: ApiKeysService) { }

  ngOnInit() {
    if (this.projectId) {
      this.loadKeys();
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['projectId'] && !changes['projectId'].firstChange) {
      if (this.projectId) {
        this.loadKeys();
      } else {
        this.apiKeys = [];
      }
    }
  }

  loadKeys() {
    this.apiKeysService.getApiKeys(this.projectId).subscribe({
      next: (keys) => this.apiKeys = keys,
      error: (err) => console.error('Failed to load keys', err)
    });
  }

  createKey() {
    if (!this.newKeyName) return;

    this.apiKeysService.createApiKey(this.projectId, {
      name: this.newKeyName,
      environment: this.newKeyEnvironment
    }).subscribe({
      next: (key) => {
        this.apiKeys = [key, ...this.apiKeys]; // Add to top of list
        this.newKeyName = ''; // reset form
      },
      error: (err) => console.error('Failed to create key', err)
    });
  }

  revokeKey(id: string) {
    if (confirm('Are you sure you want to revoke this SDK key? Any applications using it will lose access immediately.')) {
      this.apiKeysService.revokeApiKey(this.projectId, id).subscribe({
        next: () => {
          this.apiKeys = this.apiKeys.filter(k => k.id !== id);
        },
        error: (err) => console.error('Failed to revoke key', err)
      });
    }
  }

  copyToClipboard(token: string) {
    navigator.clipboard.writeText(token);
    // In a real app we would show a snackbar here
  }

  formatToken(token: string): string {
    if (!token || token.length <= 15) return token;
    // show dev_xxx...xxx format. "dev_" is 4 chars, plus 3 chars = 7 chars at start. 3 at end.
    return token.substring(0, 7) + '...' + token.substring(token.length - 3);
  }
}
