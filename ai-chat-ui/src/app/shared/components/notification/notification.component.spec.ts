import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NotificationComponent } from './notification.component';
import { NotificationService } from '../../../services/notification.service';

describe('NotificationComponent', () => {
  let component: NotificationComponent;
  let fixture: ComponentFixture<NotificationComponent>;
  let notificationService: NotificationService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NotificationComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(NotificationComponent);
    component = fixture.componentInstance;
    notificationService = TestBed.inject(NotificationService);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display notifications from service', () => {
    notificationService.success('Test success message');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const alertElements = compiled.querySelectorAll('.alert');
    
    expect(alertElements.length).toBe(1);
    expect(alertElements[0].textContent).toContain('Test success message');
  });

  it('should display multiple notifications', () => {
    notificationService.success('Message 1');
    notificationService.info('Message 2');
    notificationService.warning('Message 3');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const alertElements = compiled.querySelectorAll('.alert');
    
    expect(alertElements.length).toBe(3);
  });

  it('should apply correct alert class for success', () => {
    notificationService.success('Success message');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const alertElement = compiled.querySelector('.alert');
    
    expect(alertElement?.classList.contains('alert-success')).toBe(true);
  });

  it('should apply correct alert class for info', () => {
    notificationService.info('Info message');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const alertElement = compiled.querySelector('.alert');
    
    expect(alertElement?.classList.contains('alert-info')).toBe(true);
  });

  it('should apply correct alert class for warning', () => {
    notificationService.warning('Warning message');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const alertElement = compiled.querySelector('.alert');
    
    expect(alertElement?.classList.contains('alert-warning')).toBe(true);
  });

  it('should apply correct alert class for danger', () => {
    notificationService.error('Error message');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const alertElement = compiled.querySelector('.alert');
    
    expect(alertElement?.classList.contains('alert-danger')).toBe(true);
  });

  it('should dismiss notification when close button clicked', () => {
    notificationService.success('Test message');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const closeButton = compiled.querySelector('.btn-close') as HTMLButtonElement;
    closeButton.click();
    fixture.detectChanges();

    const alertElements = compiled.querySelectorAll('.alert');
    expect(alertElements.length).toBe(0);
  });

  it('should display correct icon for success', () => {
    notificationService.success('Success message');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const icon = compiled.querySelector('.bi');
    
    expect(icon?.classList.contains('bi-check-circle-fill')).toBe(true);
  });

  it('should display correct icon for info', () => {
    notificationService.info('Info message');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const icon = compiled.querySelector('.bi');
    
    expect(icon?.classList.contains('bi-info-circle-fill')).toBe(true);
  });

  it('should display correct icon for warning', () => {
    notificationService.warning('Warning message');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const icon = compiled.querySelector('.bi');
    
    expect(icon?.classList.contains('bi-exclamation-triangle-fill')).toBe(true);
  });

  it('should display correct icon for danger', () => {
    notificationService.error('Error message');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const icon = compiled.querySelector('.bi');
    
    expect(icon?.classList.contains('bi-x-circle-fill')).toBe(true);
  });

  it('should apply position-top class when configured', () => {
    notificationService.configure({ position: 'top' });
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const container = compiled.querySelector('.notification-container');
    
    expect(container?.classList.contains('position-top')).toBe(true);
  });

  it('should apply position-bottom class when configured', () => {
    notificationService.configure({ position: 'bottom' });
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const container = compiled.querySelector('.notification-container');
    
    expect(container?.classList.contains('position-bottom')).toBe(true);
  });

  it('should have dismissible alert with close button', () => {
    notificationService.success('Test message');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const alert = compiled.querySelector('.alert');
    const closeButton = compiled.querySelector('.btn-close');
    
    expect(alert?.classList.contains('alert-dismissible')).toBe(true);
    expect(closeButton).toBeTruthy();
  });

  it('should have proper ARIA attributes', () => {
    notificationService.success('Success message');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const alert = compiled.querySelector('.alert');
    
    expect(alert?.getAttribute('role')).toBe('alert');
    expect(alert?.getAttribute('aria-live')).toBeTruthy();
    expect(alert?.getAttribute('aria-atomic')).toBe('true');
  });

  it('should use assertive aria-live for danger notifications', () => {
    notificationService.error('Error message');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const alert = compiled.querySelector('.alert');
    
    expect(alert?.getAttribute('aria-live')).toBe('assertive');
  });

  it('should use polite aria-live for non-danger notifications', () => {
    notificationService.success('Success message');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const alert = compiled.querySelector('.alert');
    
    expect(alert?.getAttribute('aria-live')).toBe('polite');
  });
});
