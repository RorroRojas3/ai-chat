import { NgClass, NgIf } from '@angular/common';
import { AfterViewChecked, Component, Input, OnInit } from '@angular/core';
import { MessageDto } from '../../../dtos/MessageDto';
import { StoreService } from '../../../store/store.service';
import hljs from 'highlight.js';

@Component({
  selector: 'app-message-bubble',
  imports: [NgClass, NgIf],
  templateUrl: './message-bubble.component.html',
  styleUrl: './message-bubble.component.scss',
})
export class MessageBubbleComponent {
  @Input() message!: MessageDto;

  constructor(public storeService: StoreService) {}
}
