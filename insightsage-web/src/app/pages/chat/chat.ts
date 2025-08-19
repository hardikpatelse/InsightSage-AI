import { AIModel } from '@/core/entities/ai-model'
import { ChatMode } from '@/core/entities/chat-mode'
import { ChatMessageExtended } from '@/core/entities/custom/chat-message-extended'
import { ChatMessageService } from '@/core/services/chat-message.service'
import { ChatSessionService } from '@/core/services/chat-session.service'
import { Component, OnInit, OnDestroy } from '@angular/core'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, takeUntil, switchMap } from 'rxjs'

@Component({
  selector: 'app-chat',
  templateUrl: './chat.html',
  styleUrl: './chat.scss',
  standalone: false
})
export class Chat implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>()

  messages: ChatMessageExtended[] = []
  newMessage: string = ''
  isLoading: boolean = false
  currentSessionId: number | null = null
  currentSessionUuid: string | null = null

  // Chat modes similar to GitHub Copilot
  chatModes: ChatMode[] = [
    new ChatMode('Chat', 'chat', 'pi pi-comments'),
    new ChatMode('Code Review', 'code-review', 'pi pi-code'),
    new ChatMode('Explain', 'explain', 'pi pi-info-circle'),
    new ChatMode('Fix', 'fix', 'pi pi-wrench')
  ]

  selectedChatMode: ChatMode = this.chatModes[0]

  // AI Models
  aiModels: AIModel[] = [
    new AIModel(1, 'GPT-4o', 'gpt-4o', 'Most capable model for complex tasks'),
    new AIModel(2, 'GPT-4o Mini', 'gpt-4o-mini', 'Faster responses, good for simple tasks'),
    new AIModel(3, 'Claude 3.5 Sonnet', 'claude-3.5-sonnet', 'Great for code and analysis'),
    new AIModel(4, 'Gemini Pro', 'gemini-pro', 'Google\'s advanced model')
  ]

  selectedAIModel: AIModel = this.aiModels[0]

  constructor(
    private chatMessageService: ChatMessageService,
    private chatSessionService: ChatSessionService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit() {
    // Handle route parameters for session
    this.route.params
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        const sessionId = params['sessionId']
        if (sessionId) {
          // Load session by unique ID (UUID)
          this.currentSessionUuid = sessionId
          this.loadSessionByUuid(sessionId)
        } else {
          // New chat session
          this.currentSessionId = null
          this.currentSessionUuid = null
        }
      })

    // Subscribe to messages from the service
    this.chatMessageService.messages$
      .pipe(takeUntil(this.destroy$))
      .subscribe(messages => {
        this.messages = messages
      })

    // Subscribe to loading state
    this.chatMessageService.loading$
      .pipe(takeUntil(this.destroy$))
      .subscribe(loading => {
        this.isLoading = loading
      })
  }

  ngOnDestroy() {
    this.destroy$.next()
    this.destroy$.complete()
  }

  /**
   * Load session by UUID from route parameter
   */
  private loadSessionByUuid(uuid: string) {
    this.chatSessionService.getSessionByUniqueId(uuid)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (session) => {
          this.currentSessionId = session.id || null
          this.currentSessionUuid = session.uniqueIdentity
          this.loadSessionMessages()
        },
        error: (error) => {
          console.error('Failed to load session by UUID:', error)
          // Redirect to new chat if session not found
          this.router.navigate(['/chat'])
        }
      })
  }

  /**
   * Load messages for the current chat session
   */
  private loadSessionMessages() {
    if (!this.currentSessionId || this.currentSessionId <= 0) {
      return
    }
    this.chatMessageService.loadSessionMessages(this.currentSessionId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (messages) => {
          console.log('Messages loaded successfully:', messages.length)
        },
        error: (error) => {
          console.error('Failed to load messages:', error)
          // Initialize with welcome message if no messages found
          this.initializeWithWelcomeMessage()
        }
      })
  }

  /**
   * Initialize chat with a welcome message if no messages exist
   */
  private initializeWithWelcomeMessage() {
    if (this.messages.length === 0) {
      this.chatMessageService.addAssistantMessage(
        'Hello! I\'m your AI assistant. How can I help you today?',
        this.currentSessionId ?? 0
      ).subscribe({
        next: () => {
          console.log('Welcome message added')
        },
        error: (error) => {
          console.error('Failed to add welcome message:', error)
        }
      })
    }
  }

  /**
   * Send a message from the user
   */
  sendMessage() {
    if (!this.newMessage.trim() || this.isLoading) return

    const messageContent = this.newMessage.trim()
    this.newMessage = ''

    // Send user message via service
    this.chatMessageService.sendUserMessage(messageContent, this.currentSessionId ?? 0)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (messageId) => {
          console.log('User message sent successfully:', messageId)

          // Simulate AI response after a delay
          this.simulateAIResponse(messageContent)
        },
        error: (error) => {
          console.error('Failed to send message:', error)
          this.newMessage = messageContent // Restore message if failed
        }
      })
  }

  /**
   * Simulate AI response (replace with real AI integration)
   */
  private simulateAIResponse(userMessage: string) {
    setTimeout(() => {
      const aiResponse = this.generateAIResponse(userMessage)

      this.chatMessageService.addAssistantMessage(aiResponse, this.currentSessionId ?? 0)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (messageId) => {
            console.log('AI response added successfully:', messageId)
          },
          error: (error) => {
            console.error('Failed to add AI response:', error)
          }
        })
    }, 1500)
  }

  /**
   * Handle keyboard events for message input
   */
  onKeyPress(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault()
      this.sendMessage()
    }
  }

  /**
   * Generate AI response based on selected mode (mock implementation)
   */
  private generateAIResponse(userMessage: string): string {
    const responses = {
      'chat': `I understand you're asking about: "${userMessage}". Let me help you with that.`,
      'code-review': `Here's my code review analysis for: "${userMessage}". I notice a few areas for improvement...`,
      'explain': `Let me explain "${userMessage}" in detail...`,
      'fix': `I'll help you fix the issue with: "${userMessage}". Here's the solution...`
    }

    return responses[this.selectedChatMode.value as keyof typeof responses] ||
      `Thanks for your message: "${userMessage}". How can I assist you further?`
  }

  /**
   * Format timestamp for display
   */
  formatTime(timestamp: Date): string {
    return this.chatMessageService.formatMessageTime(timestamp)
  }

  /**
   * Refresh current session messages
   */
  refreshMessages() {
    this.chatMessageService.refreshCurrentSessionMessages()
  }

  /**
   * Clear current session messages (UI only)
   */
  clearMessages() {
    this.chatMessageService.clearCurrentSessionMessages()
  }
}