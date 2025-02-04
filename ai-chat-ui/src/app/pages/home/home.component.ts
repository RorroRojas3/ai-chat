import { Component } from '@angular/core';
import { PromptBoxComponent } from '../../components/home/prompt-box/prompt-box.component';

@Component({
  selector: 'app-home',
  imports: [PromptBoxComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss',
})
export class HomeComponent {}
