import { BaseEntity } from './common/base-entity'

/**
 * Chat Message entity that matches the API specification
 */
export class ChatMessage extends BaseEntity {
    chatSessionId: number = 0
    role: string | null = null
    content: string | null = null

    constructor(init?: Partial<ChatMessage>) {
        super()
        Object.assign(this, init)
    }
}

