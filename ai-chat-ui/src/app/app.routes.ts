import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { MsalGuard } from '@azure/msal-angular';
import { SessionsComponent } from './pages/sessions/sessions.component';
import { ProjectsComponent } from './pages/projects/projects.component';
import { ProjectsCreateComponent } from './pages/projects/projects-create/projects-create.component';
import { ProjectDetailComponent } from './pages/projects/project-detail/project-detail.component';

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
    path: 'projects/create',
    component: ProjectsCreateComponent,
    canActivate: [MsalGuard],
  },
  {
    path: 'projects/:id',
    component: ProjectDetailComponent,
    canActivate: [MsalGuard],
  },
  {
    path: '**',
    redirectTo: '/chat',
  },
];
