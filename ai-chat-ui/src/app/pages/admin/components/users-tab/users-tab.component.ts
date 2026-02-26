import { Component, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface AdminUser {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  status: string;
  lastLogin: string;
}

@Component({
  selector: 'app-users-tab',
  imports: [FormsModule],
  templateUrl: './users-tab.component.html',
  styleUrl: './users-tab.component.scss',
})
export class UsersTabComponent {
  searchTerm = signal('');
  roleFilter = signal('');
  statusFilter = signal('');
  selectedUserIds = signal<number[]>([]);

  readonly users = signal<AdminUser[]>([
    { id: 1, firstName: 'Rodrigo', lastName: 'Rojas', email: 'rodrigo.rojas@enterprise.ai', role: 'Admin', status: 'Active', lastLogin: '2026-02-25 09:14' },
    { id: 2, firstName: 'Elena', lastName: 'Martinez', email: 'elena.martinez@enterprise.ai', role: 'Admin', status: 'Active', lastLogin: '2026-02-25 08:42' },
    { id: 3, firstName: 'James', lastName: 'Wilson', email: 'james.wilson@enterprise.ai', role: 'User', status: 'Active', lastLogin: '2026-02-24 17:30' },
    { id: 4, firstName: 'Sarah', lastName: 'Chen', email: 'sarah.chen@enterprise.ai', role: 'User', status: 'Active', lastLogin: '2026-02-24 15:10' },
    { id: 5, firstName: 'Michael', lastName: 'Brown', email: 'michael.brown@enterprise.ai', role: 'User', status: 'Inactive', lastLogin: '2026-01-15 11:22' },
    { id: 6, firstName: 'Lisa', lastName: 'Anderson', email: 'lisa.anderson@enterprise.ai', role: 'Viewer', status: 'Active', lastLogin: '2026-02-23 14:05' },
    { id: 7, firstName: 'David', lastName: 'Kim', email: 'david.kim@enterprise.ai', role: 'User', status: 'Active', lastLogin: '2026-02-25 07:58' },
    { id: 8, firstName: 'Anna', lastName: 'Petrov', email: 'anna.petrov@enterprise.ai', role: 'User', status: 'Active', lastLogin: '2026-02-22 16:45' },
    { id: 9, firstName: 'Robert', lastName: 'Taylor', email: 'robert.taylor@enterprise.ai', role: 'Viewer', status: 'Inactive', lastLogin: '2025-12-20 09:00' },
    { id: 10, firstName: 'Maria', lastName: 'Garcia', email: 'maria.garcia@enterprise.ai', role: 'User', status: 'Active', lastLogin: '2026-02-24 12:33' },
    { id: 11, firstName: 'Thomas', lastName: 'Lee', email: 'thomas.lee@enterprise.ai', role: 'Admin', status: 'Active', lastLogin: '2026-02-25 10:02' },
    { id: 12, firstName: 'Jennifer', lastName: 'Davis', email: 'jennifer.davis@enterprise.ai', role: 'User', status: 'Active', lastLogin: '2026-02-21 18:20' },
    { id: 13, firstName: 'Carlos', lastName: 'Rivera', email: 'carlos.rivera@enterprise.ai', role: 'User', status: 'Inactive', lastLogin: '2026-01-08 10:15' },
    { id: 14, firstName: 'Emily', lastName: 'Nguyen', email: 'emily.nguyen@enterprise.ai', role: 'Viewer', status: 'Active', lastLogin: '2026-02-23 09:50' },
    { id: 15, firstName: 'Daniel', lastName: "O'Connor", email: 'daniel.oconnor@enterprise.ai', role: 'User', status: 'Active', lastLogin: '2026-02-24 20:11' },
  ]);

  filteredUsers = computed(() => {
    let result = this.users();
    const search = this.searchTerm().toLowerCase();
    const role = this.roleFilter();
    const status = this.statusFilter();

    if (search) {
      result = result.filter(
        (u) =>
          u.firstName.toLowerCase().includes(search) ||
          u.lastName.toLowerCase().includes(search) ||
          u.email.toLowerCase().includes(search),
      );
    }
    if (role) {
      result = result.filter((u) => u.role === role);
    }
    if (status) {
      result = result.filter((u) => u.status === status);
    }
    return result;
  });

  onSearchChange(event: Event): void {
    this.searchTerm.set((event.target as HTMLInputElement).value);
  }

  onRoleFilterChange(event: Event): void {
    this.roleFilter.set((event.target as HTMLSelectElement).value);
  }

  onStatusFilterChange(event: Event): void {
    this.statusFilter.set((event.target as HTMLSelectElement).value);
  }

  toggleUserSelection(userId: number): void {
    this.selectedUserIds.update((ids) => {
      const index = ids.indexOf(userId);
      if (index > -1) {
        return ids.filter((id) => id !== userId);
      }
      return [...ids, userId];
    });
  }

  isUserSelected(userId: number): boolean {
    return this.selectedUserIds().includes(userId);
  }

  toggleSelectAll(): void {
    const filtered = this.filteredUsers();
    if (this.isAllSelected()) {
      this.selectedUserIds.set([]);
    } else {
      this.selectedUserIds.set(filtered.map((u) => u.id));
    }
  }

  isAllSelected(): boolean {
    const filtered = this.filteredUsers();
    return filtered.length > 0 && filtered.every((u) => this.selectedUserIds().includes(u.id));
  }

  clearSelection(): void {
    this.selectedUserIds.set([]);
  }

  getStatusClass(status: string): string {
    return status === 'Active' ? 'status-active' : 'status-inactive';
  }

  getRoleBadgeClass(role: string): string {
    switch (role) {
      case 'Admin':
        return 'role-admin';
      case 'User':
        return 'role-user';
      case 'Viewer':
        return 'role-viewer';
      default:
        return '';
    }
  }

  getUserInitials(user: AdminUser): string {
    return (user.firstName[0] + user.lastName[0]).toUpperCase();
  }

  trackByUserId(_index: number, user: AdminUser): number {
    return user.id;
  }
}
