import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: 'home', loadComponent: () => import('./pages/home/home-landing-page/home-landing-page').then(m => m.HomeLandingPage) },
    { 
        path: 'management/memes', 
        loadComponent: () => import('./pages/management/memes/meme-list-page.component').then(m => m.MemeListPageComponent) 
    },
    { 
        path: 'management/memes/create', 
        loadComponent: () => import('./pages/management/memes/create-meme-page.component').then(m => m.CreateMemePageComponent) 
    },
    { path: '', redirectTo: 'home', pathMatch: 'full' },
];
