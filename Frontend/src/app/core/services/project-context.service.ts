import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ProjectContextService {
    // Using BehaviorSubject so any late-subscribing components immediately get the current active ID
    private currentProjectIdSubject = new BehaviorSubject<string>('df78f806-5785-43b2-8355-c3a09817664a'); // Fallback to demo init

    public currentProjectId$ = this.currentProjectIdSubject.asObservable();

    setProjectId(projectId: string) {
        this.currentProjectIdSubject.next(projectId);
    }

    getProjectId(): string {
        return this.currentProjectIdSubject.value;
    }
}
