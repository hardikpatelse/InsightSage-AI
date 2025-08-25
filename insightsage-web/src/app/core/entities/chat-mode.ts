import { BaseEntity } from "./common/base-entity"

/**
 * Chat Mode Entity based on API specification
 * Represents different chat interaction modes (Chat, Code Review, Explain, Fix, etc.)
 */
export class ChatMode extends BaseEntity {
    name: string = ''
    value: string = ''
    iconName: string = ''
    description?: string = ''
    isActive?: boolean = true
    sortOrder?: number = 0
    capabilities?: string[] = []

    constructor(init?: Partial<ChatMode>) {
        super()
        Object.assign(this, init)
    }
}

/**
 * Search parameters for chat modes
 */
export interface ChatModeSearchParams {
    name?: string
    value?: string
    isActive?: boolean
}

/**
 * Chat Mode request for create/update operations
 */
export interface ChatModeRequest {
    id?: number
    name: string
    value: string
    iconName?: string
    description?: string
    isActive?: boolean
    sortOrder?: number
    capabilities?: string[]
}

// Type guards for runtime type checking
export function isChatMode(obj: any): obj is ChatMode {
    return obj && typeof obj.label === 'string' && typeof obj.value === 'string'
}

// Common Chat Mode Types
export enum ChatModeType {
    GENERAL_CHAT = 'chat',
    CODE_REVIEW = 'code-review',
    EXPLAIN = 'explain',
    FIX = 'fix',
    GENERATE = 'generate',
    OPTIMIZE = 'optimize',
    TEST = 'test',
    DOCUMENT = 'document'
}