import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Subject, Observable } from 'rxjs';

export interface PlayerJoinedEvent {
    playerId: string;
    displayName: string;
    gameCode: string;
}

export interface PlayerStateChangedEvent {
    playerId: string;
    displayName: string;
    isReady: boolean;
}

export interface PlayerRemovedEvent {
    playerId: string;
    displayName: string;
}

@Injectable({
    providedIn: 'root'
})
export class RealtimeService {
    private hubConnection?: HubConnection;
    private readonly hubUrl = 'http://localhost:5000/hubs/games';

    private playerJoinedSubject = new Subject<PlayerJoinedEvent>();
    private playerStateChangedSubject = new Subject<PlayerStateChangedEvent>();
    private playerRemovedSubject = new Subject<PlayerRemovedEvent>();

    public playerJoined$: Observable<PlayerJoinedEvent> = this.playerJoinedSubject.asObservable();
    public playerStateChanged$: Observable<PlayerStateChangedEvent> = this.playerStateChangedSubject.asObservable();
    public playerRemoved$: Observable<PlayerRemovedEvent> = this.playerRemovedSubject.asObservable();

    constructor() { }

    /**
     * Connects to the SignalR hub
     */
    async connect(): Promise<void> {
        if (this.hubConnection?.state === 'Connected') {
            console.log('Already connected to SignalR hub');
            return;
        }

        this.hubConnection = new HubConnectionBuilder()
            .withUrl(this.hubUrl)
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        this.setupEventHandlers();

        try {
            await this.hubConnection.start();
            console.log('Connected to SignalR hub');
        } catch (error) {
            console.error('Error connecting to SignalR hub:', error);
            throw error;
        }
    }

    /**
     * Disconnects from the SignalR hub
     */
    async disconnect(): Promise<void> {
        if (this.hubConnection) {
            try {
                await this.hubConnection.stop();
                console.log('Disconnected from SignalR hub');
            } catch (error) {
                console.error('Error disconnecting from SignalR hub:', error);
            }
        }
    }

    /**
     * Joins a specific game group to receive updates for that game
     */
    async joinGameGroup(gameCode: string): Promise<void> {
        if (!this.hubConnection || this.hubConnection.state !== 'Connected') {
            throw new Error('Not connected to SignalR hub');
        }

        try {
            await this.hubConnection.invoke('JoinGameGroup', gameCode);
            console.log(`Joined game group: ${gameCode}`);
        } catch (error) {
            console.error(`Error joining game group ${gameCode}:`, error);
            throw error;
        }
    }

    /**
     * Leaves a specific game group
     */
    async leaveGameGroup(gameCode: string): Promise<void> {
        if (!this.hubConnection || this.hubConnection.state !== 'Connected') {
            return;
        }

        try {
            await this.hubConnection.invoke('LeaveGameGroup', gameCode);
            console.log(`Left game group: ${gameCode}`);
        } catch (error) {
            console.error(`Error leaving game group ${gameCode}:`, error);
        }
    }

    /**
     * Sets up event handlers for SignalR messages
     */
    private setupEventHandlers(): void {
        if (!this.hubConnection) return;

        this.hubConnection.on('PlayerJoined', (event: PlayerJoinedEvent) => {
            console.log('PlayerJoined event received:', event);
            this.playerJoinedSubject.next(event);
        });

        this.hubConnection.on('PlayerStateChanged', (event: PlayerStateChangedEvent) => {
            console.log('PlayerStateChanged event received:', event);
            this.playerStateChangedSubject.next(event);
        });

        this.hubConnection.on('PlayerRemoved', (event: PlayerRemovedEvent) => {
            console.log('PlayerRemoved event received:', event);
            this.playerRemovedSubject.next(event);
        });

        this.hubConnection.onreconnecting((error) => {
            console.warn('SignalR reconnecting...', error);
        });

        this.hubConnection.onreconnected((connectionId) => {
            console.log('SignalR reconnected. Connection ID:', connectionId);
        });

        this.hubConnection.onclose((error) => {
            console.error('SignalR connection closed', error);
        });
    }

    /**
     * Gets the current connection state
     */
    get connectionState(): string {
        return this.hubConnection?.state || 'Disconnected';
    }

    /**
     * Checks if currently connected
     */
    get isConnected(): boolean {
        return this.hubConnection?.state === 'Connected';
    }
}
