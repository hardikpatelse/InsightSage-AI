import { ApiResponse } from "./api-response"
import { ChatMessage } from "../chat-message"

/**
 * API response wrapper for single ChatMessage
 */
export class ChatMessageResponse extends ApiResponse<ChatMessage> {
}
