import { Component, inject } from '@angular/core';
import { NotificationService } from '../../../shared/services/notification.service';
import { CommonModule } from '@angular/common';

/**
 * Example component demonstrating the notification service usage.
 * This file serves as a reference for developers.
 * 
 * To use this component, import it and add it to a page template.
 */
@Component({
  selector: 'app-notification-demo',
  imports: [CommonModule],
  template: `
    <div class="container mt-5">
      <div class="card">
        <div class="card-header">
          <h3>Notification Service Demo</h3>
        </div>
        <div class="card-body">
          <p class="text-muted">
            Click the buttons below to test different notification types.
            Check the configuration section to customize behavior.
          </p>
          
          <div class="row g-3 mb-4">
            <div class="col-md-3">
              <button 
                class="btn btn-success w-100" 
                (click)="showSuccess()">
                <i class="bi bi-check-circle me-2"></i>
                Success
              </button>
            </div>
            <div class="col-md-3">
              <button 
                class="btn btn-info w-100" 
                (click)="showInfo()">
                <i class="bi bi-info-circle me-2"></i>
                Info
              </button>
            </div>
            <div class="col-md-3">
              <button 
                class="btn btn-warning w-100" 
                (click)="showWarning()">
                <i class="bi bi-exclamation-triangle me-2"></i>
                Warning
              </button>
            </div>
            <div class="col-md-3">
              <button 
                class="btn btn-danger w-100" 
                (click)="showError()">
                <i class="bi bi-x-circle me-2"></i>
                Error
              </button>
            </div>
          </div>

          <div class="row g-3 mb-4">
            <div class="col-md-6">
              <button 
                class="btn btn-outline-primary w-100" 
                (click)="showMultiple()">
                <i class="bi bi-stack me-2"></i>
                Show Multiple
              </button>
            </div>
            <div class="col-md-6">
              <button 
                class="btn btn-outline-secondary w-100" 
                (click)="clearAll()">
                <i class="bi bi-trash me-2"></i>
                Clear All
              </button>
            </div>
          </div>

          <hr>

          <h5>Configuration Options</h5>
          <div class="row g-3">
            <div class="col-md-6">
              <label class="form-label">Position</label>
              <select 
                class="form-select" 
                [(ngModel)]="position"
                (change)="updateConfig()">
                <option value="top">Top</option>
                <option value="bottom">Bottom</option>
              </select>
            </div>
            <div class="col-md-6">
              <label class="form-label">Max Notifications</label>
              <input 
                type="number" 
                class="form-control" 
                [(ngModel)]="maxNotifications"
                (change)="updateConfig()"
                min="1"
                max="10">
            </div>
            <div class="col-md-6">
              <label class="form-label">Default Timeout (ms)</label>
              <input 
                type="number" 
                class="form-control" 
                [(ngModel)]="defaultTimeout"
                (change)="updateConfig()"
                min="0"
                step="1000">
            </div>
            <div class="col-md-6">
              <div class="form-check mt-4">
                <input 
                  class="form-check-input" 
                  type="checkbox" 
                  id="preventDuplicates"
                  [(ngModel)]="preventDuplicates"
                  (change)="updateConfig()">
                <label class="form-check-label" for="preventDuplicates">
                  Prevent Duplicates
                </label>
              </div>
            </div>
          </div>

          <div class="alert alert-info mt-4" role="alert">
            <strong>Active Notifications:</strong> {{ notificationCount() }}
          </div>
        </div>
      </div>

      <div class="card mt-4">
        <div class="card-header">
          <h5>Code Examples</h5>
        </div>
        <div class="card-body">
          <h6>Basic Usage</h6>
          <pre class="bg-light p-3 rounded"><code>import {{ '{' }} Component, inject {{ '}' }} from '@angular/core';
import {{ '{' }} NotificationService {{ '}' }} from './shared/services/notification.service';

export class MyComponent {{ '{' }}
  private notificationService = inject(NotificationService);
  
  showSuccess() {{ '{' }}
    this.notificationService.success('Operation completed!');
  {{ '}' }}
{{ '}' }}</code></pre>

          <h6 class="mt-3">With Custom Timeout</h6>
          <pre class="bg-light p-3 rounded"><code>this.notificationService.error('Connection failed', 0); // No auto-dismiss
this.notificationService.info('Processing...', 10000); // 10 seconds</code></pre>

          <h6 class="mt-3">Configuration</h6>
          <pre class="bg-light p-3 rounded"><code>this.notificationService.configure({{ '{' }}
  position: 'bottom',
  maxNotifications: 3,
  preventDuplicates: true
{{ '}' }});</code></pre>
        </div>
      </div>
    </div>
  `,
  styles: [`
    pre {
      white-space: pre-wrap;
      word-wrap: break-word;
    }
  `]
})
export class NotificationDemoComponent {
  private notificationService = inject(NotificationService);

  // Configuration properties
  position: 'top' | 'bottom' = 'top';
  maxNotifications = 5;
  defaultTimeout = 5000;
  preventDuplicates = true;

  // Expose notification count signal
  notificationCount = this.notificationService.notificationCount;

  showSuccess() {
    this.notificationService.success('This is a success notification!');
  }

  showInfo() {
    this.notificationService.info('This is an informational notification.');
  }

  showWarning() {
    this.notificationService.warning('This is a warning notification!');
  }

  showError() {
    this.notificationService.error('This is an error notification!');
  }

  showMultiple() {
    this.notificationService.success('First notification');
    setTimeout(() => this.notificationService.info('Second notification'), 500);
    setTimeout(() => this.notificationService.warning('Third notification'), 1000);
  }

  clearAll() {
    this.notificationService.clearAll();
  }

  updateConfig() {
    this.notificationService.configure({
      position: this.position,
      maxNotifications: this.maxNotifications,
      defaultTimeout: this.defaultTimeout,
      preventDuplicates: this.preventDuplicates
    });
  }
}
