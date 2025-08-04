# API Response Handling System

This document explains how the new standardized API response handling system works in the InsightSage application.

## Overview

All APIs now return responses in a standardized format:

```typescript
{
    "result": T,              // The actual data
    "status": number,         // HTTP status code (200 for success)
    "errors": string[],       // Array of error messages (empty for success)
    "exceptionDetails": string | null  // Detailed exception info (null for success)
}
```

## Key Components

### 1. ApiResponse Interface (`src/app/core/entities/api-response.ts`)

Defines the standard response format and provides type guards:

```typescript
interface ApiResponse<T = any> {
  result: T
  status: number
  errors: string[]
  exceptionDetails: string | null
}

// Type guards
isApiSuccess<T>(response: ApiResponse<T>): boolean
isApiError(response: ApiResponse): boolean
```

### 2. ApiResponseInterceptor (`src/app/core/interceptors/api-response.interceptor.ts`)

- **Automatically extracts the `result` field** from successful responses
- **Handles API-level errors** (status !== 200 or errors.length > 0)
- **Shows appropriate notifications** to users
- **Logs detailed error information** for debugging

### 3. ApiService (`src/app/core/services/api.service.ts`)

Provides convenient methods for making API calls:

```typescript
// Standard calls - automatically extract result
get<T>(baseUrl: string, endpoint: string): Observable<T>
post<T>(baseUrl: string, endpoint: string, data: any): Observable<T>

// Full response calls - get complete API response
getFullResponse<T>(baseUrl: string, endpoint: string): Observable<ApiResponse<T>>
postFullResponse<T>(baseUrl: string, endpoint: string, data: any): Observable<ApiResponse<T>>
```

## Usage Examples

### Standard Usage (Recommended)

For most cases, use the standard methods that automatically extract the result:

```typescript
// Before (manual handling)
this.http.post<ApiResponse<User>>('/api/user/login', userData).subscribe({
  next: (response) => {
    if (response.status === 200 && response.errors.length === 0) {
      const user = response.result; // Manual extraction
      console.log('User:', user);
    } else {
      // Manual error handling
      console.error('API errors:', response.errors);
    }
  }
});

// After (automatic handling)
this.apiService.post<User>('/api', 'user/login', userData).subscribe({
  next: (user) => {
    // Result is automatically extracted!
    console.log('User:', user);
  },
  error: (error) => {
    // Errors are automatically handled and shown to user
    console.error('Error:', error);
  }
});
```

### Full Response Access

When you need access to the complete response (status, errors, etc.):

```typescript
this.apiService.getFullResponse<User>('/api', 'user/profile').subscribe({
  next: (response: ApiResponse<User>) => {
    console.log('Status:', response.status);
    console.log('Errors:', response.errors);
    console.log('Result:', response.result);
    console.log('Exception:', response.exceptionDetails);
  }
});
```

## Error Handling

### Automatic Error Handling

The system automatically:

1. **Detects API errors** (status !== 200 or errors present)
2. **Shows user-friendly notifications** with appropriate severity
3. **Logs detailed error information** for debugging
4. **Converts API errors to HTTP errors** for consistent error handling

### Error Notification Examples

- **Status 400**: Shows warning notification with validation errors
- **Status 401**: Shows authentication required notification
- **Status 500**: Shows server error notification
- **API errors array**: Shows the first error message to the user

### Custom Error Handling

You can still handle errors manually if needed:

```typescript
this.apiService.post<User>('/api', 'user/login', userData).subscribe({
  next: (user) => {
    console.log('Success:', user);
  },
  error: (error: HttpErrorResponse) => {
    // Access API-specific error information
    if (error.error.apiErrors) {
      console.log('API Errors:', error.error.apiErrors);
    }
    if (error.error.exceptionDetails) {
      console.log('Exception:', error.error.exceptionDetails);
    }
    
    // Custom error handling
    this.handleCustomError(error);
  }
});
```

## Migration Guide

### From DataProviderService to ApiService

Replace your existing service calls:

```typescript
// Old way
this.dataProviderService.postData<User>(baseUrl, 'user/login', userData)

// New way  
this.apiService.post<User>(baseUrl, 'user/login', userData)
```

### Benefits of Migration

1. **Automatic result extraction** - no more manual `response.result`
2. **Automatic error handling** - no more manual status/error checking
3. **User notifications** - errors are automatically shown to users
4. **Better debugging** - detailed error logging
5. **Type safety** - better TypeScript support

## Configuration

The interceptors are automatically registered in `app.config.ts`:

```typescript
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

## Testing

The system includes comprehensive error handling for various scenarios:

- **Network errors** - Connection problems
- **HTTP errors** - 4xx and 5xx status codes  
- **API errors** - Application-level errors in success responses
- **Validation errors** - Field-level validation failures
- **Server errors** - Unexpected server issues

## Best Practices

1. **Use ApiService** instead of direct HttpClient calls
2. **Let the interceptor handle errors** - don't duplicate error handling
3. **Use standard calls** for most scenarios (automatic result extraction)
4. **Use full response calls** only when you need access to status/errors
5. **Log errors appropriately** - the system handles user notifications
6. **Test error scenarios** - ensure proper error handling in your components

## Example Implementation

See `src/app/features/api-demo/api-demo.component.ts` for a complete example demonstrating:

- Standard API calls with automatic result extraction
- Full response API calls with complete response access
- Error handling and notification display
- Proper loading state management
