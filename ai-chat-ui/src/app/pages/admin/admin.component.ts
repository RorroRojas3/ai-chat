import { Component, signal } from '@angular/core';
import { UsersTabComponent } from './components/users-tab/users-tab.component';
import { ModelsTabComponent } from './components/models-tab/models-tab.component';

@Component({
  selector: 'app-admin',
  imports: [UsersTabComponent, ModelsTabComponent],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.scss',
})
export class AdminComponent {
  activeTab = signal<'users' | 'models'>('users');

  switchTab(tab: 'users' | 'models'): void {
    this.activeTab.set(tab);
  }
}
