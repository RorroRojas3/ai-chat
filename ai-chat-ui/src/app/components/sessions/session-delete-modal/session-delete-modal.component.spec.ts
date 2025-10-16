import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SessionDeleteModalComponent } from './session-delete-modal.component';

describe('SessionDeleteModalComponent', () => {
  let component: SessionDeleteModalComponent;
  let fixture: ComponentFixture<SessionDeleteModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SessionDeleteModalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SessionDeleteModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
