# Authorization Interceptor

The Authorization Interceptor automatically adds Bearer tokens to HTTP requests, eliminating the need to manually add Authorization headers in each service call.

## Overview

The `AuthInterceptor` automatically:
- Acquires access tokens using MSAL
- Adds `Authorization: Bearer {token}` headers to requests
- Handles different scopes based on the target URL
- Gracefully handles token acquisition failures

## How It Works

### 1. Automatic Token Acquisition

For each outgoing HTTP request, the interceptor:
1. Checks if the URL requires authentication
2. Gets the active MSAL account
3. Acquires the appropriate access token silently
4. Adds the `Authorization` header to the request

### 2. Smart Scope Selection

The interceptor automatically selects the correct scopes based on the target URL:

```typescript
// Microsoft Graph API calls
if (url.includes('graph.microsoft.com')) {
    return ['user.read']
}

// Your backend API calls
if (url.includes('your-api-domain.com')) {
    return ['api://your-app-id/access_as_user']
}

// Default scope
return ['user.read']
```

### 3. Public Endpoint Handling

Some endpoints don't require authentication and are automatically skipped:

```typescript
const publicEndpoints = [
    '/api/public',
    '/api/health',
    '/api/version'
]
```

## Usage

### Before (Manual Header Management)

```typescript
// Old way - manual token acquisition and header setting
getUserProfile(): Observable<any> {
    return this.msalService.acquireTokenSilent({
        scopes: ['user.read'],
        account: this.msalService.instance.getActiveAccount()!
    }).pipe(
        switchMap(result => {
            const headers = { 'Authorization': `Bearer ${result.accessToken}` }
            return this.http.get('/api/user/profile', { headers })
        })
    )
}
```

### After (Automatic Header Management)

```typescript
// New way - interceptor handles everything automatically
getUserProfile(): Observable<any> {
    return this.http.get('/api/user/profile')
    // Authorization header is automatically added!
}
```

## Configuration

The interceptor is registered in `app.config.ts` and runs before other interceptors:

```typescript
{
  provide: HTTP_INTERCEPTORS,
  useClass: AuthInterceptor,
  multi: true
},
{
  provide: HTTP_INTERCEPTORS,
  useClass: ApiResponseInterceptor,
  multi: true
},
{
  provide: HTTP_INTERCEPTORS,
  useClass: ErrorInterceptor,
  multi: true
}
```

## Customization

### Adding Custom Scopes

Update the `getScopesForUrl` method to handle your specific API requirements:

```typescript
private getScopesForUrl(url: string): string[] {
    if (url.includes('graph.microsoft.com')) {
        return ['user.read']
    }
    
    if (url.includes('your-backend-api.com')) {
        return ['api://your-app-id/full-access']
    }
    
    if (url.includes('admin-api.com')) {
        return ['api://admin-app-id/admin-access']
    }
    
    return ['user.read']
}
```

### Adding Public Endpoints

Update the `shouldSkipAuth` method to skip authentication for public endpoints:

```typescript
private shouldSkipAuth(url: string): boolean {
    const publicEndpoints = [
        '/api/public',
        '/api/health',
        '/api/version',
        '/api/documentation'  // Add your public endpoints
    ]
    
    return publicEndpoints.some(endpoint => url.includes(endpoint))
}
```

## Error Handling

The interceptor gracefully handles token acquisition failures:

- If token acquisition fails, the request proceeds without the Authorization header
- Errors are logged to the console for debugging
- The request is not blocked, allowing the API to handle authentication errors appropriately

## Benefits

1. **Automatic Token Management**: No manual token acquisition needed
2. **Consistent Headers**: All requests get proper Authorization headers
3. **Scope Management**: Correct scopes are used for different APIs
4. **Error Resilience**: Graceful handling of token failures
5. **Clean Code**: Services become simpler without header management
6. **Centralized Auth Logic**: All authentication logic in one place

## Migration Guide

When migrating existing services:

1. **Remove manual token acquisition**: Delete `msalService.acquireTokenSilent()` calls
2. **Remove header creation**: Delete manual `Authorization` header creation
3. **Simplify HTTP calls**: Use direct HTTP client calls
4. **Update scopes**: Configure scopes in the interceptor instead of individual calls

### Example Migration

```typescript
// Before
loginUser(userData: any): Observable<User> {
    return this.msalService.acquireTokenSilent({
        scopes: ['api://app-id/access'],
        account: this.msalService.instance.getActiveAccount()!
    }).pipe(
        switchMap(result => {
            const headers = { 'Authorization': `Bearer ${result.accessToken}` }
            return this.apiService.post('/api', 'user/login', userData, headers)
        })
    )
}

// After
loginUser(userData: any): Observable<User> {
    return this.apiService.post('/api', 'user/login', userData)
    // Authorization header automatically added by interceptor!
}
```

## Testing

The interceptor can be tested by:

1. **Monitoring Network Requests**: Check that Authorization headers are present
2. **Testing Token Failures**: Verify graceful handling when tokens can't be acquired
3. **Public Endpoint Testing**: Confirm public endpoints don't get auth headers
4. **Scope Verification**: Ensure correct scopes are used for different APIs
