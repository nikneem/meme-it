import { Routes } from '@angular/router';

import { LandingPage } from './pages/public/landing-page/landing-page';
import { NewGamePage } from '@pages/public/games/new-game/new-game';
import { JoinGamePage } from '@pages/public/games/join-game/join-game';
import { GameLobbyPage } from '@pages/private/game-lobby/game-lobby';

export const routes: Routes = [
    {
        path: '',
        component: LandingPage,
        title: 'Meme-It — Multiplayer Meme Party'
    },
    {
        path: 'games/new',
        component: NewGamePage,
        title: 'Start a New Game — Meme-It'
    },
    {
        path: 'games/join',
        component: JoinGamePage,
        title: 'Join a Game — Meme-It'
    },
    {
        path: 'games/join/:gamecode',
        component: JoinGamePage,
        title: 'Join a Game — Meme-It'
    },
    {
        path: 'app/games/:code',
        component: GameLobbyPage,
        title: 'Game Lobby — Meme-It'
    }
];
