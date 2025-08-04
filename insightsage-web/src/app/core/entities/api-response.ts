/**
 * Standard API response format for all backend endpoints
 */
export interface ApiResponse<T = any> {
    /**
     * The actual data returned by the API
     */
    result: T

    /**
     * HTTP status code (200 for success, others for various error types)
     */
    status: number

    /**
     * Array of error messages (empty for successful requests)
     */
    errors: string[]

    /**
     * Detailed exception information (null for successful requests)
     */
    exceptionDetails: string | null
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
    return response.status === 200 && response.errors.length === 0
}

/**
 * Type guard to check if response has errors
 */
export function isApiError(response: ApiResponse): response is ApiErrorResponse {
    return response.status !== 200 || response.errors.length > 0
}
