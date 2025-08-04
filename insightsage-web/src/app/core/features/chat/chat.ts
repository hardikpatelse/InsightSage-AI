import { Component, OnInit } from '@angular/core'
import { ChatService } from '../../services/chat.service'
import { ChatMessage } from '../../entities/chat-message'
import { UserService } from '../../services/user.service'
import { User } from '../../entities/user'

@Component({
  selector: 'app-chat',
  standalone: false,
  templateUrl: './chat.html',
  styleUrl: './chat.scss'
})
export class Chat implements OnInit {
  user: User | null = null
  prompt: string = ''
  messages: ChatMessage[] = []

  constructor(private chatService: ChatService, private userService: UserService) { }
  // Initialize chat messages or any other setup if needed
  ngOnInit(): void {

    this.messages = [
      {
        id: 1,
        role: "user",
        message: "Can you help me analyze our Q3 sales data?",
        timestamp: "2025-07-30T20:25:00Z",
        isUser: true
      },
      {
        id: 2,
        role: "assistant",
        message: "I'd be happy to help you analyze your Q3 sales data. Please share the data you'd like me to examine, and I can provide insights on trends, performance metrics, and actionable recommendations.",
        timestamp: "2025-07-30T20:25:05Z",
        isUser: false
      },
      {
        id: 3,
        role: "user",
        message: "What are the key metrics I should focus on?",
        timestamp: "2025-07-30T20:26:00Z",
        isUser: true
      },
      {
        id: 4,
        role: "assistant",
        message: "For Q3 sales analysis, focus on these key metrics: 1) Revenue growth vs Q2 and Q3 last year, 2) Customer acquisition cost (CAC), 3) Average deal size, 4) Sales cycle length, 5) Win rate by product/region, 6) Pipeline velocity, and 7) Customer lifetime value (CLV). These will give you a comprehensive view of your sales performance.",
        timestamp: "2025-07-30T20:26:08Z",
        isUser: false
      }
    ],

      this.userService.getCurrentUser().subscribe(user => {
        console.log('Current user:', user)
        if (user) {
          this.user = user
        } else {
          this.user = null
        }
      })
  }

  sendPrompt() {
    this.chatService.sendPrompt(this.prompt).subscribe(res => {
      this.messages.push({ message: this.prompt, isUser: true })
      this.messages.push({ message: res.message, isUser: false })
      this.prompt = ''
    })
  }
}
