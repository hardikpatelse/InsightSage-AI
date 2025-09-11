import { ChatMessage } from "../chat-message"
import { BaseEntity } from "../common/base-entity"

export class ChatMessageRequest extends BaseEntity {
    message: ChatMessage = new ChatMessage()
    aiModelId: number | null = null
    chatModeId: number | null = null
    processWithAI: boolean = false
    platformSlug: string = 'openai'
    model: string = 'gpt-3.5-turbo'

    constructor(init?: Partial<ChatMessageRequest>) {
        super()
        Object.assign(this, init)
    }
}
