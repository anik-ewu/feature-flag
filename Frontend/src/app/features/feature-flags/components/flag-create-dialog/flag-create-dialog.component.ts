import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { CreateFeatureFlagRequest } from '../../../../core/models/feature-flag.model';

@Component({
    selector: 'app-flag-create-dialog',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatButtonModule
    ],
    template: `
    <h2 mat-dialog-title>Create New Feature Flag</h2>
    
    <mat-dialog-content>
      <form [formGroup]="flagForm" class="dialog-form">
        
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Flag Key</mat-label>
          <input matInput formControlName="key" placeholder="e.g. new-dashboard-ui">
          <mat-hint>Must be unique, preferably lower-kebab-case</mat-hint>
          <mat-error *ngIf="flagForm.get('key')?.hasError('required')">Key is required</mat-error>
          <mat-error *ngIf="flagForm.get('key')?.hasError('pattern')">Avoid spaces, use hyphens</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea matInput formControlName="description" rows="3"></textarea>
          <mat-error *ngIf="flagForm.get('description')?.hasError('maxlength')">Max 500 characters</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Environment</mat-label>
          <mat-select formControlName="environment">
            <mat-option [value]="0">Development</mat-option>
            <mat-option [value]="1">Staging</mat-option>
            <mat-option [value]="2">Production</mat-option>
          </mat-select>
        </mat-form-field>

      </form>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="primary" [disabled]="flagForm.invalid" (click)="onSubmit()">Create</button>
    </mat-dialog-actions>
  `,
    styles: [`
    .dialog-form {
      display: flex;
      flex-direction: column;
      gap: 15px;
      min-width: 400px;
      padding-top: 10px;
    }
    .full-width {
      width: 100%;
    }
  `]
})
export class FlagCreateDialogComponent {
    private fb = inject(FormBuilder);
    private dialogRef = inject(MatDialogRef<FlagCreateDialogComponent>);

    flagForm: FormGroup = this.fb.group({
        key: ['', [Validators.required, Validators.pattern(/^[a-z0-9-]+$/)]],
        description: ['', [Validators.maxLength(500)]],
        environment: [0, Validators.required]
    });

    onSubmit() {
        if (this.flagForm.valid) {
            const payload: Partial<CreateFeatureFlagRequest> = {
                key: this.flagForm.value.key,
                description: this.flagForm.value.description,
                environment: this.flagForm.value.environment
            };
            // Let the parent component handle the actual HTTP request and ProjectId
            this.dialogRef.close(payload);
        }
    }
}
