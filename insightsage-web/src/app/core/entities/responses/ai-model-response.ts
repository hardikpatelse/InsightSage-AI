import { ApiResponse } from './api-response'
import { AIModel } from '../ai-model'

/**
 * Response wrapper for AI Model operations
 * Based on the standard API response format
 */
export interface AIModelResponse extends ApiResponse {
    result: AIModel
}

/**
 * Response wrapper for AI Models list operations
 * Based on the standard API response format
 */
export interface AIModelsResponse extends ApiResponse {
    result: AIModel[]
}
