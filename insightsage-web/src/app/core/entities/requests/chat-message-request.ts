import { ChatMessage } from "../chat-message"
import { BaseEntity } from "../common/base-entity"

export class ChatMessageRequest extends BaseEntity {
    message: ChatMessage = new ChatMessage()
    aiModelId: number | null = null
    chatModeId: number | null = null

    constructor(init?: Partial<ChatMessageRequest>) {
        super()
        Object.assign(this, init)
    }
}
