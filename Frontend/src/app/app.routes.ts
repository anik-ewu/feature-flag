import { Routes } from '@angular/router';
import { LayoutComponent } from './shared/components/layout/layout.component';

export const routes: Routes = [
    {
        path: '',
        component: LayoutComponent,
        children: [
            {
                path: 'feature-flags',
                loadComponent: () => import('./features/feature-flags/components/flag-list/flag-list.component').then(m => m.FlagListComponent)
            },
            {
                path: 'feature-flags/edit/:id',
                loadComponent: () => import('./features/feature-flags/components/flag-detail/flag-detail.component').then(m => m.FlagDetailComponent)
            },
            { path: '', redirectTo: 'feature-flags', pathMatch: 'full' }
        ]
    },
    { path: '**', redirectTo: '' }
];
