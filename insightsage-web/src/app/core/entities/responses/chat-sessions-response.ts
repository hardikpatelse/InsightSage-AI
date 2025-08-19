import { ChatSession } from "../chat-session"
import { ApiResponse } from "./api-response"

export interface ChatSessionListResponse extends ApiResponse<ChatSession[]> {
}