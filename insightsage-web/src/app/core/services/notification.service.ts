import { Injectable } from '@angular/core'
import { BehaviorSubject, Observable } from 'rxjs'

export interface Notification {
    id: string
    type: 'success' | 'error' | 'warning' | 'info'
    title?: string
    message: string
    duration?: number
    timestamp: Date
}

@Injectable({
    providedIn: 'root'
})
export class NotificationService {
    private notifications = new BehaviorSubject<Notification[]>([])
    public notifications$ = this.notifications.asObservable()

    private defaultDuration = 5000 // 5 seconds

    constructor() { }

    /**
     * Show a success notification
     */
    showSuccess(message: string, title?: string, duration?: number): void {
        this.addNotification('success', message, title, duration)
    }

    /**
     * Show an error notification
     */
    showError(message: string, title?: string, duration?: number): void {
        this.addNotification('error', message, title, duration || 8000) // Errors stay longer
    }

    /**
     * Show a warning notification
     */
    showWarning(message: string, title?: string, duration?: number): void {
        this.addNotification('warning', message, title, duration)
    }

    /**
     * Show an info notification
     */
    showInfo(message: string, title?: string, duration?: number): void {
        this.addNotification('info', message, title, duration)
    }

    /**
     * Remove a specific notification
     */
    removeNotification(id: string): void {
        const currentNotifications = this.notifications.value
        const updatedNotifications = currentNotifications.filter(n => n.id !== id)
        this.notifications.next(updatedNotifications)
    }

    /**
     * Clear all notifications
     */
    clearAll(): void {
        this.notifications.next([])
    }

    /**
     * Get current notifications
     */
    getNotifications(): Observable<Notification[]> {
        return this.notifications$
    }

    private addNotification(
        type: Notification['type'],
        message: string,
        title?: string,
        duration?: number
    ): void {
        const notification: Notification = {
            id: this.generateId(),
            type,
            title,
            message,
            duration: duration || this.defaultDuration,
            timestamp: new Date()
        }

        const currentNotifications = this.notifications.value
        this.notifications.next([...currentNotifications, notification])

        // Auto-remove notification after duration
        const notificationDuration = notification.duration || this.defaultDuration
        if (notificationDuration > 0) {
            setTimeout(() => {
                this.removeNotification(notification.id)
            }, notificationDuration)
        }
    }

    private generateId(): string {
        return Date.now().toString(36) + Math.random().toString(36).substr(2)
    }
}
