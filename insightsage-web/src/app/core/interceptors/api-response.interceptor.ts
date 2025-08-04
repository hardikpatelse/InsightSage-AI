import { Injectable } from '@angular/core'
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpResponse, HttpErrorResponse } from '@angular/common/http'
import { Observable, throwError } from 'rxjs'
import { map, catchError } from 'rxjs/operators'
import { ApiResponse, isApiSuccess, isApiError } from '../entities/api-response'
import { NotificationService } from '../services/notification.service'
import { environment } from '../../../environments/environment'

@Injectable()
export class ApiResponseInterceptor implements HttpInterceptor {

    constructor(private notificationService: NotificationService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(request).pipe(
            map((event: HttpEvent<any>) => {
                // Only process HTTP responses
                if (event instanceof HttpResponse) {
                    const response = event.body

                    // Check if this is an API response that follows our standard format
                    if (this.isStandardApiResponse(response)) {
                        // Check if client wants the full response (special header)
                        if (request.headers.has('X-Full-Response')) {
                            return event // Return full response as-is
                        }

                        return this.handleStandardApiResponse(event, response)
                    }
                }
                return event
            }),
            catchError((error: HttpErrorResponse) => {
                // Handle cases where the server returns an error with our standard format
                if (error.error && this.isStandardApiResponse(error.error)) {
                    return this.handleApiErrorResponse(error)
                }
                // Let other interceptors handle non-standard errors
                return throwError(() => error)
            })
        )
    }

    /**
     * Check if the response follows our standard API format
     */
    private isStandardApiResponse(response: any): response is ApiResponse {
        return response &&
            typeof response === 'object' &&
            response.hasOwnProperty('result') &&
            response.hasOwnProperty('status') &&
            response.hasOwnProperty('errors') &&
            response.hasOwnProperty('exceptionDetails') &&
            typeof response.status === 'number' &&
            Array.isArray(response.errors)
    }

    /**
     * Handle successful HTTP responses with our standard API format
     */
    private handleStandardApiResponse(event: HttpResponse<any>, apiResponse: ApiResponse): HttpResponse<any> {
        if (isApiSuccess(apiResponse)) {
            // Success: return only the result data
            console.log('API Success:', {
                url: event.url,
                status: apiResponse.status,
                resultType: typeof apiResponse.result
            })

            return event.clone({
                body: apiResponse.result
            })
        } else if (isApiError(apiResponse)) {
            // API returned an error in success HTTP status
            this.handleApiError(apiResponse, event.url || 'Unknown URL')

            // Create an HttpErrorResponse to maintain error handling flow
            const errorResponse = new HttpErrorResponse({
                error: {
                    apiErrors: apiResponse.errors,
                    exceptionDetails: apiResponse.exceptionDetails,
                    originalStatus: apiResponse.status
                },
                status: apiResponse.status,
                statusText: 'API Error',
                url: event.url || undefined
            })

            throw errorResponse
        }

        // Fallback: return original response
        return event
    }

    /**
     * Handle HTTP error responses that contain our standard API format
     */
    private handleApiErrorResponse(error: HttpErrorResponse): Observable<never> {
        const apiResponse: ApiResponse = error.error

        this.handleApiError(apiResponse, error.url || 'Unknown URL')

        // Create enhanced error with API details
        const enhancedError = new HttpErrorResponse({
            error: {
                ...error.error,
                apiErrors: apiResponse.errors,
                exceptionDetails: apiResponse.exceptionDetails,
                originalStatus: apiResponse.status
            },
            headers: error.headers,
            status: error.status,
            statusText: error.statusText,
            url: error.url || undefined
        })

        return throwError(() => enhancedError)
    }

    /**
     * Handle API-level errors and show appropriate notifications
     */
    private handleApiError(apiResponse: ApiResponse, url: string): void {
        const errors = apiResponse.errors || []
        const status = apiResponse.status

        // Log detailed error information
        console.error('API Error Response:', {
            url,
            status,
            errors,
            exceptionDetails: apiResponse.exceptionDetails,
            timestamp: new Date().toISOString()
        })

        // Show user-friendly notification based on error type
        if (errors.length > 0) {
            // Show the first error message to the user
            const primaryError = errors[0]
            this.showApiErrorNotification(primaryError, status)

            // Log additional errors to console if there are multiple
            if (errors.length > 1) {
                console.warn('Additional API errors:', errors.slice(1))
            }
        } else {
            // Fallback message if no specific errors provided
            this.showApiErrorNotification('An unexpected error occurred', status)
        }

        // Show exception details in development mode
        if (!environment.production && apiResponse.exceptionDetails) {
            console.error('Exception Details:', apiResponse.exceptionDetails)
        }
    }

    /**
     * Show appropriate notification based on error status and message
     */
    private showApiErrorNotification(errorMessage: string, status: number): void {
        let title = 'Error'

        // Customize title based on status code
        switch (status) {
            case 400:
                title = 'Invalid Request'
                break
            case 401:
                title = 'Authentication Required'
                break
            case 403:
                title = 'Access Denied'
                break
            case 404:
                title = 'Not Found'
                break
            case 409:
                title = 'Conflict'
                break
            case 422:
                title = 'Validation Error'
                break
            case 429:
                title = 'Too Many Requests'
                break
            case 500:
                title = 'Server Error'
                break
            default:
                if (status >= 500) {
                    title = 'Server Error'
                } else if (status >= 400) {
                    title = 'Request Error'
                } else {
                    title = 'Unexpected Error'
                }
        }

        // Show notification with appropriate severity
        if (status >= 500) {
            this.notificationService.showError(errorMessage, title)
        } else if (status === 401) {
            this.notificationService.showWarning(errorMessage, title)
        } else if (status >= 400) {
            this.notificationService.showWarning(errorMessage, title)
        } else {
            this.notificationService.showError(errorMessage, title)
        }
    }
}
