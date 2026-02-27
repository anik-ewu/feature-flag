import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSliderModule } from '@angular/material/slider';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { FeatureFlagApiService } from '../../../../core/http/feature-flag-api.service';
import { ProjectContextService } from '../../../../core/services/project-context.service';
import { FeatureFlag, TargetingRule } from '../../../../core/models/feature-flag.model';

@Component({
  selector: 'app-flag-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatSliderModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatSnackBarModule
  ],
  template: `
    <div class="page-container" *ngIf="flag; else loading">
      <div class="header">
        <button mat-icon-button routerLink="/feature-flags"><mat-icon>arrow_back</mat-icon></button>
        <h2>Edit Flag: <code>{{flag.key}}</code></h2>
        <span class="spacer"></span>
        <button mat-raised-button color="primary" [disabled]="form.invalid" (click)="saveFlag()">
          <mat-icon>save</mat-icon> Save Changes
        </button>
      </div>

      <div class="content-grid" [formGroup]="form">
        
        <!-- Left Column: Core Settings -->
        <div class="col-main">
          <mat-card class="mb-20">
            <mat-card-header>
              <mat-card-title>Core Settings</mat-card-title>
            </mat-card-header>
            <mat-card-content class="form-content">
              
              <div class="toggle-container">
                <mat-slide-toggle formControlName="isEnabled" color="primary">
                  Master Toggle ({{ form.get('isEnabled')?.value ? 'Enabled' : 'Disabled'}})
                </mat-slide-toggle>
                <p class="hint">If disabled, the flag will always evaluate to false, regardless of rules or rollout percentage.</p>
              </div>

              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Description</mat-label>
                <textarea matInput formControlName="description" rows="3"></textarea>
              </mat-form-field>

              <mat-divider></mat-divider>
              
              <div class="slider-container">
                <h3>Percentage Rollout: {{ form.get('rolloutPercentage')?.value }}%</h3>
                <mat-slider min="0" max="100" step="1" discrete class="full-width">
                  <input matSliderThumb formControlName="rolloutPercentage">
                </mat-slider>
                <p class="hint">Use 0 to disable percentage rollout. Use 100 to enable for all valid users.</p>
              </div>

            </mat-card-content>
          </mat-card>
        </div>

        <!-- Right Column: Targeting Rules -->
        <div class="col-side">
          <mat-card>
            <mat-card-header>
              <mat-card-title>Targeting Rules (OR logic)</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <p class="hint" *ngIf="rules.length === 0">No custom targeting rules applied.</p>

              <div formArrayName="rules" class="rules-list">
                <div *ngFor="let rule of rules.controls; let i=index" [formGroupName]="i" class="rule-box mat-elevation-z1">
                  
                  <div class="rule-header">
                    <strong>Rule #{{i + 1}}</strong>
                    <button mat-icon-button color="warn" (click)="removeRule(i)"><mat-icon>close</mat-icon></button>
                  </div>

                  <div class="rule-fields">
                    <mat-form-field appearance="outline">
                      <mat-label>Target Property</mat-label>
                      <mat-select formControlName="type">
                        <mat-option value="UserId">User ID</mat-option>
                        <mat-option value="Email">Email</mat-option>
                        <mat-option value="Country">Country</mat-option>
                        <mat-option value="CustomProperty">Custom JWT Property</mat-option>
                      </mat-select>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Operator</mat-label>
                      <mat-select formControlName="operator">
                        <mat-option value="Equals">Equals</mat-option>
                        <mat-option value="Contains">Contains</mat-option>
                        <mat-option value="In">In (comma separated)</mat-option>
                      </mat-select>
                    </mat-form-field>

                    <mat-form-field appearance="outline">
                      <mat-label>Value</mat-label>
                      <input matInput formControlName="value">
                      <mat-error>Value is required</mat-error>
                    </mat-form-field>
                  </div>
                </div>
              </div>
              
              <button mat-stroked-button color="primary" class="full-width mt-10" (click)="addRule()">
                <mat-icon>add</mat-icon> Add Rule
              </button>
            </mat-card-content>
          </mat-card>
        </div>

      </div>
    </div>
    
    <ng-template #loading>
      <div class="loading-full">
        <mat-spinner></mat-spinner>
      </div>
    </ng-template>
  `,
  styles: [`
    .page-container { max-width: 1400px; margin: 0 auto; }
    .header { display: flex; align-items: center; margin-bottom: 20px; gap: 15px; }
    .spacer { flex: 1; }
    .content-grid { display: flex; flex-direction: column; gap: 20px; align-items: stretch; }
    .form-content { padding-top: 20px; }
    .mb-20 { margin-bottom: 20px; }
    .mt-10 { margin-top: 10px; }
    .full-width { width: 100%; }
    .toggle-container { margin-bottom: 25px; }
    .slider-container { margin-top: 20px; }
    .hint { color: #666; font-size: 0.85em; margin-top: 5px; }
    
    .rules-list { display: flex; flex-direction: column; gap: 15px; margin-top: 15px; }
    .rule-box { border-left: 4px solid #3f51b5; padding: 15px 15px 5px 15px; border-radius: 4px; background: #fafafa; }
    .rule-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 5px; }
    .rule-fields { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 15px; align-items: start; }
    .rule-fields mat-form-field { width: 100%; }
    
    .loading-full { display: flex; justify-content: center; align-items: center; height: 50vh; }
  `]
})
export class FlagDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private api = inject(FeatureFlagApiService);
  private fb = inject(FormBuilder);
  private snackBar = inject(MatSnackBar);
  private context = inject(ProjectContextService);

  flagId!: string;
  projectId: string | null = null;
  flag!: FeatureFlag;

  form: FormGroup = this.fb.group({
    description: [''],
    isEnabled: [false],
    rolloutPercentage: [0],
    rules: this.fb.array([])
  });

  get rules() {
    return this.form.get('rules') as FormArray;
  }

  ngOnInit() {
    this.flagId = this.route.snapshot.paramMap.get('id')!;
    this.context.currentProjectId$.subscribe(id => {
      this.projectId = id;
      if (id && !this.flag) {
        this.loadFlag();
      }
    });
  }

  loadFlag() {
    if (!this.projectId) return;

    this.api.getFlagById(this.flagId, this.projectId).subscribe({
      next: (data) => {
        this.flag = data;
        this.populateForm();
      },
      error: () => {
        this.snackBar.open('Flag not found', 'Close');
        this.router.navigate(['/feature-flags']);
      }
    });
  }

  populateForm() {
    this.form.patchValue({
      description: this.flag.description,
      isEnabled: this.flag.isEnabled,
      rolloutPercentage: this.flag.rolloutPercentage
    });

    this.rules.clear();
    if (this.flag.targetingRules) {
      this.flag.targetingRules.forEach(rule => {
        this.rules.push(this.fb.group({
          type: [rule.type, Validators.required],
          operator: [rule.operator, Validators.required],
          value: [rule.value, Validators.required]
        }));
      });
    }
  }

  addRule() {
    this.rules.push(this.fb.group({
      type: ['UserId', Validators.required],
      operator: ['Equals', Validators.required],
      value: ['', Validators.required]
    }));
  }

  removeRule(index: number) {
    this.rules.removeAt(index);
  }

  saveFlag() {
    if (this.form.invalid || !this.projectId) return;

    // Send the core update (backend might need separate endpoint for rules currently based on CQRS tasks)
    const updateReq = {
      id: this.flag.id,
      projectId: this.projectId,
      description: this.form.value.description,
      isEnabled: this.form.value.isEnabled,
      rolloutPercentage: this.form.value.rolloutPercentage,
      targetingRules: this.form.value.rules
    };

    this.api.updateFlag(updateReq).subscribe({
      next: () => {
        this.snackBar.open('Flag updated successfully', 'Success', { duration: 3000 });
        this.router.navigate(['/feature-flags']);
      },
      error: () => this.snackBar.open('Failed to update flag', 'Close')
    });
  }
}
