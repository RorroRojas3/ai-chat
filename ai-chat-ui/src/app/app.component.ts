import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './shared/navbar/navbar.component';
import { StoreService } from './store/store.service';
import { MenuOffcanvasComponent } from './shared/menu-offcanvas/menu-offcanvas.component';
import { forkJoin } from 'rxjs';
import { ModelService } from './services/model.service';
import { SessionService } from './services/session.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavbarComponent, MenuOffcanvasComponent],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  constructor(
    private storeService: StoreService,
    private sessionService: SessionService,
    private modelService: ModelService
  ) {}

  ngOnInit(): void {
    forkJoin([
      this.modelService.getModels(),
      this.sessionService.searchSessions(''),
    ]).subscribe(([models, sessions]) => {
      this.storeService.models.set(models);
      this.storeService.selectedModel.set(models[0]);
      this.storeService.sessions.set(sessions);
    });
  }
}
