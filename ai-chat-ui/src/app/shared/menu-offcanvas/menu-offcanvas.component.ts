import { Component } from '@angular/core';
import { StoreService } from '../../store/store.service';

@Component({
  selector: 'app-menu-offcanvas',
  imports: [],
  templateUrl: './menu-offcanvas.component.html',
  styleUrl: './menu-offcanvas.component.scss',
})
export class MenuOffcanvasComponent {
  constructor(public storeService: StoreService) {}
}
