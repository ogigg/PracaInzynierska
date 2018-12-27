import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DevicesManagerComponent } from './devices-manager.component';

describe('DevicesManagerComponent', () => {
  let component: DevicesManagerComponent;
  let fixture: ComponentFixture<DevicesManagerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DevicesManagerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DevicesManagerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
