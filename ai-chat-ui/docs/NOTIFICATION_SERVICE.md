# Notification Service Documentation

## Overview

The notification service provides a centralized way to display Bootstrap-styled alert notifications throughout the Angular 19 application. It uses Angular Signals for reactive state management and supports success, info, warning, and danger/error notifications.

## Features

- ✅ **Signal-based State Management**: Uses Angular 19 signals for reactive updates
- ✅ **Bootstrap 5.3.8 Styling**: Uses only Bootstrap alert classes (no external UI libraries)
- ✅ **Multiple Notification Types**: success, info, warning, danger/error
- ✅ **Auto-dismiss**: Configurable timeout for automatic dismissal
- ✅ **Manual Dismiss**: Close button for user-initiated dismissal
- ✅ **Duplicate Prevention**: Optional prevention of duplicate notifications
- ✅ **Maximum Limit**: Configurable maximum number of simultaneous alerts
- ✅ **Positioning**: Top or bottom positioning
- ✅ **Accessibility**: ARIA attributes for screen readers
- ✅ **Icons**: Bootstrap Icons for visual indicators

## Architecture

### Files Structure

```
src/app/
├── models/
│   └── notification.model.ts          # Type definitions and interfaces
├── services/
│   ├── notification.service.ts        # Core notification service
│   └── notification.service.spec.ts   # Service unit tests
└── shared/
    └── components/
        └── notification/
            ├── notification.component.ts       # Display component
            ├── notification.component.html     # Template
            ├── notification.component.scss     # Styles
            └── notification.component.spec.ts  # Component unit tests
```

### Models

#### `NotificationType`
```typescript
type NotificationType = 'success' | 'info' | 'warning' | 'danger';
```

#### `Notification`
```typescript
interface Notification {
  id: string;              // Unique identifier
  message: string;         // Message content
  type: NotificationType;  // Alert type
  timeout?: number;        // Auto-dismiss timeout (ms)
  dismissible: boolean;    // Can be manually dismissed
  timestamp: Date;         // Creation timestamp
}
```

#### `NotificationConfig`
```typescript
interface NotificationConfig {
  defaultTimeout: number;      // Default auto-dismiss timeout (ms)
  maxNotifications: number;    // Max simultaneous notifications (0 = unlimited)
  preventDuplicates: boolean;  // Prevent duplicate messages
  position: 'top' | 'bottom';  // Container position
}
```

## Usage

### Basic Usage

#### 1. Import the Service

The service is provided at root level, so you can inject it anywhere:

```typescript
import { Component, inject } from '@angular/core';
import { NotificationService } from './services/notification.service';

@Component({
  selector: 'app-my-component',
  // ...
})
export class MyComponent {
  private notificationService = inject(NotificationService);

  someMethod() {
    this.notificationService.success('Operation completed successfully!');
  }
}
```

#### 2. Display Notifications

The `NotificationComponent` is already added to `app.component.html`, so notifications will appear automatically when triggered.

### API Methods

#### Success Notification
```typescript
notificationService.success(message: string, timeout?: number): void

// Examples
notificationService.success('User saved successfully!');
notificationService.success('File uploaded!', 3000); // Custom 3s timeout
```

#### Info Notification
```typescript
notificationService.info(message: string, timeout?: number): void

// Examples
notificationService.info('Processing your request...');
notificationService.info('New features available', 7000);
```

#### Warning Notification
```typescript
notificationService.warning(message: string, timeout?: number): void

// Examples
notificationService.warning('Your session will expire soon');
notificationService.warning('Unsaved changes detected', 10000);
```

#### Error Notification
```typescript
notificationService.error(message: string, timeout?: number): void

// Examples
notificationService.error('Failed to save user');
notificationService.error('Network connection lost', 0); // No auto-dismiss
```

#### Dismiss Notification
```typescript
notificationService.dismiss(id: string): void

// Example
const notifications = notificationService.notifications();
notificationService.dismiss(notifications[0].id);
```

#### Clear All Notifications
```typescript
notificationService.clearAll(): void

// Example
notificationService.clearAll();
```

### Configuration

