import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Notification, NotificationConfig } from '../models/notification.model';

@Injectable({
    providedIn: 'root'
})
export class NotificationService {
    private notifications$ = new BehaviorSubject<Notification[]>([]);
    private idCounter = 0;

    getNotifications(): Observable<Notification[]> {
        return this.notifications$.asObservable();
    }

    show(config: NotificationConfig): void {
        const notification: Notification = {
            id: `notification-${++this.idCounter}-${Date.now()}`,
            type: config.type,
            title: config.title,
            description: config.description,
            link: config.link,
            lifetime: config.lifetime ?? 5000
        };

        const currentNotifications = this.notifications$.value;
        this.notifications$.next([notification, ...currentNotifications]);

        // Auto-remove after lifetime
        setTimeout(() => {
            this.remove(notification.id);
        }, notification.lifetime);
    }

    success(title: string, description: string, link?: string, lifetime?: number): void {
        this.show({ type: 'success', title, description, link, lifetime });
    }

    info(title: string, description: string, link?: string, lifetime?: number): void {
        this.show({ type: 'info', title, description, link, lifetime });
    }

    error(title: string, description: string, link?: string, lifetime?: number): void {
        this.show({ type: 'error', title, description, link, lifetime });
    }

    remove(id: string): void {
        const currentNotifications = this.notifications$.value;
        this.notifications$.next(currentNotifications.filter(n => n.id !== id));
    }

    clear(): void {
        this.notifications$.next([]);
    }
}
