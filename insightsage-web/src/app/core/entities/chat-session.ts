import { AIModel } from "./ai-model"
import { ChatMode } from "./chat-mode"
import { BaseEntity } from "./common/base-entity"

// Chat Session Entity Interfaces based on Swagger API specification
export class ChatSession extends BaseEntity {
    name: string = ''
    uniqueIdentity: string = ''
    userId: string = ''
    aiModelId?: number | null = null
    chatModeId?: number | null = null
    aiModel?: AIModel | null = null
    chatMode?: ChatMode | null = null


    constructor(init?: Partial<ChatSession>) {
        super()
        Object.assign(this, init)
    }
}

export interface ChatSessionSearchParams {
    name?: string
}


// Type guards for runtime type checking
export function isChatSession(obj: any): obj is ChatSession {
    return obj && typeof obj.name === 'string'
}
