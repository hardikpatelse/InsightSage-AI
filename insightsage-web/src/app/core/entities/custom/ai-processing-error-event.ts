export interface AIProcessingErrorEvent {
    sessionId: number
    messageId?: number
    error: string
    timestamp: string
}