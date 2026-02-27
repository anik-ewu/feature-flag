import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface ProjectDto {
    id: string;
    tenantId: string;
    name: string;
}

export interface CreateProjectRequest {
    name: string;
    tenantId: string;
}

@Injectable({ providedIn: 'root' })
export class ProjectApiService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/projects`;

    getProjectsByTenant(tenantId: string): Observable<ProjectDto[]> {
        return this.http.get<ProjectDto[]>(`${this.apiUrl}/tenant/${tenantId}`);
    }

    createProject(request: CreateProjectRequest): Observable<any> {
        return this.http.post(this.apiUrl, request);
    }
}
