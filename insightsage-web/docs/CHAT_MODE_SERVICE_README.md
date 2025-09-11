# Chat Mode Service Documentation

## Overview

The `ChatModeService` provides comprehensive API integration for managing chat modes in the InsightSage application. It follows the same patterns as the `ChatSessionService` and `AIModelService`, integrating seamlessly with the existing API interceptor architecture.

## Features

- **CRUD Operations**: Create, read, update, and delete chat modes
- **Search & Filter**: Search modes by label, value, and active status
- **State Management**: Reactive state management with RxJS BehaviorSubjects
- **Error Handling**: Comprehensive error handling with user-friendly messages
- **Caching**: Automatic caching and cache invalidation
- **Loading States**: Built-in loading state management
- **Ordering**: Support for custom sort ordering
- **Capability-based Filtering**: Filter modes by capabilities

## API Endpoints

Based on the API endpoint provided:

```bash
curl -X 'GET' \
  'https://localhost:7039/api/ChatMode' \
  -H 'accept: text/plain' \
  -H 'Authorization: Bearer {token}'
```

## Usage Examples

### 1. Basic Usage - Get All Modes

```typescript
import { ChatModeService } from '@/core/services/chat-mode.service'
import { ChatMode } from '@/core/entities/chat-mode'

constructor(private chatModeService: ChatModeService) {}

ngOnInit() {
  // Get all chat modes
  this.chatModeService.getAllModes().subscribe({
    next: (modes: ChatMode[]) => {
      console.log('Chat Modes:', modes)
      this.chatModes = modes
    },
    error: (error) => {
      console.error('Failed to load modes:', error)
    }
  })
}
```

### 2. Get Active Modes Only

```typescript
// Get only active/enabled modes
this.chatModeService.getActiveModes().subscribe(modes => {
  this.activeModes = modes
  if (modes.length > 0) {
    this.selectedMode = modes[0] // Select first active mode
  }
})
```

### 3. Get Default Mode

```typescript
// Get the default mode (usually 'chat' or first active mode)
this.chatModeService.getDefaultMode().subscribe(mode => {
  if (mode) {
    this.selectedChatMode = mode
  }
})
```

### 4. Get Modes Sorted by Order

```typescript
// Get modes sorted by their display order
this.chatModeService.getModesSortedByOrder().subscribe(modes => {
  this.sortedModes = modes
})
```

### 5. Search Modes

```typescript
// Search modes by label
this.chatModeService.searchModes({ 
  label: 'Code',
  isActive: true 
}).subscribe(modes => {
  this.codeRelatedModes = modes
})

// Get mode by specific value
this.chatModeService.getModeByValue('code-review').subscribe(mode => {
  if (mode) {
    this.codeReviewMode = mode
  }
})
```

### 6. Create a New Mode

```typescript
import { ChatModeRequest } from '@/core/entities/chat-mode'

createNewMode() {
  const newMode: ChatModeRequest = {
    label: 'Debug',
    value: 'debug',
    icon: 'pi pi-bug',
    description: 'Debug and troubleshoot issues',
    isActive: true,
    sortOrder: 5,
    capabilities: ['debugging', 'analysis', 'troubleshooting']
  }

  this.chatModeService.createMode(newMode).subscribe({
    next: (modeId: number) => {
      console.log('Mode created with ID:', modeId)
      // Refresh the modes list (automatically handled by service)
    },
    error: (error) => {
      console.error('Failed to create mode:', error)
    }
  })
}
```

### 7. Update Mode Status

```typescript
// Toggle mode active/inactive status
toggleModeStatus(modeId: number, isActive: boolean) {
  this.chatModeService.toggleModeStatus(modeId, isActive).subscribe({
    next: () => {
      console.log('Mode status updated successfully')
    },
    error: (error) => {
      console.error('Failed to update mode status:', error)
    }
  })
}
```

### 8. Subscribe to Reactive State

```typescript
ngOnInit() {
  // Subscribe to the modes observable for reactive updates
  this.chatModeService.modes$.subscribe(modes => {
    this.chatModes = modes
  })

  // Subscribe to loading state
  this.chatModeService.loading$.subscribe(loading => {
    this.isLoading = loading
  })

  // Subscribe to current selected mode
  this.chatModeService.currentMode$.subscribe(mode => {
    this.selectedMode = mode
  })

  // Initial load
  this.chatModeService.getAllModes().subscribe()
}
```

### 9. Filter by Capabilities

```typescript
// Get modes that have specific capabilities
this.chatModeService.getModesByCapability('code-analysis').subscribe(modes => {
  this.codeAnalysisModes = modes
})
```

### 10. Update Sort Order

```typescript
// Update the display order of a mode
updateModeOrder(modeId: number, newOrder: number) {
  this.chatModeService.updateModeOrder(modeId, newOrder).subscribe({
    next: () => {
      console.log('Mode order updated')
      // Refresh sorted list
      this.loadSortedModes()
    },
    error: (error) => {
      console.error('Failed to update mode order:', error)
    }
  })
}
```

## Service Methods

