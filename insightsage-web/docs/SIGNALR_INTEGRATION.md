# SignalR Chat Integration Implementation

## Overview

This implementation integrates Microsoft SignalR real-time communication into the InsightSage AI Chat application, enabling immediate user feedback and real-time AI response delivery.

## Architecture

### 1. SignalR Hub Service (`ChatHubService`)

**File:** `src/app/core/services/chat-hub.service.ts`

**Key Features:**
- Automatic connection management with retry logic
- Azure AD token-based authentication
- Real-time event handling for AI processing
- Session-based communication

**Events Handled:**
- `AIProcessingStarted` - Shows typing indicator
- `AIResponseReceived` - Displays AI response in real-time
- `AIProcessingError` - Handles AI processing failures

**Methods:**
```typescript
// Connection Management
await chatHubService.connect()
await chatHubService.disconnect()

// Session Management
await chatHubService.joinSession(sessionId)
await chatHubService.leaveSession(sessionId)

// State Monitoring
chatHubService.isConnected
chatHubService.connectionState$
```

### 2. Chat Component Integration

**File:** `src/app/pages/chat/chat.ts`

**Enhanced Features:**
- Real-time connection status indicator
- AI typing indicator
- Automatic session joining when navigating to chat sessions
- Direct API communication for message sending

**Key Properties:**
```typescript
isSignalRConnected: boolean = false
isAITyping: boolean = false
```

**Event Handlers:**
- `handleAIResponse()` - Processes incoming AI responses
- `handleAIError()` - Handles AI processing errors
- `joinSignalRSession()` - Joins session for real-time updates
- `leaveSignalRSession()` - Leaves session when switching chats

### 3. UI Enhancements

**File:** `src/app/pages/chat/chat.html`

**Added Elements:**
```html
<!-- SignalR Connection Status -->
<div class="flex align-items-center gap-1">
    <i [class]="isSignalRConnected ? 'pi pi-circle-fill text-green-500' : 'pi pi-circle-fill text-red-500'"></i>
    <small [class]="isSignalRConnected ? 'text-green-600' : 'text-red-600'">
        {{ isSignalRConnected ? 'Real-time connected' : 'Real-time disconnected' }}
    </small>
</div>

<!-- AI Typing Indicator -->
<div class="flex align-items-center gap-1" *ngIf="isAITyping">
    <i class="pi pi-spin pi-spinner text-primary text-sm"></i>
    <small class="text-primary font-medium">AI is thinking...</small>
</div>
```

## Message Flow

### 1. User Sends Message
```
User types message → sendMessage() → sendMessageWithAI() → API call to /message/with-ai
```

### 2. Real-time AI Processing
```
Backend processes → SignalR events → Frontend updates UI → User sees real-time feedback
```

### 3. Event Sequence
1. **User Message Sent**: Input cleared, user message appears immediately
2. **AIProcessingStarted**: Typing indicator shows "AI is thinking..."
3. **AIResponseReceived**: Typing indicator hides, AI response appears
4. **Error Handling**: If processing fails, error message shown

## Configuration

### Environment Setup
```typescript
// src/environments/environment.ts
export const environment = {
    chatApiBaseUrl: 'https://localhost:7039/api',
    // ... other config
}
```

### SignalR Hub URL
The service automatically constructs the hub URL:
```typescript
// Example: https://localhost:7039/chat-hub
const hubUrl = `${environment.chatApiBaseUrl.replace('/api', '')}/chat-hub`
```

## Authentication

### Token Management
- Uses same MSAL configuration as other services
- Automatically acquires tokens with scopes: `['openid', 'profile', 'email']`
- Handles token refresh automatically
- Fallback to ID token if access token unavailable

### Connection Authentication
```typescript
this.connection = new HubConnectionBuilder()
    .withUrl(hubUrl, {
        accessTokenFactory: () => accessToken // Dynamic token provider
    })
    .build()
```

## Error Handling

### Connection Failures
- Automatic reconnection with exponential backoff: `[0, 2000, 10000, 30000]`
- Connection state monitoring with visual indicators
- Graceful degradation when SignalR unavailable

