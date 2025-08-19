import { ApiResponse } from "./api-response"
import { ChatMessage } from "../chat-message"

/**
 * API response wrapper for ChatMessage list
 */
export class ChatMessagesResponse extends ApiResponse<ChatMessage[]> {
}
