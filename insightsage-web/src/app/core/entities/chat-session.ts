import { BaseEntity } from "./common/base-entity"

// Chat Session Entity Interfaces based on Swagger API specification
export class ChatSession extends BaseEntity {
    name: string = ''
    uniqueIdentity: string = ''
    userId: string = ''

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