#### Update Configuration
```typescript
notificationService.configure(config: Partial<NotificationConfig>): void

// Examples
notificationService.configure({
  defaultTimeout: 3000,
  position: 'bottom'
});

notificationService.configure({
  maxNotifications: 3,
  preventDuplicates: false
});
```

#### Default Configuration
```typescript
{
  defaultTimeout: 5000,        // 5 seconds
  maxNotifications: 5,         // Max 5 notifications
  preventDuplicates: true,     // Prevent duplicates
  position: 'top'             // Top of screen
}
```

### Signals

The service exposes read-only signals for reactive programming:

```typescript
// Access current notifications
const notifications = notificationService.notifications();

// Access current configuration
const config = notificationService.config();

// Get notification count (computed signal)
const count = notificationService.notificationCount();

// Use in effects
effect(() => {
  console.log('Active notifications:', notificationService.notificationCount());
});
```

## Examples

### Example 1: Form Submission

```typescript
@Component({
  selector: 'app-user-form',
  // ...
})
export class UserFormComponent {
  private notificationService = inject(NotificationService);
  private userService = inject(UserService);

  onSubmit(userData: User) {
    this.userService.save(userData).subscribe({
      next: () => {
        this.notificationService.success('User saved successfully!');
      },
      error: (error) => {
        this.notificationService.error(`Failed to save user: ${error.message}`);
      }
    });
  }
}
```

### Example 2: File Upload

```typescript
@Component({
  selector: 'app-file-upload',
  // ...
})
export class FileUploadComponent {
  private notificationService = inject(NotificationService);

  onFileSelected(file: File) {
    if (file.size > 10 * 1024 * 1024) {
      this.notificationService.warning('File size exceeds 10MB limit');
      return;
    }

    this.notificationService.info('Uploading file...');
    this.uploadFile(file).subscribe({
      next: () => {
        this.notificationService.success('File uploaded successfully!');
      },
      error: () => {
        this.notificationService.error('File upload failed');
      }
    });
  }
}
```

### Example 3: Session Management

```typescript
@Component({
  selector: 'app-session',
  // ...
})
export class SessionComponent implements OnInit {
  private notificationService = inject(NotificationService);

  ngOnInit() {
    // Configure for this component's needs
    this.notificationService.configure({
      position: 'bottom',
      maxNotifications: 3
    });

    // Warn about session expiry
    this.checkSessionExpiry();
  }

  checkSessionExpiry() {
    const expiresIn = this.getSessionExpiryTime();
    if (expiresIn < 5 * 60 * 1000) { // Less than 5 minutes
      this.notificationService.warning(
        'Your session will expire in 5 minutes',
        0 // Don't auto-dismiss
      );
    }
  }
}
```

### Example 4: API Error Handling

```typescript
@Injectable({
  providedIn: 'root'
})
export class ApiInterceptor implements HttpInterceptor {
  private notificationService = inject(NotificationService);

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          this.notificationService.error('Session expired. Please log in again.');
        } else if (error.status === 500) {
          this.notificationService.error('Server error. Please try again later.');
        } else if (error.status === 0) {
          this.notificationService.error('Network error. Check your connection.');
        }
        return throwError(() => error);
      })
    );
  }
}
```

### Example 5: Multiple Notifications

```typescript
@Component({
  selector: 'app-batch-processor',
  // ...
})
export class BatchProcessorComponent {
  private notificationService = inject(NotificationService);

  processBatch(items: any[]) {
    let successCount = 0;
    let errorCount = 0;

    items.forEach(item => {
      this.processItem(item).subscribe({
        next: () => successCount++,
        error: () => errorCount++,
        complete: () => {
          if (successCount + errorCount === items.length) {
            if (errorCount === 0) {
              this.notificationService.success(
                `All ${successCount} items processed successfully!`
              );
            } else {
              this.notificationService.warning(
                `Processed ${successCount} items, ${errorCount} failed`
              );
            }
          }
        }
      });
    });
  }
}
```

## Styling and Customization

### Bootstrap Alert Classes

The component uses standard Bootstrap alert classes:
- `alert-success` - Green (success operations)
- `alert-info` - Blue (informational messages)
- `alert-warning` - Yellow (warnings)
- `alert-danger` - Red (errors)

