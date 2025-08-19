import { ComponentFixture, TestBed } from '@angular/core/testing'

import { Chat } from './chat'
import { ChatModule } from './chat.module'

describe('Chat', () => {
  let component: Chat
  let fixture: ComponentFixture<Chat>

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChatModule]
    })
      .compileComponents()

    fixture = TestBed.createComponent(Chat)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
