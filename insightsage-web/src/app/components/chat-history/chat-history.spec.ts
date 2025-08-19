import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChatHistory } from './chat-history';

describe('ChatHistory', () => {
  let component: ChatHistory;
  let fixture: ComponentFixture<ChatHistory>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChatHistory]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ChatHistory);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
