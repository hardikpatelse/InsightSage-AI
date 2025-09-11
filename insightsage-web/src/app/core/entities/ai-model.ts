import { BaseEntity } from "./common/base-entity"

/**
 * AI Model Entity based on API specification
 * Represents an AI model available in the system
 */
export class AIModel extends BaseEntity {
    name: string = ''
    value?: string = '' // Keeping original value field for backward compatibility
    description: string = ''
    provider?: string = ''
    modelVersion?: string = ''
    isActive?: boolean = true
    maxTokens?: number
    costPerToken?: number
    capabilities?: string[] = []
    modelType?: string = ''

    constructor(init?: Partial<AIModel>) {
        super()
        Object.assign(this, init)
    }
}

/**
 * Search parameters for AI models
 */
export interface AIModelSearchParams {
    name?: string
    provider?: string
    modelType?: string
    isActive?: boolean
}

/**
 * AI Model request for create/update operations
 */
export interface AIModelRequest {
    id?: number
    name: string
    value?: string
    description?: string
    provider?: string
    modelVersion?: string
    isActive?: boolean
    maxTokens?: number
    costPerToken?: number
    capabilities?: string[]
    modelType?: string
}

// Type guards for runtime type checking
export function isAIModel(obj: any): obj is AIModel {
    return obj && typeof obj.name === 'string'
}

// Common AI Model Types
export enum AIModelType {
    TEXT_GENERATION = 'text-generation',
    CHAT_COMPLETION = 'chat-completion',
    EMBEDDING = 'embedding',
    IMAGE_GENERATION = 'image-generation',
    SPEECH_TO_TEXT = 'speech-to-text',
    TEXT_TO_SPEECH = 'text-to-speech'
}

// Common AI Model Providers
export enum AIModelProvider {
    OPENAI = 'openai',
    ANTHROPIC = 'anthropic',
    GOOGLE = 'google',
    MICROSOFT = 'microsoft',
    META = 'meta',
    HUGGING_FACE = 'huggingface',
    COHERE = 'cohere',
    CUSTOM = 'custom'
}