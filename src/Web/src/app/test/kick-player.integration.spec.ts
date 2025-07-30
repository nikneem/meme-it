import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Store } from '@ngrx/store';
import { GameLobbyComponent } from '../../../src/app/pages/game/game-lobby.component';
import { kickPlayer } from '../../../src/app/store/game/game.actions';

// Simple test to verify kick player action is properly exported and can be dispatched
describe('KickPlayer Integration', () => {
  it('should export kickPlayer action', () => {
    expect(kickPlayer).toBeDefined();
    expect(typeof kickPlayer).toBe('function');
    
    const action = kickPlayer({ targetPlayerId: 'test-player-id' });
    expect(action.type).toBe('[Game] Kick Player');
    expect(action.targetPlayerId).toBe('test-player-id');
  });
});
