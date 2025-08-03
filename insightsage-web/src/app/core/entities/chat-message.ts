export interface ChatMessage {
    message: string
    isUser: boolean
    timestamp?: string
    id?: number
    role?: 'user' | 'assistant' | 'system'
}