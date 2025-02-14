import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './shared/navbar/navbar.component';
import { ChatService } from './services/chat.service';
import { StoreService } from './store/store.service';
import { MenuOffcanvasComponent } from './shared/menu-offcanvas/menu-offcanvas.component';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavbarComponent, MenuOffcanvasComponent],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  constructor(
    private chatService: ChatService,
    private storeService: StoreService
  ) {}

  ngOnInit(): void {
    forkJoin([
      this.chatService.getModels(),
      this.chatService.getSessions(),
    ]).subscribe(([models, sessions]) => {
      this.storeService.models.set(models);
      this.storeService.selectedModelId.set(models[0].name);
      this.storeService.sessions.set(sessions);
    });
  }
}
