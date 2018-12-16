import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SignalRtestComponent } from './signal-rtest.component';

describe('SignalRtestComponent', () => {
  let component: SignalRtestComponent;
  let fixture: ComponentFixture<SignalRtestComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SignalRtestComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SignalRtestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
