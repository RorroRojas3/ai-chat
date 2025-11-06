import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SessionRenameModalComponent } from './session-rename-modal.component';

describe('SessionRenameModalComponent', () => {
  let component: SessionRenameModalComponent;
  let fixture: ComponentFixture<SessionRenameModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SessionRenameModalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SessionRenameModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
