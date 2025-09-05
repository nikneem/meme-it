import { Routes } from '@angular/router';
import { GameLobbyGuard } from './guards/game-lobby.guard';
import { GameActiveGuard } from './guards/game-active.guard';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () => import('./layouts/public/public-layout.component').then(m => m.PublicLayoutComponent),
        children: [
            { 
                path: 'home', 
                loadComponent: () => import('./pages/home/home-landing-page/home-landing-page').then(m => m.HomeLandingPage) 
            },
            {
                path: 'game',
                loadComponent: () => import('./pages/game/game-join.component').then(m => m.GameJoinComponent)
            },
            {
                path: 'game/:gameCode/lobby',
                loadComponent: () => import('./pages/game/game-lobby.component').then(m => m.GameLobbyComponent),
                canActivate: [GameLobbyGuard]
            },
            {
                path: 'game/:gameCode/active',
                loadComponent: () => import('./pages/game/game-active.component').then(m => m.GameActiveComponent),
                canActivate: [GameActiveGuard]
            },
            // Add more public routes here (game pages, about, etc.)
            { path: '', redirectTo: 'home', pathMatch: 'full' }
        ]
    },
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
    }
];
