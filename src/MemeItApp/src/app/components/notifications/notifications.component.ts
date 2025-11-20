import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';
import { Observable } from 'rxjs';
import { NotificationService } from '../../services/notification.service';
import { Notification } from '../../models/notification.model';

@Component({
    selector: 'memeit-notifications',
    imports: [CommonModule],
    templateUrl: './notifications.component.html',
    styleUrl: './notifications.component.scss',
    animations: [
        trigger('slideIn', [
            transition(':enter', [
                style({ height: '0', opacity: '0', transform: 'translateY(-20px)' }),
                animate('300ms ease-out', style({ height: '*', opacity: '1', transform: 'translateY(0)' }))
            ]),
            transition(':leave', [
                animate('200ms ease-in', style({ height: '0', opacity: '0', transform: 'translateY(-20px)' }))
            ])
        ])
    ]
})
export class NotificationsComponent implements OnInit {
    notifications$!: Observable<Notification[]>;

    constructor(private notificationService: NotificationService) { }

    ngOnInit(): void {
        this.notifications$ = this.notificationService.getNotifications();
    }

    getIcon(type: string): string {
        switch (type) {
            case 'success':
                return 'check_circle';
            case 'error':
                return 'error';
            case 'info':
            default:
                return 'info';
        }
    }

    onClose(id: string): void {
        this.notificationService.remove(id);
    }
}
