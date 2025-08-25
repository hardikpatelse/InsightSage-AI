# AI Model Service Documentation

## Overview

The `AIModelService` provides comprehensive API integration for managing AI models in the InsightSage application. It follows the same patterns as the `ChatSessionService` and integrates seamlessly with the existing API interceptor architecture.

## Features

- **CRUD Operations**: Create, read, update, and delete AI models
- **Search & Filter**: Search models by name, provider, type, and status
- **State Management**: Reactive state management with RxJS BehaviorSubjects
- **Error Handling**: Comprehensive error handling with user-friendly messages
- **Caching**: Automatic caching and cache invalidation
- **Loading States**: Built-in loading state management

## API Endpoints

Based on the curl command provided:

```bash
curl -X 'GET' \
  'https://localhost:7039/api/AIModel' \
  -H 'accept: text/plain' \
  -H 'Authorization: Bearer {token}'
```

## Usage Examples

### 1. Basic Usage - Get All Models

```typescript
import { AIModelService } from '@/core/services/ai-model.service'
import { AIModel } from '@/core/entities/ai-model'

constructor(private aiModelService: AIModelService) {}

ngOnInit() {
  // Get all AI models
  this.aiModelService.getAllModels().subscribe({
    next: (models: AIModel[]) => {
      console.log('AI Models:', models)
      this.aiModels = models
    },
    error: (error) => {
      console.error('Failed to load models:', error)
    }
  })
}
```

### 2. Get Active Models Only

```typescript
// Get only active/enabled models
this.aiModelService.getActiveModels().subscribe(models => {
  this.activeModels = models
  if (models.length > 0) {
    this.selectedModel = models[0] // Select first active model
  }
})
```

### 3. Search Models by Provider

```typescript
// Search models by provider (e.g., OpenAI, Anthropic)
this.aiModelService.getModelsByProvider('openai').subscribe(models => {
  this.openAIModels = models
})

// Or use the search method directly
this.aiModelService.searchModels({ 
  provider: 'anthropic', 
  isActive: true 
}).subscribe(models => {
  this.anthropicModels = models
})
```

### 4. Create a New Model

```typescript
import { AIModelRequest } from '@/core/entities/ai-model'

createNewModel() {
  const newModel: AIModelRequest = {
    name: 'GPT-4 Turbo',
    value: 'gpt-4-turbo',
    description: 'Latest GPT-4 model with improved performance',
    provider: 'openai',
    modelType: 'chat-completion',
    isActive: true,
    maxTokens: 128000,
    costPerToken: 0.00003,
    capabilities: ['text-generation', 'code-completion', 'analysis']
  }

  this.aiModelService.createModel(newModel).subscribe({
    next: (modelId: number) => {
      console.log('Model created with ID:', modelId)
      // Refresh the models list (automatically handled by service)
    },
    error: (error) => {
      console.error('Failed to create model:', error)
    }
  })
}
```

### 5. Update Model Status

```typescript
// Toggle model active/inactive status
toggleModelStatus(modelId: number, isActive: boolean) {
  this.aiModelService.toggleModelStatus(modelId, isActive).subscribe({
    next: () => {
      console.log('Model status updated successfully')
    },
    error: (error) => {
      console.error('Failed to update model status:', error)
    }
  })
}
```

### 6. Subscribe to Reactive State

```typescript
ngOnInit() {
  // Subscribe to the models observable for reactive updates
  this.aiModelService.models$.subscribe(models => {
    this.aiModels = models
  })

  // Subscribe to loading state
  this.aiModelService.loading$.subscribe(loading => {
    this.isLoading = loading
  })

  // Subscribe to current selected model
  this.aiModelService.currentModel$.subscribe(model => {
    this.selectedModel = model
  })

  // Initial load
  this.aiModelService.getAllModels().subscribe()
}
```

### 7. Advanced Usage - Cost-Effective Model Selection

```typescript
// Get the most cost-effective model
this.aiModelService.getMostCostEffectiveModel().subscribe(model => {
  if (model) {
    console.log('Most cost-effective model:', model.name)
    this.selectedModel = model
  }
})

// Get high-capacity models (8K+ tokens)
this.aiModelService.getHighCapacityModels(8000).subscribe(models => {
  this.highCapacityModels = models
})
```

## Service Methods

### Core CRUD Methods
- `getAllModels()`: Get all AI models
- `getModelById(id: number)`: Get specific model by ID
- `createModel(model: AIModelRequest)`: Create new model
- `updateModel(id: number, model: AIModelRequest)`: Update existing model
- `deleteModel(id: number, modifiedBy?: number)`: Delete model

### Search & Filter Methods
- `getActiveModels()`: Get only active models
- `searchModels(params?: AIModelSearchParams)`: Search with filters
- `getModelsByProvider(provider: string)`: Filter by provider
- `getModelsByType(modelType: string)`: Filter by model type

### Utility Methods
- `setCurrentModel(model: AIModel | null)`: Set active model
- `getCurrentModel()`: Get current active model
- `isLoading()`: Check loading state
- `clearCache()`: Clear cached data
- `checkHealth()`: Health check endpoint

### Batch Operations
- `deleteModels(ids: number[])`: Delete multiple models
- `toggleModelStatus(id: number, status: boolean)`: Toggle active status

## Error Handling

The service includes comprehensive error handling:

```typescript
this.aiModelService.getAllModels().subscribe({
  next: (models) => {
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

The service is already integrated into the chat component:

```typescript
// In chat.ts
export class Chat implements OnInit {
  aiModels: AIModel[] = []
  selectedAIModel: AIModel | null = null

  constructor(private aiModelService: AIModelService) {}

  ngOnInit() {
    this.loadAIModels() // Loads models from API with fallback
  }
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
3. **Error Handling**: Always provide error handling for better UX
4. **Cleanup**: Unsubscribe from observables in `ngOnDestroy`
5. **Caching**: Leverage the built-in caching for better performance
6. **Fallback Data**: Provide fallback models for offline scenarios

## Testing

The service can be easily mocked for testing:

```typescript
// Mock service for testing
const mockAIModelService = {
  getAllModels: () => of([mockAIModel]),
  getActiveModels: () => of([mockAIModel]),
  models$: of([mockAIModel]),
  loading$: of(false)
}
```

This service provides a robust foundation for AI model management in the InsightSage application with full integration into the existing architecture.
