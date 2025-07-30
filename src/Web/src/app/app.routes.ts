import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: 'home', loadComponent: () => import('./pages/home/home-landing-page/home-landing-page').then(m => m.HomeLandingPage) },
    { 
        path: 'management', 
        loadComponent: () => import('./layouts/management/management-layout.component').then(m => m.ManagementLayoutComponent),
        children: [
            {
                path: 'memes', 
                loadComponent: () => import('./pages/management/memes/meme-list-page.component').then(m => m.MemeListPageComponent) 
            },
            {
                path: 'memes/create', 
                loadComponent: () => import('./pages/management/memes/create-meme-page.component').then(m => m.CreateMemePageComponent) 
            },
            { path: '', redirectTo: 'memes', pathMatch: 'full' }
        ]
    },
    { path: '', redirectTo: 'home', pathMatch: 'full' },
];
