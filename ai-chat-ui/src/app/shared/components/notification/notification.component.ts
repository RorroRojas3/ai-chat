import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../services/notification.service';

/**
 * Component that displays notification alerts using Bootstrap styling.
 * Uses Angular Signals for reactive updates.
 */
@Component({
  selector: 'app-notification',
  imports: [CommonModule],
  templateUrl: './notification.component.html',
  styleUrls: ['./notification.component.scss']
})
export class NotificationComponent {
  private notificationService = inject(NotificationService);

  /**
   * Signal containing all active notifications
   */
  notifications = this.notificationService.notifications;

  /**
   * Signal containing the notification service configuration
   */
  config = this.notificationService.config;

  /**
   * Dismisses a notification by its ID
   */
  dismiss(id: string): void {
    this.notificationService.dismiss(id);
  }

  /**
   * Gets the Bootstrap alert class for a notification type
   */
  getAlertClass(type: string): string {
    return `alert-${type}`;
  }

  /**
   * Gets the Bootstrap icon for a notification type
   */
  getAlertIcon(type: string): string {
    switch (type) {
      case 'success':
        return 'bi-check-circle-fill';
      case 'info':
        return 'bi-info-circle-fill';
      case 'warning':
        return 'bi-exclamation-triangle-fill';
      case 'danger':
        return 'bi-x-circle-fill';
      default:
        return 'bi-info-circle-fill';
    }
  }
}
