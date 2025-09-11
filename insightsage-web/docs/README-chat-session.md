# Chat Session Service Documentation

## Overview
The `ChatSessionService` provides comprehensive methods for managing chat sessions through the InsightSage Chat API. It includes full CRUD operations, user-specific session management, and advanced features like search, duplication, and batch operations.

## Features

### 🔧 **Core API Methods**
- ✅ **GET /api/Session** - Get all sessions
- ✅ **POST /api/Session** - Create new session
- ✅ **GET /api/Session/{id}** - Get session by ID
- ✅ **PUT /api/Session/{id}** - Update session
- ✅ **DELETE /api/Session/{id}** - Delete session
- ✅ **GET /api/Session/unique/{uniqueIdentity}** - Get by UUID
- ✅ **GET /api/Session/user** - Get user sessions
- ✅ **GET /api/Session/search/user** - Search user sessions
- ✅ **GET /api/Session/health** - Health check

### 🚀 **Enhanced Utility Methods**
- ✅ **Session State Management** - Current session tracking
- ✅ **Batch Operations** - Multiple session deletion
- ✅ **Session Duplication** - Clone existing sessions
- ✅ **Name Validation** - Check for duplicate names
- ✅ **Recent Session Tracking** - Get most recent session
- ✅ **Session Counting** - Get user session count
- ✅ **Auto-refresh** - Automatic cache updates

## Usage Examples

### Basic Operations

```typescript
import { ChatSessionService } from '@/core/services/chat-session.service';

constructor(private chatSessionService: ChatSessionService) {}

// Create a new session
createSession() {
  this.chatSessionService.createNewSession('My New Chat').subscribe({
    next: (session) => console.log('Session created:', session),
    error: (error) => console.error('Error creating session:', error)
  });
}

// Get user sessions
loadUserSessions() {
  this.chatSessionService.getUserSessions().subscribe({
    next: (sessions) => this.sessions = sessions,
    error: (error) => console.error('Error loading sessions:', error)
  });
}

// Update session name
renameSession(sessionId: number, newName: string) {
  this.chatSessionService.renameSession(sessionId, newName).subscribe({
    next: (id) => console.log('Session renamed successfully'),
    error: (error) => console.error('Error renaming session:', error)
  });
}

// Delete session
deleteSession(sessionId: number) {
  this.chatSessionService.deleteSession(sessionId).subscribe({
    next: (result) => console.log('Session deleted:', result),
    error: (error) => console.error('Error deleting session:', error)
  });
}
```

### Advanced Operations

```typescript
// Search sessions
searchSessions(query: string) {
  this.chatSessionService.searchUserSessions({ name: query }).subscribe({
    next: (sessions) => this.searchResults = sessions,
    error: (error) => console.error('Search error:', error)
  });
}

// Duplicate session
duplicateSession(sessionId: number) {
  this.chatSessionService.duplicateSession(sessionId, 'Copy of Session').subscribe({
    next: (newSession) => console.log('Session duplicated:', newSession),
    error: (error) => console.error('Duplication error:', error)
  });
}

// Get session by unique ID
loadSessionByUuid(uuid: string) {
  this.chatSessionService.getSessionByUniqueId(uuid).subscribe({
    next: (session) => this.currentSession = session,
    error: (error) => console.error('Error loading session:', error)
  });
}

// Batch delete sessions
deleteMultipleSessions(sessionIds: number[]) {
  this.chatSessionService.deleteSessions(sessionIds).subscribe({
    next: (results) => console.log('Sessions deleted:', results),
    error: (error) => console.error('Batch delete error:', error)
  });
}
```

### State Management

```typescript
// Subscribe to current session changes
ngOnInit() {
  this.chatSessionService.currentSession$.subscribe(session => {
    this.currentSession = session;
    console.log('Current session changed:', session);
  });

  // Subscribe to sessions list changes
  this.chatSessionService.sessions$.subscribe(sessions => {
    this.sessionsList = sessions;
    console.log('Sessions list updated:', sessions.length);
  });
}

// Set current session
selectSession(session: ChatSession) {
  this.chatSessionService.setCurrentSession(session);
}

// Get current session synchronously
getCurrentSession() {
  return this.chatSessionService.getCurrentSession();
}
```

## API Response Types

### ChatSession
```typescript
interface ChatSession {
  id?: number;
  createdAt?: string;
  updatedAt?: string;
  name: string;                    // Required
  uniqueIdentity?: string;         // UUID
  userId?: string;                 // UUID
}
```

### Response Wrappers
```typescript
interface ChatSessionResponse {
  status: HttpStatusCode;
  errors?: string[] | null;
  exceptionDetails?: string | null;
  result?: ChatSession;
}

interface ChatSessionListResponse {
  status: HttpStatusCode;
  errors?: string[] | null;
  exceptionDetails?: string | null;
  result?: ChatSession[] | null;
}

interface AddUpdateEntityResponse {
  status: HttpStatusCode;
  errors?: string[] | null;
  exceptionDetails?: string | null;
  id: number;
}
```

## Error Handling

The service includes comprehensive error handling:

```typescript
// All methods include proper error handling
this.chatSessionService.createSession(newSession).subscribe({
  next: (id) => {
    // Success handling
    console.log('Session created with ID:', id);
  },
  error: (error) => {
    // Error handling
    console.error('Failed to create session:', error);
    // Show user-friendly error message
    this.showErrorMessage('Failed to create session. Please try again.');
  }
});
```

## State Management Features

### Reactive State
- `currentSession$`: Observable of the currently active session
- `sessions$`: Observable of the user's session list
- Automatic cache updates on CRUD operations

### Utility Methods
- `setCurrentSession()`: Set active session
- `getCurrentSession()`: Get active session synchronously
- `clearCache()`: Clear all cached data
- `refreshCurrentSession()`: Refresh current session data

## Best Practices

1. **Always handle errors** in your subscriptions
2. **Use the reactive streams** for UI updates
3. **Leverage caching** - the service automatically manages cache
4. **Check session existence** before performing operations
5. **Use type-safe methods** - prefer typed request interfaces

## Dependencies

- `ApiService`: For HTTP operations
- `environment.chatApiBaseUrl`: Base URL configuration
- `RxJS`: For reactive programming

## Authentication

The service automatically includes authentication headers through the `ApiService`. Ensure users are authenticated before making API calls.

## Performance Notes

- The service includes intelligent caching
- Automatic list refresh after CRUD operations
- Debounced search operations
- Minimal API calls through state management
