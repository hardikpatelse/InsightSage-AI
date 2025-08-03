import { Injectable } from '@angular/core'
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http'
import { Observable, throwError } from 'rxjs'
import { catchError, retry } from 'rxjs/operators'
import { Router } from '@angular/router'
import { environment } from '../../../environments/environment'
import { NotificationService } from '../services/notification.service'

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

    constructor(
        private router: Router,
        private notificationService: NotificationService
    ) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(request).pipe(
            // Retry failed requests once (except for certain error codes)
            retry({
                count: 1,
                delay: (error: HttpErrorResponse) => {
                    // Don't retry on client errors (4xx) or authentication errors
                    if (error.status >= 400 && error.status < 500) {
                        throw error
                    }
                    // Retry server errors (5xx) and network errors
                    return throwError(() => error)
                }
            }),
            catchError((error: HttpErrorResponse) => {
                let errorMessage = 'An unexpected error occurred'
                let userFriendlyMessage = 'Something went wrong. Please try again.'

                if (error.error instanceof ErrorEvent) {
                    // Client-side or network error
                    errorMessage = `Network Error: ${error.error.message}`
                    userFriendlyMessage = 'Network connection problem. Please check your internet connection.'
                } else {
                    // Server-side error
                    switch (error.status) {
                        case 400:
                            errorMessage = 'Bad Request: ' + (error.error?.message || 'Invalid request')
                            userFriendlyMessage = 'Invalid request. Please check your input and try again.'
                            break
                        case 401:
                            errorMessage = 'Unauthorized: Authentication required'
                            userFriendlyMessage = 'Your session has expired. Please log in again.'
                            this.handleAuthenticationError()
                            break
                        case 403:
                            errorMessage = 'Forbidden: Access denied'
                            userFriendlyMessage = 'You do not have permission to perform this action.'
                            break
                        case 404:
                            errorMessage = 'Not Found: Resource not available'
                            userFriendlyMessage = 'The requested resource was not found.'
                            break
                        case 409:
                            errorMessage = 'Conflict: ' + (error.error?.message || 'Data conflict')
                            userFriendlyMessage = 'There was a conflict with your request. Please refresh and try again.'
                            break
                        case 422:
                            errorMessage = 'Validation Error: ' + (error.error?.message || 'Invalid data')
                            userFriendlyMessage = 'Please check your input and correct any errors.'
                            break
                        case 429:
                            errorMessage = 'Too Many Requests: Rate limit exceeded'
                            userFriendlyMessage = 'Too many requests. Please wait a moment and try again.'
                            break
                        case 500:
                            errorMessage = 'Internal Server Error'
                            userFriendlyMessage = 'Server error. Please try again later.'
                            break
                        case 502:
                            errorMessage = 'Bad Gateway'
                            userFriendlyMessage = 'Service temporarily unavailable. Please try again later.'
                            break
                        case 503:
                            errorMessage = 'Service Unavailable'
                            userFriendlyMessage = 'Service is temporarily down. Please try again later.'
                            break
                        case 504:
                            errorMessage = 'Gateway Timeout'
                            userFriendlyMessage = 'Request timed out. Please try again.'
                            break
                        default:
                            if (error.status >= 500) {
                                errorMessage = `Server Error (${error.status}): ${error.error?.message || error.message}`
                                userFriendlyMessage = 'Server error. Please try again later.'
                            } else {
                                errorMessage = `HTTP Error (${error.status}): ${error.error?.message || error.message}`
                                userFriendlyMessage = 'An error occurred. Please try again.'
                            }
                    }
                }

                // Log the detailed error for debugging
                console.error('HTTP Error Details:', {
                    status: error.status,
                    statusText: error.statusText,
                    url: error.url,
                    message: errorMessage,
                    error: error.error,
                    timestamp: new Date().toISOString()
                })

                // Show user-friendly notification (you can integrate with a toast service here)
                this.showErrorNotification(userFriendlyMessage, error.status)

                // Create enhanced error object with additional context
                const enhancedError = new HttpErrorResponse({
                    error: {
                        ...error.error,
                        userMessage: userFriendlyMessage,
                        timestamp: new Date().toISOString(),
                        requestUrl: error.url || undefined
                    },
                    headers: error.headers,
                    status: error.status,
                    statusText: error.statusText,
                    url: error.url || undefined
                })

                return throwError(() => enhancedError)
            })
        )
    }

    private handleAuthenticationError(): void {
        // Clear any stored authentication data
        localStorage.removeItem('msal.idtoken')
        localStorage.removeItem('msal.account.keys')

        // Redirect to login page
        this.router.navigate(['/login'])
    }

    private showErrorNotification(message: string, status: number): void {
        // Show user-friendly notification using the notification service
        if (status === 401) {
            this.notificationService.showError(message, 'Authentication Required')
        } else if (status >= 500) {
            this.notificationService.showError(message, 'Server Error')
        } else if (status >= 400) {
            this.notificationService.showError(message, 'Request Error')
        } else {
            this.notificationService.showError(message, 'Network Error')
        }

        // Log detailed error for debugging
        console.warn(`API Error (${status}): ${message}`)

        // For development, you might want to show an alert for server errors (remove in production)
        if (!environment.production && status >= 500) {
            setTimeout(() => {
                console.error(`Development Alert: ${message}`)
            }, 100)
        }
    }
}
