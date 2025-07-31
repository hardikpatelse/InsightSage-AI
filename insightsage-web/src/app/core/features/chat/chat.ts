import { Component } from '@angular/core'
import { ChatService } from '../../services/chat.service'
import { ChatMessage } from '../../entities/chat-message'

@Component({
  selector: 'app-chat',
  standalone: false,
  templateUrl: './chat.html',
  styleUrl: './chat.scss'
})
export class Chat {
  prompt: string = ''
  messages: ChatMessage[] = []

  constructor(private chatService: ChatService) {
    // Initialize chat messages or any other setup if needed
  }

  sendPrompt() {
    this.chatService.sendPrompt(this.prompt).subscribe(res => {
      this.messages.push({ text: "You: " + this.prompt, isUser: true })
      this.messages.push({ text: "AI: " + res.message, isUser: false })
      this.prompt = ''
    })
  }
}
