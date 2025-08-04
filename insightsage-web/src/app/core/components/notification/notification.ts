import { Component, OnInit, OnDestroy } from '@angular/core'
import { CommonModule } from '@angular/common'
import { Subscription } from 'rxjs'
import { NotificationService, Notification } from '../../services/notification.service'

@Component({
    selector: 'app-notification',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './notification.html',
    styleUrl: './notification.scss'
})
export class NotificationComponent implements OnInit, OnDestroy {
    notifications: Notification[] = []
    private subscription: Subscription = new Subscription()

    constructor(private notificationService: NotificationService) { }

    ngOnInit(): void {
        this.subscription = this.notificationService.getNotifications().subscribe(
            notifications => {
                this.notifications = notifications
            }
        )
    }

    ngOnDestroy(): void {
        this.subscription.unsubscribe()
    }

    removeNotification(id: string): void {
        this.notificationService.removeNotification(id)
    }

    trackByNotificationId(index: number, notification: Notification): string {
        return notification.id
    }

    getIconClass(type: string): string {
        switch (type) {
            case 'success':
                return 'text-green-500'
            case 'error':
                return 'text-red-500'
            case 'warning':
                return 'text-yellow-500'
            case 'info':
                return 'text-blue-500'
            default:
                return 'text-gray-500'
        }
    }

    getBackgroundClass(type: string): string {
        switch (type) {
            case 'success':
                return 'bg-green-50 border-green-200'
            case 'error':
                return 'bg-red-50 border-red-200'
            case 'warning':
                return 'bg-yellow-50 border-yellow-200'
            case 'info':
                return 'bg-blue-50 border-blue-200'
            default:
                return 'bg-gray-50 border-gray-200'
        }
    }
}
