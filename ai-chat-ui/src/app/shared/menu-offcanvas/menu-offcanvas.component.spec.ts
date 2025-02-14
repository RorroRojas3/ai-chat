import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MenuOffcanvasComponent } from './menu-offcanvas.component';

describe('MenuOffcanvasComponent', () => {
  let component: MenuOffcanvasComponent;
  let fixture: ComponentFixture<MenuOffcanvasComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MenuOffcanvasComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MenuOffcanvasComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