### Core CRUD Methods
- `getAllModes()`: Get all chat modes
- `getModeById(id: number)`: Get specific mode by ID
- `createMode(mode: ChatModeRequest)`: Create new mode
- `updateMode(id: number, mode: ChatModeRequest)`: Update existing mode
- `deleteMode(id: number, modifiedBy?: number)`: Delete mode

### Search & Filter Methods
- `getActiveModes()`: Get only active modes
- `searchModes(params?: ChatModeSearchParams)`: Search with filters
- `getModeByValue(value: string)`: Get mode by value
- `getModesByCapability(capability: string)`: Filter by capability

### Utility Methods
- `setCurrentMode(mode: ChatMode | null)`: Set active mode
- `getCurrentMode()`: Get current active mode
- `isLoading()`: Check loading state
- `clearCache()`: Clear cached data
- `checkHealth()`: Health check endpoint
- `getDefaultMode()`: Get the default chat mode
- `getModesSortedByOrder()`: Get modes sorted by display order

### Batch Operations
- `deleteModes(ids: number[])`: Delete multiple modes
- `toggleModeStatus(id: number, status: boolean)`: Toggle active status
- `updateModeOrder(id: number, order: number)`: Update sort order

### Validation Methods
- `isModeLabelExists(label: string)`: Check if label exists
- `isModeValueExists(value: string)`: Check if value exists

## Error Handling

The service includes comprehensive error handling:

```typescript
this.chatModeService.getAllModes().subscribe({
  next: (modes) => {
    // Success handling
  },
  error: (error) => {
    // Error object contains:
    // - error.message: User-friendly error message
    // - error.error.apiErrors: Array of API-specific errors
    // - error.status: HTTP status code
    console.error('API Error:', error)
  }
})
```

## Integration with API Interceptor

The service works seamlessly with the existing `ApiResponseInterceptor`:

- **Automatic Unwrapping**: The interceptor automatically unwraps successful API responses
- **Error Enhancement**: Errors are enhanced with additional context
- **Loading States**: Managed automatically by the service
- **Type Safety**: Full TypeScript support with proper typing

## Chat Component Integration

The service is integrated into the chat component:

```typescript
// In chat.ts
export class Chat implements OnInit {
  chatModes: ChatMode[] = []
  selectedChatMode: ChatMode | null = null

  constructor(private chatModeService: ChatModeService) {}

  ngOnInit() {
    this.loadChatModes() // Loads modes from API with fallback
  }

  private loadChatModes() {
    this.chatModeService.getActiveModes().subscribe({
      next: (modes) => {
        this.chatModes = modes
        // Set default mode
        const defaultMode = modes.find(mode => mode.value === 'chat') || modes[0]
        this.selectedChatMode = defaultMode
      },
      error: (error) => {
        // Fallback to hardcoded modes
        this.setFallbackChatModes()
      }
    })
  }
}
```

## Mode Entity Structure

```typescript
export class ChatMode extends BaseEntity {
  label: string = ''           // Display name
  value: string = ''           // Unique identifier
  icon: string = ''            // Icon class (e.g., 'pi pi-comments')
  description?: string = ''    // Mode description
  isActive?: boolean = true    // Whether mode is enabled
  sortOrder?: number = 0       // Display order
  capabilities?: string[] = [] // Mode capabilities
}
```

## Common Chat Mode Values

The service includes an enum for common mode types:

```typescript
export enum ChatModeType {
  GENERAL_CHAT = 'chat',
  CODE_REVIEW = 'code-review',
  EXPLAIN = 'explain',
  FIX = 'fix',
  GENERATE = 'generate',
  OPTIMIZE = 'optimize',
  TEST = 'test',
  DOCUMENT = 'document'
}
```

## Environment Configuration

Ensure your environment files have the correct API base URL:

```typescript
// environment.ts
export const environment = {
  production: false,
  chatApiBaseUrl: 'https://localhost:7039/api'
}
```

## Best Practices

1. **Use Reactive Patterns**: Subscribe to observables for real-time updates
2. **Handle Loading States**: Show loading indicators using the `loading$` observable
3. **Error Handling**: Always provide error handling with fallback modes
4. **Cleanup**: Unsubscribe from observables in `ngOnDestroy`
5. **Caching**: Leverage the built-in caching for better performance
6. **Sort Order**: Use `sortOrder` for consistent mode display ordering
7. **Capabilities**: Use capabilities for feature-based mode filtering
8. **Default Mode**: Always have a fallback default mode available

## Testing

The service can be easily mocked for testing:

```typescript
// Mock service for testing
const mockChatModeService = {
  getAllModes: () => of([mockChatMode]),
  getActiveModes: () => of([mockChatMode]),
  getDefaultMode: () => of(mockChatMode),
  modes$: of([mockChatMode]),
  loading$: of(false)
}
```

## Integration with Other Services

The ChatModeService works alongside other services:

```typescript
// Example: Using chat mode context in message service
sendMessage(content: string, sessionId: number) {
  const currentMode = this.chatModeService.getCurrentMode()
  const contextualContent = this.addModeContext(content, currentMode)
  return this.chatMessageService.sendUserMessage(contextualContent, sessionId)
}
```

This service provides a robust foundation for chat mode management in the InsightSage application with full integration into the existing architecture.
