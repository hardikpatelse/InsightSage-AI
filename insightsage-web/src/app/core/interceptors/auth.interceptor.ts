import { Injectable } from '@angular/core'
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http'
import { Observable, from, switchMap, catchError, firstValueFrom } from 'rxjs'
import { MsalService } from '@azure/msal-angular'
import { environment } from '../../../environments/environment'

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

    constructor(private msalService: MsalService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        // Skip adding auth header for certain URLs
        if (this.shouldSkipAuth(request.url)) {
            return next.handle(request)
        }

        // Get the active account
        const account = this.msalService.instance.getActiveAccount()
        if (!account) {
            console.warn('No active account found for request:', request.url)
            // No active account, proceed without auth header
            return next.handle(request)
        }

        // For backend API calls, acquire token with default scopes
        const scopes = this.getScopesForUrl(request.url)

        console.log('Acquiring token for backend API:', request.url, 'with scopes:', scopes)

        // Acquire token silently and add to request
        return from(
            firstValueFrom(this.msalService.acquireTokenSilent({
                scopes: scopes,
                account: account
            }))
        ).pipe(
            switchMap(result => {
                if (!result || !result.accessToken) {
                    console.error('No ID token received for request:', request.url)
                    return next.handle(request)
                }
                console.log('Token acquired successfully:', result)
                console.log('Token acquired successfully for backend API:', request.url)
                // Clone the request and add the Authorization header
                const authRequest = request.clone({
                    setHeaders: {
                        'Authorization': `Bearer ${result.accessToken}`
                    }
                })
                return next.handle(authRequest)
            }),
            catchError(error => {
                console.error('Failed to acquire token for backend API:', request.url, error)
                // Proceed without auth header for backend APIs
                return next.handle(request)
            })
        )
    }

    /**
     * Determine if authentication should be skipped for this URL
     */
    private shouldSkipAuth(url: string): boolean {
        // Skip auth for Microsoft Graph API calls (they handle their own auth)
        if (url.includes('graph.microsoft.com')) {
            return true
        }

        // Skip auth for public endpoints (add your public API endpoints here)
        const publicEndpoints = [
            '/api/public',
            '/api/health',
            '/api/version'
        ]

        return publicEndpoints.some(endpoint => url.includes(endpoint))
    }

    /**
     * Get appropriate scopes based on the URL being accessed
     */
    private getScopesForUrl(url: string): string[] {
        // For your backend APIs, you might need different scopes
        // Add your custom scopes here based on the endpoint
        if (url.includes('your-api-domain.com')) {
            return ['api://your-app-id/access_as_user']
        }

        if (url.includes('localhost:7038') || url.includes('localhost:7039') || url.includes('your-api-domain.com')) {
            return [`api://${environment.msal.clientId}/access_as_user`] // Your API scope
        }

        // Check if this is your main backend API
        if (url.includes(this.getBackendDomain())) {
            return ['user.read'] // Or your custom backend API scope
        }

        // Default scope for general API access
        return ['user.read']
    }

    /**
     * Get the backend domain from environment or configuration
     */
    private getBackendDomain(): string {
        return environment.backendDomain // Update this to match your backend API domain
    }
}






// import { Injectable } from '@angular/core'
// import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http'
// import { Observable, from, switchMap, catchError, firstValueFrom } from 'rxjs'
// import { MsalService } from '@azure/msal-angular'
// import { environment } from '../../../environments/environment'

// @Injectable()
// export class AuthInterceptor implements HttpInterceptor {

//     constructor(private msalService: MsalService) { }

//     intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
//         // Skip adding auth header for certain URLs
//         if (this.shouldSkipAuth(request.url)) {
//             return next.handle(request)
//         }

//         // Get the active account
//         const account = this.msalService.instance.getActiveAccount()
//         if (!account) {
//             console.warn('No active account found for request:', request.url)
//             return next.handle(request)
//         }

//         // For backend API calls, use ID token temporarily until API scope is configured
//         const scopes = this.getScopesForUrl(request.url)

//         console.log('Acquiring token for backend API:', request.url, 'with scopes:', scopes)

//         // Acquire token silently and add to request
//         return from(
//             firstValueFrom(this.msalService.acquireTokenSilent({
//                 scopes: scopes,
//                 account: account
//             }))
//         ).pipe(
//             switchMap(result => {
//                 if (!result) {
//                     console.error('No token result received for request:', request.url)
//                     return next.handle(request)
//                 }

//                 // TEMPORARY FIX: Use ID token for backend APIs until proper API scope is configured
//                 const token = result.idToken || result.accessToken

//                 if (!token) {
//                     console.error('No token received for request:', request.url)
//                     return next.handle(request)
//                 }

//                 console.log('Token acquired successfully for backend API:', request.url)
//                 console.log('Using token type:', result.idToken ? 'ID Token' : 'Access Token')

//                 // Clone the request and add the Authorization header
//                 const authRequest = request.clone({
//                     setHeaders: {
//                         'Authorization': `Bearer ${token}`
//                     }
//                 })
//                 return next.handle(authRequest)
//             }),
//             catchError(error => {
//                 console.error('Failed to acquire token for backend API:', request.url, error)
//                 return next.handle(request)
//             })
//         )
//     }

//     /**
//      * Determine if authentication should be skipped for this URL
//      */
//     private shouldSkipAuth(url: string): boolean {
//         // Skip auth for Microsoft Graph API calls (they handle their own auth)
//         if (url.includes('graph.microsoft.com')) {
//             return true
//         }

//         // Skip auth for public endpoints (add your public API endpoints here)
//         const publicEndpoints = [
//             '/api/public',
//             '/api/health',
//             '/api/version',
//             '/api/user/login' // Allow login endpoint without auth
//         ]

//         return publicEndpoints.some(endpoint => url.includes(endpoint))
//     }

//     /**
//      * Get appropriate scopes based on the URL being accessed
//      */
//     private getScopesForUrl(url: string): string[] {
//         // For your backend APIs, use basic Microsoft Graph scopes
//         // This avoids the "resource not found" error
//         if (url.includes('localhost:7038') || url.includes('localhost:7039')) {
//             // Use basic scopes that don't require custom API registration
//             return ['openid', 'profile', 'email']
//         }

//         // Default scope for general API access
//         return ['openid', 'profile', 'email']
//     }

//     /**
//      * Get the backend domain from environment or configuration
//      */
//     private getBackendDomain(): string {
//         return environment.backendDomain || 'localhost'
//     }
// }