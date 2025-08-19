import { ChatMessage } from '@/core/entities/chat-message'
import { ChatMessageExtended } from '@/core/entities/custom/chat-message-extended'
import { Component, ElementRef, Input, AfterViewInit, OnChanges, SimpleChanges, ViewChild } from '@angular/core'

@Component({
  selector: 'app-chat-history',
  standalone: false,
  templateUrl: './chat-history.html',
  styleUrl: './chat-history.scss'
})
export class ChatHistory implements AfterViewInit, OnChanges {
  @Input() messages: ChatMessageExtended[] = []
  @Input() isLoading: boolean = false

  @ViewChild('chatContainer') chatContainer!: ElementRef

  ngAfterViewInit() {
    // Scroll to the bottom after view initialization
    this.scrollToBottom()
  }

  ngOnChanges(changes: SimpleChanges) {
    // Scroll to bottom when messages or loading state changes
    if (changes['messages'] || changes['isLoading']) {
      // Use setTimeout to ensure DOM has updated
      setTimeout(() => this.scrollToBottom(), 50)
    }
  }

  private scrollToBottom() {
    try {
      if (this.chatContainer?.nativeElement) {
        const element = this.chatContainer.nativeElement
        element.scrollTop = element.scrollHeight
      }
    } catch (err) {
      console.error('Error scrolling to bottom:', err)
    }
  }

  formatTime(timestamp: Date): string {
    return timestamp.toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit'
    })
  }
}
