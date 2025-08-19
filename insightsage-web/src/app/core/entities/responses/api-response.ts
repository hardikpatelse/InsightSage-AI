import { BaseResponse } from "./base-response"

/**
 * Standard API response format for all backend endpoints
 */
export class ApiResponse<T = any> extends BaseResponse {
    /**
     * The actual data returned by the API
     */
    result!: T
}

/**
 * Helper type for successful API responses
 */
export interface ApiSuccessResponse<T = any> extends ApiResponse<T> {
    status: 200
    errors: []
    exceptionDetails: null
}

/**
 * Helper type for error API responses
 */
export interface ApiErrorResponse extends ApiResponse<null> {
    result: null
    status: number // Non-200 status codes
    errors: string[]
    exceptionDetails: string | null
}

/**
 * Type guard to check if response is successful
 */
export function isApiSuccess<T>(response: ApiResponse<T>): response is ApiSuccessResponse<T> {
    return response.status === 200 && response.errors?.length === 0 && (response.result !== undefined || response.result !== null)
}

/**
 * Type guard to check if response has errors
 */
export function isApiError(response: ApiResponse): response is ApiErrorResponse {
    return response.errors !== undefined && response.errors !== null && response.errors.length > 0 && response.errors.every((error) => error !== '')
}
