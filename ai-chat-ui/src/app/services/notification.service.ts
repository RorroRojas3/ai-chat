import { Injectable, signal, computed } from '@angular/core';
import { Notification, NotificationConfig, NotificationType } from '../models/notification.model';

/**
 * Service for managing notifications/alerts throughout the application.
 * Uses Angular Signals for reactive state management.
 */
@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationsSignal = signal<Notification[]>([]);
  private configSignal = signal<NotificationConfig>({
    defaultTimeout: 5000,
    maxNotifications: 5,
    preventDuplicates: true,
    position: 'top'
  });

  /**
   * Read-only signal exposing the current notifications
   */
  readonly notifications = this.notificationsSignal.asReadonly();

  /**
   * Read-only signal exposing the current configuration
   */
  readonly config = this.configSignal.asReadonly();

  /**
   * Computed signal for the number of active notifications
   */
  readonly notificationCount = computed(() => this.notificationsSignal().length);

  /**
   * Updates the notification service configuration
   */
  configure(config: Partial<NotificationConfig>): void {
    this.configSignal.update(current => ({
      ...current,
      ...config
    }));
  }

  /**
   * Shows a success notification
   */
  success(message: string, timeout?: number): void {
    this.addNotification(message, 'success', timeout);
  }

  /**
   * Shows an info notification
   */
  info(message: string, timeout?: number): void {
    this.addNotification(message, 'info', timeout);
  }

  /**
   * Shows a warning notification
   */
  warning(message: string, timeout?: number): void {
    this.addNotification(message, 'warning', timeout);
  }

  /**
   * Shows an error notification
   */
  error(message: string, timeout?: number): void {
    this.addNotification(message, 'danger', timeout);
  }

  /**
   * Removes a notification by its ID
   */
  dismiss(id: string): void {
    this.notificationsSignal.update(notifications =>
      notifications.filter(n => n.id !== id)
    );
  }

  /**
   * Clears all notifications
   */
  clearAll(): void {
    this.notificationsSignal.set([]);
  }

  /**
   * Internal method to add a notification
   */
  private addNotification(message: string, type: NotificationType, timeout?: number): void {
    const config = this.configSignal();
    
    // Check for duplicates if enabled
    if (config.preventDuplicates) {
      const hasDuplicate = this.notificationsSignal().some(
        n => n.message === message && n.type === type
      );
      if (hasDuplicate) {
        return;
      }
    }

    const notification: Notification = {
      id: this.generateId(),
      message,
      type,
      timeout: timeout ?? config.defaultTimeout,
      dismissible: true,
      timestamp: new Date()
    };

    // Add the notification
    this.notificationsSignal.update(notifications => {
      const updated = [...notifications, notification];
      
      // Enforce max notifications limit
      if (config.maxNotifications > 0 && updated.length > config.maxNotifications) {
        return updated.slice(updated.length - config.maxNotifications);
      }
      
      return updated;
    });

    // Set up auto-dismiss if timeout is specified
    if (notification.timeout && notification.timeout > 0) {
      setTimeout(() => {
        this.dismiss(notification.id);
      }, notification.timeout);
    }
  }

  /**
   * Generates a unique ID for a notification
   */
  private generateId(): string {
    return `notification-${Date.now()}-${Math.random().toString(36).substring(2, 9)}`;
  }
}
