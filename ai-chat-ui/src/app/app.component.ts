import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './shared/navbar/navbar.component';
import { ChatService } from './services/chat.service';
import { StoreService } from './store/store.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavbarComponent],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  constructor(
    private chatService: ChatService,
    private storeService: StoreService
  ) {}

  ngOnInit(): void {
    this.chatService.getModels().subscribe((models) => {
      this.storeService.models.set(models);
    });
  }
}
