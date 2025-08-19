import { Injectable } from '@angular/core'
import { Observable, BehaviorSubject, map, catchError, of, throwError } from 'rxjs'
import { ApiService } from './api.service'
import { environment } from '../../../environments/environment'
import { ChatMessage } from '../entities/chat-message'
import { ChatMessageResponse } from '../entities/responses/chat-message-response'
import { ChatMessagesResponse } from '../entities/responses/chat-messages-response'
import { AddUpdateEntityResponse } from '../entities/common/add-update-entity-response'
import { DeleteEntityResponse } from '../entities/common/delete-entity-response'
import { ChatMessageExtended } from '../entities/custom/chat-message-extended'

/**
 * Service for managing chat messages with comprehensive API integration
 * Based on the Swagger API specification for Message endpoints
 */
@Injectable({
    providedIn: 'root'
})
export class ChatMessageService {
    private readonly baseUrl = environment.chatApiBaseUrl
    private readonly endpoint = 'Message'

    // State management for messages
    private messagesSubject = new BehaviorSubject<ChatMessageExtended[]>([])
    public messages$ = this.messagesSubject.asObservable()

    private loadingSubject = new BehaviorSubject<boolean>(false)
    public loading$ = this.loadingSubject.asObservable()

    private currentSessionIdSubject = new BehaviorSubject<number | null>(null)
    public currentSessionId$ = this.currentSessionIdSubject.asObservable()

    constructor(private apiService: ApiService) { }

    // ========================
    // API Methods
    // ========================

    /**
     * GET /api/Message
     * Get all chat messages
     */
    getAllMessages(): Observable<ChatMessage[]> {
        this.loadingSubject.next(true)

        return this.apiService.get<ChatMessage[]>(this.baseUrl, this.endpoint).pipe(
            map(messages => {
                this.loadingSubject.next(false)

                // The interceptor automatically unwraps successful responses
                return Array.isArray(messages) ? messages : []
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error('Error fetching all messages:', error)
                return of([])
            })
        )
    }

    /**
     * GET /api/Message/{id}
     * Get a specific chat message by ID
     */
    getMessageById(id: number): Observable<ChatMessage | null> {
        this.loadingSubject.next(true)

        return this.apiService.get<ChatMessage>(this.baseUrl, `${this.endpoint}/${id}`).pipe(
            map(message => {
                this.loadingSubject.next(false)

                // The interceptor automatically unwraps successful responses
                return message || null
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error(`Error fetching message ${id}:`, error)
                return of(null)
            })
        )
    }

    /**
     * GET /api/Message/session/{sessionId}
     * Get all messages for a specific chat session
     */
    getMessagesBySessionId(sessionId: number): Observable<ChatMessage[]> {
        this.loadingSubject.next(true)

        return this.apiService.get<ChatMessage[]>(this.baseUrl, `${this.endpoint}/session/${sessionId}`).pipe(
            map(messages => {
                this.loadingSubject.next(false)
                console.log('ChatMessageService - getMessagesBySessionId response (unwrapped by interceptor):', messages)

                // The interceptor automatically unwraps successful responses, so we get the array directly
                const messageArray = Array.isArray(messages) ? messages : []

                console.log('ChatMessageService - extracted messages:', messageArray)
                const uiMessages = this.convertToExtendedMessages(messageArray)
                this.messagesSubject.next(uiMessages)
                this.currentSessionIdSubject.next(sessionId)
                return messageArray
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error(`Error fetching messages for session ${sessionId}:`, error)
                this.messagesSubject.next([])
                return of([])
            })
        )
    }

    /**
     * POST /api/Message
     * Create a new chat message
     */
    createMessage(message: Partial<ChatMessage>): Observable<number> {
        this.loadingSubject.next(true)

        const messagePayload: Partial<ChatMessage> = {
            chatSessionId: message.chatSessionId,
            role: message.role,
            content: message.content,
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString()
        }

        return this.apiService.post<number>(this.baseUrl, this.endpoint, messagePayload).pipe(
            map(messageId => {
                this.loadingSubject.next(false)

                // The interceptor automatically unwraps successful responses
                const result = typeof messageId === 'number' ? messageId : 0

                // Refresh messages for current session if applicable
                if (message.chatSessionId === this.currentSessionIdSubject.value) {
                    this.refreshCurrentSessionMessages()
                }
                return result
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error('Error creating message:', error)
                return throwError(() => error)
            })
        )
    }

    /**
     * PUT /api/Message/{id}
     * Update an existing chat message
     */
    updateMessage(id: number, message: Partial<ChatMessage>): Observable<number> {
        this.loadingSubject.next(true)

        const updatePayload: Partial<ChatMessage> = {
            ...message,
            id,
            updatedAt: new Date().toISOString()
        }

        return this.apiService.put<number>(this.baseUrl, `${this.endpoint}/${id}`, updatePayload).pipe(
            map(messageId => {
                this.loadingSubject.next(false)

                // The interceptor automatically unwraps successful responses
                const result = typeof messageId === 'number' ? messageId : 0

                // Refresh messages for current session if applicable
                if (message.chatSessionId === this.currentSessionIdSubject.value) {
                    this.refreshCurrentSessionMessages()
                }
                return result
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error(`Error updating message ${id}:`, error)
                return throwError(() => error)
            })
        )
    }

