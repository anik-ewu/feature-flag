import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

@Component({
    selector: 'app-project-create-dialog',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule
    ],
    template: `
    <h2 mat-dialog-title>Create New Project</h2>
    
    <mat-dialog-content>
      <form [formGroup]="projectForm" class="dialog-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Project Name</mat-label>
          <input matInput formControlName="name" placeholder="e.g. Acme Web App">
          <mat-error *ngIf="projectForm.get('name')?.hasError('required')">Project name is required</mat-error>
          <mat-error *ngIf="projectForm.get('name')?.hasError('maxlength')">Max 150 characters permitted</mat-error>
        </mat-form-field>
      </form>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="primary" [disabled]="projectForm.invalid" (click)="onSubmit()">Create</button>
    </mat-dialog-actions>
  `,
    styles: [`
    .dialog-form {
      display: flex;
      flex-direction: column;
      min-width: 400px;
      margin: 0;
      padding-top: 10px;
      padding-bottom: 20px;
    }
    .full-width {
      width: 100%;
    }
  `]
})
export class ProjectCreateDialogComponent {
    private fb = inject(FormBuilder);
    private dialogRef = inject(MatDialogRef<ProjectCreateDialogComponent>);

    projectForm: FormGroup = this.fb.group({
        name: ['', [Validators.required, Validators.maxLength(150)]]
    });

    onSubmit() {
        if (this.projectForm.valid) {
            this.dialogRef.close(this.projectForm.value.name);
        }
    }
}
