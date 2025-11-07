import { TestBed } from '@angular/core/testing';
import { NotificationService } from './notification.service';
import { NotificationType } from '../models/notification.model';

describe('NotificationService', () => {
  let service: NotificationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NotificationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should have default configuration', () => {
    const config = service.config();
    expect(config.defaultTimeout).toBe(5000);
    expect(config.maxNotifications).toBe(5);
    expect(config.preventDuplicates).toBe(true);
    expect(config.position).toBe('top');
  });

  it('should update configuration', () => {
    service.configure({
      defaultTimeout: 3000,
      position: 'bottom'
    });

    const config = service.config();
    expect(config.defaultTimeout).toBe(3000);
    expect(config.position).toBe('bottom');
    expect(config.maxNotifications).toBe(5);
  });

  it('should add success notification', () => {
    service.success('Test success message');
    const notifications = service.notifications();
    
    expect(notifications.length).toBe(1);
    expect(notifications[0].message).toBe('Test success message');
    expect(notifications[0].type).toBe('success');
  });

  it('should add info notification', () => {
    service.info('Test info message');
    const notifications = service.notifications();
    
    expect(notifications.length).toBe(1);
    expect(notifications[0].type).toBe('info');
  });

  it('should add warning notification', () => {
    service.warning('Test warning message');
    const notifications = service.notifications();
    
    expect(notifications.length).toBe(1);
    expect(notifications[0].type).toBe('warning');
  });

  it('should add error notification', () => {
    service.error('Test error message');
    const notifications = service.notifications();
    
    expect(notifications.length).toBe(1);
    expect(notifications[0].type).toBe('danger');
  });

  it('should dismiss notification by id', () => {
    service.success('Test message');
    const notifications = service.notifications();
    const notificationId = notifications[0].id;
    
    service.dismiss(notificationId);
    
    expect(service.notifications().length).toBe(0);
  });

  it('should clear all notifications', () => {
    service.success('Message 1');
    service.info('Message 2');
    service.warning('Message 3');
    
    expect(service.notifications().length).toBe(3);
    
    service.clearAll();
    
    expect(service.notifications().length).toBe(0);
  });

  it('should prevent duplicate notifications when configured', () => {
    service.configure({ preventDuplicates: true });
    
    service.success('Duplicate message');
    service.success('Duplicate message');
    
    expect(service.notifications().length).toBe(1);
  });

  it('should allow duplicate notifications when configured', () => {
    service.configure({ preventDuplicates: false });
    
    service.success('Duplicate message');
    service.success('Duplicate message');
    
    expect(service.notifications().length).toBe(2);
  });

  it('should enforce maximum notifications limit', () => {
    service.configure({ maxNotifications: 3 });
    
    service.success('Message 1');
    service.info('Message 2');
    service.warning('Message 3');
    service.error('Message 4');
    
    const notifications = service.notifications();
    expect(notifications.length).toBe(3);
    expect(notifications[0].message).toBe('Message 2');
    expect(notifications[2].message).toBe('Message 4');
  });

  it('should have unique IDs for each notification', () => {
    service.success('Message 1');
    service.success('Message 2');
    
    const notifications = service.notifications();
    expect(notifications[0].id).not.toBe(notifications[1].id);
  });

  it('should track notification count', () => {
    expect(service.notificationCount()).toBe(0);
    
    service.success('Message 1');
    expect(service.notificationCount()).toBe(1);
    
    service.info('Message 2');
    expect(service.notificationCount()).toBe(2);
    
    service.clearAll();
    expect(service.notificationCount()).toBe(0);
  });

  it('should set timestamp on notifications', () => {
    const before = new Date();
    service.success('Test message');
    const after = new Date();
    
    const notification = service.notifications()[0];
    expect(notification.timestamp).toBeInstanceOf(Date);
    expect(notification.timestamp.getTime()).toBeGreaterThanOrEqual(before.getTime());
    expect(notification.timestamp.getTime()).toBeLessThanOrEqual(after.getTime());
  });

  it('should mark notifications as dismissible', () => {
    service.success('Test message');
    const notification = service.notifications()[0];
    
    expect(notification.dismissible).toBe(true);
  });

  it('should use custom timeout when provided', () => {
    service.success('Test message', 10000);
    const notification = service.notifications()[0];
    
    expect(notification.timeout).toBe(10000);
  });

  it('should use default timeout when not provided', () => {
    service.configure({ defaultTimeout: 3000 });
    service.success('Test message');
    const notification = service.notifications()[0];
    
    expect(notification.timeout).toBe(3000);
  });
});
