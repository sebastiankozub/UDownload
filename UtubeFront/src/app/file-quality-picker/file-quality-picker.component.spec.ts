import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FileQualityPickerComponent } from './file-quality-picker.component';

describe('FileQualityPickerComponent', () => {
  let component: FileQualityPickerComponent;
  let fixture: ComponentFixture<FileQualityPickerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [FileQualityPickerComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FileQualityPickerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