### Custom Styling

You can customize the notification appearance by overriding CSS classes in your component or global styles:

```scss
// Custom notification container styling
.notification-container {
  max-width: 800px; // Wider notifications
  
  .alert {
    border-radius: 8px; // More rounded corners
    font-size: 1.1rem; // Larger text
  }
}
```

### Animation

Notifications use a slide-in animation defined in `notification.component.scss`. You can customize this:

```scss
@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateY(-2rem); // Slide from further up
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
```

## Accessibility

The notification component follows accessibility best practices:

- **ARIA Attributes**: Each notification has `role="alert"`, `aria-live`, and `aria-atomic` attributes
- **Screen Reader Support**: Danger notifications use `aria-live="assertive"` for immediate announcement
- **Keyboard Navigation**: Close buttons are keyboard accessible
- **Semantic HTML**: Uses proper Bootstrap alert structure
- **Icon Labels**: Icons provide visual context matching the alert type

## Testing

### Testing the Service

```typescript
import { TestBed } from '@angular/core/testing';
import { NotificationService } from './notification.service';

describe('NotificationService', () => {
  let service: NotificationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NotificationService);
  });

  it('should add success notification', () => {
    service.success('Test message');
    expect(service.notifications().length).toBe(1);
    expect(service.notifications()[0].type).toBe('success');
  });
});
```

### Testing Components Using the Service

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NotificationService } from './services/notification.service';
import { MyComponent } from './my.component';

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

  it('should show success notification on save', () => {
    component.save();
    expect(notificationService.notifications().length).toBe(1);
    expect(notificationService.notifications()[0].type).toBe('success');
  });
});
```

## Best Practices

1. **Use Appropriate Types**: Match notification type to the message severity
   - Success: Completed actions
   - Info: General information, progress updates
   - Warning: Non-critical issues, confirmations needed
   - Error: Failed operations, critical issues

2. **Keep Messages Concise**: Brief, clear messages are more effective
   ```typescript
   // Good
   notificationService.success('User saved');
   
   // Too verbose
   notificationService.success('The user has been successfully saved to the database');
   ```

3. **Use Timeouts Wisely**:
   - Success/Info: 3-5 seconds
   - Warnings: 7-10 seconds
   - Errors: 0 (no auto-dismiss) or 10+ seconds

4. **Don't Spam Users**: Use `preventDuplicates` and `maxNotifications` to avoid overwhelming users

5. **Provide Context in Errors**: Include helpful information
   ```typescript
   notificationService.error(`Failed to save user: ${error.message}`);
   ```

6. **Clear Notifications When Appropriate**: Clear old notifications when navigating or starting new operations
   ```typescript
   // Before starting a new process
   notificationService.clearAll();
   ```

## Troubleshooting

### Notifications Not Appearing

1. Verify `NotificationComponent` is in `app.component.html`
2. Check browser console for errors
3. Ensure Bootstrap CSS is loaded

### Notifications Don't Auto-dismiss

1. Check if timeout is set to 0 (disables auto-dismiss)
2. Verify timeout value is reasonable (> 0)
3. Check browser console for JavaScript errors

### Duplicate Notifications Appearing

1. Verify `preventDuplicates` configuration
2. Check if the same message is being triggered multiple times
3. Consider using `clearAll()` before showing new notifications

### Styling Issues

1. Ensure Bootstrap 5.3.8 CSS is loaded
2. Check for CSS conflicts in global styles
3. Verify component styles are not being overridden

## Future Enhancements

Potential improvements for future versions:

- [ ] Progress bar showing time until auto-dismiss
- [ ] Sound notifications (with user preference)
- [ ] Notification history/log
- [ ] Rich content support (HTML in messages)
- [ ] Action buttons in notifications
- [ ] Notification grouping/stacking strategies
- [ ] Persistent notifications (survive page refresh)
- [ ] Notification priority levels
- [ ] Custom icons support
- [ ] Animation customization options

## Support

For issues, questions, or contributions related to the notification service, please refer to the main repository documentation or create an issue in the project's issue tracker.
