export interface AIResponseReceivedEvent {
    messageId: number
    sessionId: number
    content: string
    timestamp: string
    usage: {
        promptTokens: number
        completionTokens: number
        totalTokens: number
    }
}