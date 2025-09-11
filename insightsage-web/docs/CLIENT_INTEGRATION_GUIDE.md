# Client Integration Guide - InsightSage AI Chat

## ?? **SignalR Connection Setup**

### **JavaScript/TypeScript Client**

```typescript
import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr';

class InsightSageChatClient {
    private connection: HubConnection;

    constructor(private accessToken: string) {
        this.connection = new HubConnectionBuilder()
            .withUrl('/api/chat-hub', {
                accessTokenFactory: () => this.accessToken
            })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        this.setupEventHandlers();
    }

    private setupEventHandlers() {
        // AI processing started
        this.connection.on('AIProcessingStarted', (data) => {
            console.log('AI processing started:', data);
            this.showTypingIndicator(data.sessionId);
        });

        // AI response received
        this.connection.on('AIResponseReceived', (data) => {
            console.log('AI response received:', data);
            this.hideTypingIndicator(data.sessionId);
            this.displayAIMessage(data);
        });

        // AI processing error
        this.connection.on('AIProcessingError', (data) => {
            console.error('AI processing error:', data);
            this.hideTypingIndicator(data.sessionId);
            this.showError(data.error);
        });
    }

    async connect(): Promise<void> {
        try {
            await this.connection.start();
            console.log('Connected to InsightSage Chat Hub');
        } catch (err) {
            console.error('Error connecting to hub:', err);
            throw err;
        }
    }

    async joinSession(sessionId: string): Promise<void> {
        if (this.connection.state === 'Connected') {
            await this.connection.invoke('JoinSession', sessionId);
        }
    }

    async sendMessage(message: {
        content: string;
        sessionId?: number;
        aiModelId?: number;
        chatModeId?: number;
        processWithAI?: boolean;
        platformSlug?: string;
        model?: string;
    }): Promise<any> {
        const response = await fetch('/api/message/with-ai', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${this.accessToken}`
            },
            body: JSON.stringify({
                message: {
                    role: 'user',
                    content: message.content,
                    chatSessionId: message.sessionId || 0
                },
                aiModelId: message.aiModelId,
                chatModeId: message.chatModeId,
                processWithAI: message.processWithAI ?? true,
                platformSlug: message.platformSlug || 'openai',
                model: message.model || 'gpt-3.5-turbo'
            })
        });

        return response.json();
    }

    private showTypingIndicator(sessionId: number) {
        // Show "AI is typing..." indicator
        const indicator = document.querySelector(`#typing-${sessionId}`);
        if (indicator) {
            indicator.style.display = 'block';
        }
    }

    private hideTypingIndicator(sessionId: number) {
        // Hide "AI is typing..." indicator
        const indicator = document.querySelector(`#typing-${sessionId}`);
        if (indicator) {
            indicator.style.display = 'none';
        }
    }

    private displayAIMessage(data: any) {
        // Display AI message in chat UI
        const messagesContainer = document.querySelector(`#messages-${data.sessionId}`);
        if (messagesContainer) {
            const messageElement = document.createElement('div');
            messageElement.className = 'ai-message';
            messageElement.innerHTML = `
                <div class="message-content">${data.content}</div>
                <div class="message-meta">
                    <span class="timestamp">${new Date(data.timestamp).toLocaleTimeString()}</span>
                    <span class="tokens">Tokens: ${data.usage.totalTokens}</span>
                </div>
            `;
            messagesContainer.appendChild(messageElement);
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }
    }

    private showError(error: string) {
        // Show error message to user
        console.error('Chat error:', error);
        // Implement your error display logic here
    }
}

// Usage Example
const chatClient = new InsightSageChatClient(userToken);

async function initializeChat() {
    await chatClient.connect();
    await chatClient.joinSession('session-123');
    
    // Send a message
    const response = await chatClient.sendMessage({
        content: 'Hello, can you help me with JavaScript?',
        sessionId: 123,
        aiModelId: 1,
        chatModeId: 2
    });
    
    if (response.status === 200) {
        console.log('Message sent successfully');
    }
}
```

### **React Hook Example**

```typescript
import { useEffect, useState, useCallback } from 'react';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

export const useChatHub = (accessToken: string) => {
    const [connection, setConnection] = useState<HubConnection | null>(null);
    const [isConnected, setIsConnected] = useState(false);
    const [messages, setMessages] = useState<any[]>([]);
    const [isAITyping, setIsAITyping] = useState(false);

    useEffect(() => {
        const newConnection = new HubConnectionBuilder()
            .withUrl('/api/chat-hub', {
                accessTokenFactory: () => accessToken
            })
            .withAutomaticReconnect()
            .build();

        setConnection(newConnection);

        return () => {
            newConnection?.stop();
        };
    }, [accessToken]);

    useEffect(() => {
        if (connection) {
            connection.start()
                .then(() => setIsConnected(true))
                .catch(err => console.error('Connection failed:', err));

            connection.on('AIProcessingStarted', () => {
                setIsAITyping(true);
            });

            connection.on('AIResponseReceived', (data) => {
                setIsAITyping(false);
                setMessages(prev => [...prev, {
                    id: data.messageId,
                    role: 'assistant',
                    content: data.content,
                    timestamp: data.timestamp,
                    usage: data.usage
                }]);
            });

            connection.on('AIProcessingError', (data) => {
                setIsAITyping(false);
                console.error('AI Error:', data.error);
            });
        }
    }, [connection]);

    const sendMessage = useCallback(async (content: string, options?: any) => {
        if (!isConnected) return;

        // Add user message to UI immediately
        const userMessage = {
            id: Date.now(),
            role: 'user',
            content,
            timestamp: new Date().toISOString()
        };
        setMessages(prev => [...prev, userMessage]);

        // Send to API
        const response = await fetch('/api/message/with-ai', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`
            },
            body: JSON.stringify({
                message: {
                    role: 'user',
                    content,
                    chatSessionId: options?.sessionId || 0
                },
                processWithAI: true,
                platformSlug: options?.platformSlug || 'openai',
                model: options?.model || 'gpt-3.5-turbo',
                ...options
            })
        });

        return response.json();
    }, [isConnected, accessToken]);

    return {
        isConnected,
        messages,
        isAITyping,
        sendMessage,
        joinSession: (sessionId: string) => connection?.invoke('JoinSession', sessionId)
    };
};
```

## ?? **UI/UX Patterns**

### **1. Immediate User Feedback**
```typescript
// User sends message
await sendMessage("Hello AI");

// UI immediately shows:
// - User message in chat
// - "AI is thinking..." indicator
// - Message status: "Sent" ?
```

### **2. Real-time AI Response**
```typescript
// When AI responds via SignalR:
// - Hide typing indicator
// - Stream or display AI response
// - Show token usage info
// - Update message status
```

### **3. Error Handling**
```typescript
// If AI processing fails:
// - Show error message
// - Offer retry option
// - Don't lose user's message
```

## ?? **Message Flow States**

1. **Sending** ? User message being saved
2. **Sent** ? User message saved successfully  
3. **Processing** ? AI is generating response
4. **Completed** ? AI response received
5. **Error** ? Something went wrong

## ? **Performance Benefits**

- **Immediate Response**: User gets instant feedback
- **Non-blocking**: UI remains responsive during AI processing
- **Real-time**: AI responses appear as they're ready
- **Scalable**: Server can handle multiple concurrent requests
- **Mobile-friendly**: Works well with mobile network conditions