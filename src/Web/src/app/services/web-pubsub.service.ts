import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';

export interface WebSocketConnectionStatus {
  isConnected: boolean;
  isConnecting: boolean;
  error?: string;
}

export interface GameUpdateMessage {
  type: string;
  data: any;
  gameCode: string;
  timestamp: string;
}

export const GameUpdateMessageTypes = {
  GAME_UPDATED: 'GameUpdated',
  PLAYER_JOINED: 'PlayerJoined',
  PLAYER_LEFT: 'PlayerLeft',
  PLAYER_READY_STATUS_CHANGED: 'PlayerReadyStatusChanged',
  PLAYER_KICKED: 'PlayerKicked',
  GAME_STARTED: 'GameStarted',
  GAME_SETTINGS_UPDATED: 'GameSettingsUpdated'
} as const;

@Injectable({
  providedIn: 'root'
})
export class WebPubSubService {
  private websocket: WebSocket | null = null;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;
  private reconnectDelay = 1000; // Start with 1 second

  private connectionStatusSubject = new BehaviorSubject<WebSocketConnectionStatus>({
    isConnected: false,
    isConnecting: false
  });

  private messageSubject = new Subject<GameUpdateMessage>();

  public connectionStatus$ = this.connectionStatusSubject.asObservable();
  public messages$ = this.messageSubject.asObservable();

  connect(connectionUrl: string, gameCode: string): Promise<void> {
    return new Promise((resolve, reject) => {
      if (this.websocket?.readyState === WebSocket.OPEN) {
        resolve();
        return;
      }

      this.updateConnectionStatus({ isConnected: false, isConnecting: true });

      try {
        this.websocket = new WebSocket(connectionUrl, 'json.webpubsub.azure.v1');

        this.websocket.onopen = () => {
          console.log('WebSocket connected to Azure Web PubSub');
          this.reconnectAttempts = 0;
          this.updateConnectionStatus({ isConnected: true, isConnecting: false });
          
          // Join the game group
          this.joinGroup(`game-${gameCode}`);
          resolve();
        };

        this.websocket.onmessage = (event) => {
          try {
            const message = JSON.parse(event.data);
            this.handleMessage(message);
          } catch (error) {
            console.error('Error parsing WebSocket message:', error);
          }
        };

        this.websocket.onclose = (event) => {
          console.log('WebSocket connection closed:', event.code, event.reason);
          this.updateConnectionStatus({ 
            isConnected: false, 
            isConnecting: false,
            error: event.reason || 'Connection closed'
          });
          
          // Attempt to reconnect if not manually closed
          if (event.code !== 1000 && this.reconnectAttempts < this.maxReconnectAttempts) {
            this.attemptReconnect(connectionUrl, gameCode);
          }
        };

        this.websocket.onerror = (error) => {
          console.error('WebSocket error:', error);
          this.updateConnectionStatus({ 
            isConnected: false, 
            isConnecting: false,
            error: 'Connection error'
          });
          reject(error);
        };

      } catch (error) {
        this.updateConnectionStatus({ 
          isConnected: false, 
          isConnecting: false,
          error: 'Failed to create WebSocket connection'
        });
        reject(error);
      }
    });
  }

  private attemptReconnect(connectionUrl: string, gameCode: string): void {
    this.reconnectAttempts++;
    const delay = this.reconnectDelay * Math.pow(2, this.reconnectAttempts - 1); // Exponential backoff
    
    console.log(`Attempting to reconnect in ${delay}ms (attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts})`);
    
    setTimeout(() => {
      this.connect(connectionUrl, gameCode).catch(() => {
        // Reconnection failed, will try again if attempts remain
      });
    }, delay);
  }

  private handleMessage(message: any): void {
    console.log('Received WebSocket message:', message);

    // Handle different Azure Web PubSub message types
    if (message.type === 'message' && message.data) {
      // This is a user message (our game update)
      try {
        const gameUpdate: GameUpdateMessage = typeof message.data === 'string' 
          ? JSON.parse(message.data) 
          : message.data;
        
        this.messageSubject.next(gameUpdate);
      } catch (error) {
        console.error('Error parsing game update message:', error);
      }
    } else if (message.type === 'system') {
      // Handle system messages (connected, disconnected, etc.)
      console.log('System message:', message);
    }
  }

  private joinGroup(groupName: string): void {
    if (this.websocket?.readyState === WebSocket.OPEN) {
      const joinMessage = {
        type: 'joinGroup',
        group: groupName
      };
      this.websocket.send(JSON.stringify(joinMessage));
      console.log(`Joined group: ${groupName}`);
    }
  }

  disconnect(): void {
    if (this.websocket) {
      this.websocket.close(1000, 'User disconnected');
      this.websocket = null;
    }
    this.updateConnectionStatus({ isConnected: false, isConnecting: false });
  }

  private updateConnectionStatus(status: WebSocketConnectionStatus): void {
    this.connectionStatusSubject.next(status);
  }

  // Send message to the group (if needed for future features)
  sendToGroup(groupName: string, message: any): void {
    if (this.websocket?.readyState === WebSocket.OPEN) {
      const sendMessage = {
        type: 'sendToGroup',
        group: groupName,
        data: JSON.stringify(message)
      };
      this.websocket.send(JSON.stringify(sendMessage));
    } else {
      console.error('WebSocket is not connected');
    }
  }

  isConnected(): boolean {
    return this.websocket?.readyState === WebSocket.OPEN;
  }
}