    /**
     * DELETE /api/Message/{id}
     * Delete a chat message
     */
    deleteMessage(id: number, modifiedBy: number = 0): Observable<string> {
        this.loadingSubject.next(true)

        const deleteEndpoint = `${this.endpoint}/${id}?modifiedBy=${modifiedBy}`

        return this.apiService.delete<string>(this.baseUrl, deleteEndpoint).pipe(
            map(response => {
                this.loadingSubject.next(false)

                // The interceptor automatically unwraps successful responses
                const result = typeof response === 'string' ? response : 'Message deleted successfully'

                // Refresh messages for current session
                this.refreshCurrentSessionMessages()
                return result
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error(`Error deleting message ${id}:`, error)
                return throwError(() => error)
            })
        )
    }

    /**
     * GET /api/Message/health
     * Check message service health
     */
    checkHealth(): Observable<boolean> {
        return this.apiService.get<any>(this.baseUrl, `${this.endpoint}/health`).pipe(
            map(() => true),
            catchError(error => {
                console.error('Message service health check failed:', error)
                return of(false)
            })
        )
    }

    // ========================
    // Convenience Methods
    // ========================

    /**
     * Send a user message and optionally trigger AI response
     */
    sendUserMessage(content: string, sessionId: number): Observable<number> {
        const userMessage: Partial<ChatMessage> = {
            chatSessionId: sessionId,
            role: 'user',
            content: content.trim()
        }

        return this.createMessage(userMessage)
    }

    /**
     * Add an AI assistant response
     */
    addAssistantMessage(content: string, sessionId: number): Observable<number> {
        const assistantMessage: Partial<ChatMessage> = {
            chatSessionId: sessionId,
            role: 'assistant',
            content: content.trim()
        }

        return this.createMessage(assistantMessage)
    }

    /**
     * Add a system message
     */
    addSystemMessage(content: string, sessionId: number): Observable<number> {
        const systemMessage: Partial<ChatMessage> = {
            chatSessionId: sessionId,
            role: 'system',
            content: content.trim()
        }

        return this.createMessage(systemMessage)
    }

    /**
     * Clear all messages from current session (UI only)
     */
    clearCurrentSessionMessages(): void {
        this.messagesSubject.next([])
    }

    /**
     * Set loading state
     */
    setLoadingState(loading: boolean): void {
        this.loadingSubject.next(loading)
    }

    /**
     * Get current messages
     */
    getCurrentMessages(): ChatMessageExtended[] {
        return this.messagesSubject.value
    }

    /**
     * Get current session ID
     */
    getCurrentSessionId(): number | null {
        return this.currentSessionIdSubject.value
    }

    /**
     * Load messages for a specific session and set it as current
     */
    loadSessionMessages(sessionId: number): Observable<ChatMessage[]> {
        return this.getMessagesBySessionId(sessionId)
    }

    /**
     * Refresh messages for the current session
     */
    refreshCurrentSessionMessages(): void {
        const currentSessionId = this.currentSessionIdSubject.value
        if (currentSessionId) {
            this.getMessagesBySessionId(currentSessionId).subscribe()
        }
    }

    /**
     * Add a temporary UI message (for optimistic updates)
     */
    addTemporaryMessage(message: Partial<ChatMessageExtended>): void {
        const currentMessages = this.messagesSubject.value
        const tempMessage: ChatMessageExtended = new ChatMessageExtended({
            ...message,
        })

        this.messagesSubject.next([...currentMessages, tempMessage])
    }

    /**
     * Remove a temporary message
     */
    removeTemporaryMessage(tempId: number): void {
        const currentMessages = this.messagesSubject.value
        const filteredMessages = currentMessages.filter(msg => msg.id !== tempId)
        this.messagesSubject.next(filteredMessages)
    }

    /**
     * Update a temporary message with real data
     */
    updateTemporaryMessage(tempId: number, realMessage: ChatMessage): void {
        const currentMessages = this.messagesSubject.value
        const messageIndex = currentMessages.findIndex(msg => msg.id === tempId)

        if (messageIndex !== -1) {
            const updatedMessages = [...currentMessages]
            updatedMessages[messageIndex] = this.convertToExtendedMessage(realMessage)
            this.messagesSubject.next(updatedMessages)
        }
    }

    // ========================
    // Utility Methods
    // ========================

    /**
     * Convert API ChatMessage to UI ChatMessageExtended
     */
    private convertToExtendedMessage(message: ChatMessage): ChatMessageExtended {
        return new ChatMessageExtended({
            ...message,
        })
    }

    /**
     * Convert array of API ChatMessages to UI ChatMessageExtended array
     */
    private convertToExtendedMessages(messages: ChatMessage[]): ChatMessageExtended[] {
        console.log('ChatMessageService - converting messages to extended format:', messages)
        return messages.map(message => this.convertToExtendedMessage(message))
    }

    /**
     * Format message timestamp
     */
    formatMessageTime(timestamp: Date): string {
        return timestamp.toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit'
        })
    }

    /**
     * Check if a message is from the current user
     */
    isUserMessage(message: ChatMessage): boolean {
        return message.role === 'user'
    }

    /**
     * Check if a message is from the AI assistant
     */
    isAssistantMessage(message: ChatMessage): boolean {
        return message.role === 'assistant'
    }

    /**
     * Check if a message is a system message
     */
    isSystemMessage(message: ChatMessage): boolean {
        return message.role === 'system'
    }

    // ========================
    // Error Handling
    // ========================

    /**
     * Handle API errors with user-friendly messages
     */
    private handleError(operation: string, error: any): Observable<never> {
        console.error(`${operation} failed:`, error)

        let errorMessage = 'An unexpected error occurred'

        if (error?.error?.errors?.length > 0) {
            errorMessage = error.error.errors.join(', ')
        } else if (error?.error?.exceptionDetails) {
            errorMessage = error.error.exceptionDetails
        } else if (error?.message) {
            errorMessage = error.message
        }

        return throwError(() => new Error(errorMessage))
    }
}