### Message Sending Failures
- User message restored to input field
- Error logging with detailed information
- API fallback mechanisms

### AI Processing Errors
- Real-time error notifications via SignalR
- Typing indicator removal
- Message history refresh to ensure UI sync

## Performance Benefits

### Immediate Feedback
- ✅ User sees their message instantly
- ✅ Real-time typing indicators
- ✅ Connection status awareness

### Non-blocking UI
- ✅ UI remains responsive during AI processing
- ✅ Multiple concurrent conversations supported
- ✅ Background token refresh

### Scalability
- ✅ Session-based communication reduces unnecessary traffic
- ✅ Automatic reconnection handles network interruptions
- ✅ Efficient message routing via SignalR groups

## Testing the Integration

### 1. Connection Status
- Navigate to chat page
- Check console for: `"Connected to InsightSage Chat Hub"`
- Verify green connection indicator in UI

### 2. Real-time Messaging
- Send a message
- Observe typing indicator: "AI is thinking..."
- Watch for real-time AI response

### 3. Session Management
- Navigate between different chat sessions
- Console should show: `"Joined SignalR session: {sessionId}"`
- Verify real-time updates only for current session

### 4. Error Handling
- Disconnect from internet briefly
- Observe reconnection attempts
- Check that UI remains functional

## Console Logging

### Successful Flow
```
AuthInterceptor processing request: https://localhost:7039/chat-hub/negotiate
Connected to InsightSage Chat Hub
SignalR connection state: Connected
Joined SignalR session: 123
AI processing started for session: 123
AI response received for session: 123
AI response received: Hello! How can I help you today?...
Token usage: 25 total tokens
```

### Error Scenarios
```
Failed to initialize SignalR: [Error Details]
SignalR connection closed [Reason]
AI processing error for session: 123 - API rate limit exceeded
Failed to join SignalR session 123: Connection not established
```

## Future Enhancements

### Planned Features
- **Message Status Indicators**: Sent, Delivered, Read
- **Typing Indicators**: Show when other users are typing
- **File Upload Progress**: Real-time upload status
- **Message Reactions**: Real-time reaction updates
- **Presence Indicators**: Online/offline user status

### Performance Improvements
- **Message Batching**: Group multiple events for better performance
- **Selective Updates**: Only update UI for relevant events
- **Compression**: Enable SignalR message compression
- **Connection Pooling**: Reuse connections across components

## Troubleshooting

### Common Issues

**1. Connection Fails**
- Check backend SignalR hub is running
- Verify authentication token is valid
- Check browser console for CORS errors

**2. Events Not Received**
- Verify session ID is correct
- Check if properly joined SignalR session
- Confirm backend is sending events to correct session

**3. UI Not Updating**
- Check Angular change detection
- Verify event subscriptions are not destroyed
- Confirm components are properly subscribed to observables

### Debug Commands
```typescript
// Check connection state
console.log('SignalR connected:', this.chatHubService.isConnected)

// Monitor events
this.chatHubService.aiResponseReceived$.subscribe(console.log)

// Test session joining
await this.chatHubService.joinSession('test-session-123')
```

## Dependencies

### Required Packages
```json
{
  "@microsoft/signalr": "^8.x.x",
  "@azure/msal-angular": "^4.x.x",
  "@azure/msal-browser": "^4.x.x"
}
```

### Angular Modules
- `CommonModule` - Basic Angular directives
- `FormsModule` - Template-driven forms support
- `ReactiveFormsModule` - Reactive forms (future use)

## Security Considerations

### Token Security
- Tokens are acquired dynamically, never stored
- Automatic token refresh before expiration
- Secure transmission over HTTPS/WSS

### Message Validation
- All messages validated on backend
- Session-based isolation
- User authorization checks per session

### Connection Security
- WSS (WebSocket Secure) in production
- Azure AD integration for user identity
- Rate limiting and throttling protection

This implementation provides a solid foundation for real-time chat functionality while maintaining security, performance, and user experience standards.
