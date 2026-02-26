import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { MsalGuard } from '@azure/msal-angular';
import { ConversationsComponent } from './pages/conversations/conversations.component';
import { AdminComponent } from './pages/admin/admin.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/chat',
    pathMatch: 'full',
  },
  {
    path: 'chat',
    component: HomeComponent,
    canActivate: [MsalGuard],
  },
  {
    path: 'chat/conversation/:conversationId',
    component: HomeComponent,
    canActivate: [MsalGuard],
  },
  {
    path: 'conversations',
    component: ConversationsComponent,
    canActivate: [MsalGuard],
  },
  {
    path: 'admin',
    component: AdminComponent,
    canActivate: [MsalGuard],
  },
  {
    path: '**',
    redirectTo: '/chat',
  },
];
