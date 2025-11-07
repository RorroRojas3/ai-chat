# Notification Service Usage Instructions

## Overview

This document provides guidance on using and extending the notification service in the AI Chat application.

## Quick Start

### Using Notifications in Your Components

1. **Import and inject the service**:

```typescript
import { Component, inject } from '@angular/core';
import { NotificationService } from '@shared/services/notification.service';

@Component({
  selector: 'app-my-component',
  // ...
})
export class MyComponent {
  private notificationService = inject(NotificationService);
  
  showSuccessMessage() {
    this.notificationService.success('Operation completed!');
  }
}
```

2. **Use the appropriate method** for your notification type:
   - `success()` - For successful operations
   - `info()` - For informational messages
   - `warning()` - For warnings or cautions
   - `error()` - For errors or failures

## Where the Notification Component Lives

The `NotificationComponent` is globally available and already included in `app.component.html`. You don't need to import it in individual components - just use the service.

**Location**: `src/app/shared/components/notification/notification.component.ts`

## Service API

### Methods

```typescript
// Show notifications
notificationService.success(message: string, timeout?: number): void
notificationService.info(message: string, timeout?: number): void
notificationService.warning(message: string, timeout?: number): void
notificationService.error(message: string, timeout?: number): void

// Manage notifications
notificationService.dismiss(id: string): void
notificationService.clearAll(): void

// Configure behavior
notificationService.configure(config: Partial<NotificationConfig>): void
```

### Signals (Read-only)

```typescript
notificationService.notifications()     // Get all active notifications
notificationService.config()            // Get current configuration
notificationService.notificationCount() // Get count (computed signal)
```

## Configuration Options

Default configuration:
```typescript
{
  defaultTimeout: 5000,        // Auto-dismiss after 5 seconds
  maxNotifications: 5,         // Maximum 5 simultaneous notifications
  preventDuplicates: true,     // Prevent duplicate messages
  position: 'top'             // Display at top of screen
}
```

### Changing Configuration

```typescript
// In your component's ngOnInit or constructor
notificationService.configure({
  position: 'bottom',
  maxNotifications: 3
});
```

## Common Use Cases

### 1. Form Submission Success/Error

```typescript
onSubmit(formData: any) {
  this.apiService.save(formData).subscribe({
    next: () => {
      this.notificationService.success('Data saved successfully!');
    },
    error: (error) => {
      this.notificationService.error(`Save failed: ${error.message}`);
    }
  });
}
```

### 2. Loading State with Info Message

```typescript
loadData() {
  this.notificationService.info('Loading data...');
  
  this.dataService.fetch().subscribe({
    next: (data) => {
      this.notificationService.clearAll(); // Remove loading message
      this.notificationService.success('Data loaded!');
    }
  });
}
```

### 3. Validation Warnings

```typescript
validateInput(value: string) {
  if (value.length > 100) {
    this.notificationService.warning('Input exceeds maximum length of 100 characters');
  }
}
```

### 4. Global Error Handling in Interceptors

```typescript
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  private notificationService = inject(NotificationService);

  intercept(req: HttpRequest<any>, next: HttpHandler) {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status >= 500) {
          this.notificationService.error('Server error. Please try again later.');
        } else if (error.status === 401) {
          this.notificationService.error('Session expired. Please log in again.');
        }
        return throwError(() => error);
      })
    );
  }
}
```

## Extending the Service

### Adding New Notification Types

If you need additional notification types:

1. Update `NotificationType` in `src/app/shared/models/notification.model.ts`:
```typescript
export type NotificationType = 'success' | 'info' | 'warning' | 'danger' | 'custom';
```

2. Add a new method to `NotificationService`:
```typescript
custom(message: string, timeout?: number): void {
  this.addNotification(message, 'custom', timeout);
}
```

3. Update `NotificationComponent` to handle the new type:
```typescript
getAlertClass(type: string): string {
  if (type === 'custom') return 'alert-secondary';
  return `alert-${type}`;
}
```

### Customizing Appearance

Override styles in your global `styles.scss` or component styles:

```scss
.notification-container {
  // Change position
  top: 2rem; // More space from top
  
  .alert {
    // Customize alert appearance
    border-radius: 12px;
    box-shadow: 0 1rem 2rem rgba(0, 0, 0, 0.2);
  }
  
  // Customize specific alert types
  .alert-success {
    background-color: #custom-green;
  }
}
```

## Testing Your Code

When testing components that use the notification service:

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NotificationService } from '@shared/services/notification.service';

describe('MyComponent', () => {
  let component: MyComponent;
  let fixture: ComponentFixture<MyComponent>;
  let notificationService: NotificationService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(MyComponent);
    component = fixture.componentInstance;
    notificationService = TestBed.inject(NotificationService);
  });

  it('should show success notification', () => {
    component.performAction();
    
    expect(notificationService.notifications().length).toBe(1);
    expect(notificationService.notifications()[0].type).toBe('success');
  });
});
```

## File Structure

```
src/app/shared/
├── models/
│   └── notification.model.ts           # Types and interfaces
├── services/
│   ├── notification.service.ts         # Main service
│   └── notification.service.spec.ts    # Service tests
└── components/
    └── notification/
        ├── notification.component.ts        # Component
        ├── notification.component.html      # Template
        ├── notification.component.scss      # Styles
        └── notification.component.spec.ts   # Component tests
```

## Best Practices

1. **Use appropriate notification types** for better UX
2. **Keep messages concise** - users should be able to read them quickly
3. **Don't spam users** - use `preventDuplicates` and reasonable `maxNotifications`
4. **Set appropriate timeouts**:
   - Success/Info: 3-5 seconds
   - Warnings: 7-10 seconds
   - Errors: 0 (no auto-dismiss) or 10+ seconds
5. **Clear old notifications** before showing new ones in long operations
6. **Test notification behavior** in your component tests

## Full Documentation

For comprehensive documentation, examples, and advanced usage, see:
- **Full Documentation**: `ai-chat-ui/docs/NOTIFICATION_SERVICE.md`
- **Service Source**: `ai-chat-ui/src/app/shared/services/notification.service.ts`
- **Component Source**: `ai-chat-ui/src/app/shared/components/notification/notification.component.ts`

## Support

If you encounter issues or have questions:
1. Check the full documentation in `docs/NOTIFICATION_SERVICE.md`
2. Review the service and component source code
3. Check existing test files for usage examples
4. Create an issue in the repository if needed
