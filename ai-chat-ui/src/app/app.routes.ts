import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { MsalGuard } from '@azure/msal-angular';

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
    path: 'chat/session/:sessionId',
    component: HomeComponent,
    canActivate: [MsalGuard],
  },
  {
    path: '**',
    redirectTo: '/chat',
  },
];
