import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CheckboxListComponent } from './checkbox-list.component';

describe('CheckboxListComponent', () => {
  let component: CheckboxListComponent;
  let fixture: ComponentFixture<CheckboxListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      //declarations: [CheckboxListComponent],
      imports: [CheckboxListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CheckboxListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
