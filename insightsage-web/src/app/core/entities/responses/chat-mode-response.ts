import { ApiResponse } from './api-response'
import { ChatMode } from '../chat-mode'

/**
 * Response wrapper for Chat Mode operations
 * Based on the standard API response format
 */
export interface ChatModeResponse extends ApiResponse {
    result: ChatMode
}

/**
 * Response wrapper for Chat Modes list operations
 * Based on the standard API response format
 */
export interface ChatModesResponse extends ApiResponse {
    result: ChatMode[]
}
