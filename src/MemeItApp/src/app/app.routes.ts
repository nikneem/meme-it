import { Routes } from '@angular/router';

import { LandingPage } from './pages/public/landing-page/landing-page';

export const routes: Routes = [
    {
        path: '',
        component: LandingPage,
        title: 'Meme-It — Multiplayer Meme Party'
    }
];
