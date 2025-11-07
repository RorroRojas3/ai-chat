import { NgClass } from '@angular/common';
import { Component, input, inject } from '@angular/core';
import { MessageDto } from '../../../dtos/MessageDto';
import { StoreService } from '../../../store/store.service';

@Component({
  selector: 'app-message-bubble',
  imports: [NgClass],
  templateUrl: './message-bubble.component.html',
  styleUrl: './message-bubble.component.scss',
})
export class MessageBubbleComponent {
  message = input.required<MessageDto>();

  public readonly storeService = inject(StoreService);
}
