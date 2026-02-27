import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface ApiKeyDto {
    id: string;
    projectId: string;
    environment: string;
    name: string;
    key: string;
    createdAt: string;
}

export interface CreateApiKeyRequest {
    environment: number; // 0=Dev, 1=Staging, 2=Prod mapped by enum
    name: string;
}

@Injectable({
    providedIn: 'root'
})
export class ApiKeysService {
    private apiUrl = `${environment.apiUrl}/projects`;

    constructor(private http: HttpClient) { }

    getApiKeys(projectId: string): Observable<ApiKeyDto[]> {
        return this.http.get<ApiKeyDto[]>(`${this.apiUrl}/${projectId}/apikeys`);
    }

    createApiKey(projectId: string, request: CreateApiKeyRequest): Observable<ApiKeyDto> {
        return this.http.post<ApiKeyDto>(`${this.apiUrl}/${projectId}/apikeys`, request);
    }

    revokeApiKey(projectId: string, id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${projectId}/apikeys/${id}`);
    }
}
