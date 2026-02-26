import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { FeatureFlag, CreateFeatureFlagRequest, UpdateFeatureFlagRequest } from '../models/feature-flag.model';

@Injectable({
    providedIn: 'root'
})
export class FeatureFlagApiService {
    private http = inject(HttpClient);
    private baseUrl = `${environment.apiUrl}/featureflags`;

    getFlagsByProject(projectId: string): Observable<FeatureFlag[]> {
        return this.http.get<FeatureFlag[]>(`${this.baseUrl}/project/${projectId}`);
    }

    getFlagById(id: string, projectId: string): Observable<FeatureFlag> {
        return this.http.get<FeatureFlag>(`${this.baseUrl}/${id}?projectId=${projectId}`);
    }

    createFlag(request: CreateFeatureFlagRequest): Observable<string> {
        return this.http.post<string>(this.baseUrl, request);
    }

    updateFlag(request: UpdateFeatureFlagRequest): Observable<void> {
        return this.http.put<void>(`${this.baseUrl}/${request.id}`, request);
    }

    deleteFlag(id: string, projectId: string): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}?projectId=${projectId}`);
    }
}
