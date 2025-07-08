import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: 'home', loadComponent: () => import('./pages/home/home-landing-page/home-landing-page').then(m => m.HomeLandingPage) },
    { path: '', redirectTo: 'home', pathMatch: 'full' },
];
