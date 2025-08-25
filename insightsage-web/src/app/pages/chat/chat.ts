import { AIModel } from '@/core/entities/ai-model'
import { ChatMode } from '@/core/entities/chat-mode'
import { ChatMessageExtended } from '@/core/entities/custom/chat-message-extended'
import { ChatSession } from '@/core/entities/chat-session'
import { ChatMessageService } from '@/core/services/chat-message.service'
import { ChatSessionService } from '@/core/services/chat-session.service'
import { AIModelService } from '@/core/services/ai-model.service'
import { ChatModeService } from '@/core/services/chat-mode.service'
import { Component, OnInit, OnDestroy } from '@angular/core'
import { ActivatedRoute, Router } from '@angular/router'
import { Subject, takeUntil, switchMap, Observable, tap } from 'rxjs'

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

  // Chat modes - will be loaded from service
  chatModes: ChatMode[] = []
  selectedChatMode: ChatMode | null = null

  // AI Models - will be loaded from service
  aiModels: AIModel[] = []
  selectedAIModel: AIModel | null = null

  constructor(
    private chatMessageService: ChatMessageService,
    private chatSessionService: ChatSessionService,
    private aiModelService: AIModelService,
    private chatModeService: ChatModeService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit() {
    // Load chat modes and AI models
    this.loadChatModes()
    this.loadAIModels()

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
          this.clearMessages()
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
          if (session.aiModel) {
            this.selectedAIModel = session.aiModel
          }
          if (session.chatMode) {
            this.selectedChatMode = session.chatMode
          }
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
   * Load chat modes from the service
   */
  private loadChatModes() {
    this.chatModeService.getActiveModes()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (modes) => {
          this.chatModes = modes
          // Set the default mode or first mode as selected
          if (modes.length > 0 && !this.selectedChatMode) {
            // Try to find 'chat' mode first, otherwise use first mode
            const defaultMode = modes.find(mode => mode.value === 'chat') || modes[0]
            this.selectedChatMode = defaultMode
          }
        },
        error: (error) => {
          console.error('Failed to load chat modes:', error)
          // Use fallback modes if API fails
          this.setFallbackChatModes()
        }
      })
  }

  /**
   * Set fallback chat modes if API fails
   */
  private setFallbackChatModes() {
    this.chatModes = [
      new ChatMode({
        id: 1,
        name: 'Chat',
        value: 'chat',
        iconName: 'pi pi-comments',
        description: 'General conversation mode',
        isActive: true,
        sortOrder: 1
      }),
      new ChatMode({
        id: 2,
        name: 'Code Review',
        value: 'code-review',
        iconName: 'pi pi-code',
        description: 'Review and analyze code',
        isActive: true,
        sortOrder: 2
      }),
      new ChatMode({
        id: 3,
        name: 'Explain',
        value: 'explain',
        iconName: 'pi pi-info-circle',
        description: 'Explain complex concepts',
        isActive: true,
        sortOrder: 3
      }),
      new ChatMode({
        id: 4,
        name: 'Fix',
        value: 'fix',
        iconName: 'pi pi-wrench',
        description: 'Fix issues and bugs',
        isActive: true,
        sortOrder: 4
      })
    ]
    this.selectedChatMode = this.chatModes[0]
  }

  /**
   * Load AI models from the service
   */
  private loadAIModels() {
    this.aiModelService.getActiveModels()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (models) => {
          this.aiModels = models
          // Set the first model as default if none selected
          if (models.length > 0 && !this.selectedAIModel) {
            this.selectedAIModel = models[0]
          }
        },
        error: (error) => {
          console.error('Failed to load AI models:', error)
          // Use fallback models if API fails
          this.setFallbackAIModels()
        }
      })
  }

  /**
   * Set fallback AI models if API fails
   */
  private setFallbackAIModels() {
    this.aiModels = [
      new AIModel({
        id: 1,
        name: 'GPT-4o',
        value: 'gpt-4o',
        description: 'Most capable model for complex tasks',
        isActive: true,
        provider: 'openai'
      }),
      new AIModel({
        id: 2,
        name: 'GPT-4o Mini',
        value: 'gpt-4o-mini',
        description: 'Faster responses, good for simple tasks',
        isActive: true,
        provider: 'openai'
      }),
      new AIModel({
        id: 3,
        name: 'Claude 3.5 Sonnet',
        value: 'claude-3.5-sonnet',
        description: 'Great for code and analysis',
        isActive: true,
        provider: 'anthropic'
      })
    ]
    this.selectedAIModel = this.aiModels[0]
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
      // Get current selected AI model and chat mode IDs for welcome message
      const aiChatModelId = this.selectedAIModel?.id ?? undefined
      const chatModeId = this.selectedChatMode?.id ?? undefined

      this.chatMessageService.addAssistantMessage(
        'Hello! I\'m your AI assistant. How can I help you today?',
        this.currentSessionId ?? 0,
        aiChatModelId,
        chatModeId
      ).subscribe({
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

    // Get current selected AI model and chat mode IDs
    const aiModelId = this.selectedAIModel?.id ?? undefined
    const chatModeId = this.selectedChatMode?.id ?? undefined

    // Send user message via service
    this.chatMessageService.sendUserMessage(
      messageContent,
      this.currentSessionId ?? 0,
      aiModelId,
      chatModeId
    )
      .pipe(takeUntil(this.destroy$))
      .subscribe({
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
    // setTimeout(() => {
    //   const aiResponse = this.generateAIResponse(userMessage)

    //   this.chatMessageService.addAssistantMessage(aiResponse, this.currentSessionId ?? 0)
    //     .pipe(takeUntil(this.destroy$))
    //     .subscribe({
    //       next: (messageId) => {
    //         console.log('AI response added successfully:', messageId)
    //       },
    //       error: (error) => {
    //         console.error('Failed to add AI response:', error)
    //       }
    //     })
    // }, 1500)
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
  // private generateAIResponse(userMessage: string): string {
  //   const responses = {
  //     'chat': `I understand you're asking about: "${userMessage}". Let me help you with that.`,
  //     'code-review': `Here's my code review analysis for: "${userMessage}". I notice a few areas for improvement...`,
  //     'explain': `Let me explain "${userMessage}" in detail...`,
  //     'fix': `I'll help you fix the issue with: "${userMessage}". Here's the solution...`
  //   }

  //   return responses[this.selectedChatMode.value as keyof typeof responses] ||
  //     `Thanks for your message: "${userMessage}". How can I assist you further?`
  // }

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
   * Create a new chat session with current AI model and chat mode settings
   */
  createNewChatSession(sessionName?: string): Observable<ChatSession> {
    const aiModelId = this.selectedAIModel?.id ?? undefined
    const chatModeId = this.selectedChatMode?.id ?? undefined

    return this.chatSessionService.createNewSession(
      sessionName,
      aiModelId,
      chatModeId
    ).pipe(
      tap(newSession => {
        // Navigate to the new session
        this.router.navigate(['/chat', newSession.uniqueIdentity])
      })
    )
  }

  /**
   * Start a new chat with current settings
   */
  startNewChat(): void {
    const sessionName = `Chat Session ${new Date().toLocaleString()}`
    this.createNewChatSession(sessionName).subscribe({
      error: (error) => {
        console.error('Failed to create new chat session:', error)
      }
    })
  }

  /**
   * Clear current session messages (UI only)
   */
  clearMessages() {
    this.chatMessageService.clearCurrentSessionMessages()
  }
}