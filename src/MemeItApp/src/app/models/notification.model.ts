export type NotificationType = 'success' | 'info' | 'error';

export interface Notification {
    id: string;
    type: NotificationType;
    title: string;
    description: string;
    link?: string;
    lifetime?: number;
}

export interface NotificationConfig {
    type: NotificationType;
    title: string;
    description: string;
    link?: string;
    lifetime?: number;
}
