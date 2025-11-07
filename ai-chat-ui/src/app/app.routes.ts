import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { MsalGuard } from '@azure/msal-angular';
import { SessionsComponent } from './pages/sessions/sessions.component';
import { ProjectsComponent } from './pages/projects/projects.component';

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
    path: 'sessions',
    component: SessionsComponent,
    canActivate: [MsalGuard],
  },
  {
    path: 'projects',
    component: ProjectsComponent,
    canActivate: [MsalGuard],
  },
  {
    path: '**',
    redirectTo: '/chat',
  },
];
