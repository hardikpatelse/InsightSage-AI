import { Injectable, OnDestroy } from '@angular/core'
import { HttpClient } from '@angular/common/http'
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel, HttpTransportType } from '@microsoft/signalr'
import { BehaviorSubject, Subject, firstValueFrom } from 'rxjs'
import { MsalService } from '@azure/msal-angular'
import { environment } from '../../../../environments/environment'
import { AIProcessingStartedEvent } from '@/core/entities/custom/ai-processing-started-event'
import { AIResponseReceivedEvent } from '@/core/entities/custom/ai-response-received-event'
import { AIProcessingErrorEvent } from '@/core/entities/custom/ai-processing-error-event'

@Injectable({
    providedIn: 'root'
})
export class ChatHubService implements OnDestroy {
    private connection: HubConnection | null = null
    private connectionStateSubject = new BehaviorSubject<HubConnectionState>(HubConnectionState.Disconnected)
    private destroy$ = new Subject<void>()

    // Event subjects for real-time updates
    private aiProcessingStartedSubject = new Subject<AIProcessingStartedEvent>()
    private aiResponseReceivedSubject = new Subject<AIResponseReceivedEvent>()
    private aiProcessingErrorSubject = new Subject<AIProcessingErrorEvent>()

    // Public observables
    public connectionState$ = this.connectionStateSubject.asObservable()
    public aiProcessingStarted$ = this.aiProcessingStartedSubject.asObservable()
    public aiResponseReceived$ = this.aiResponseReceivedSubject.asObservable()
    public aiProcessingError$ = this.aiProcessingErrorSubject.asObservable()
    userId: any

    constructor(private httpClient: HttpClient, private msalService: MsalService) {
        // Initialize connection but don't connect automatically
        // Connection will be established when explicitly requested
    }

    ngOnDestroy() {
        this.destroy$.next()
        this.destroy$.complete()
        this.disconnect()
    }

    /**
     * Initialize SignalR connection with proper authentication using interceptor pattern
     */
    private initializeConnection() {
        try {
            const hubUrl = this.getHubUrl()

            this.connection = new HubConnectionBuilder()
                .withUrl(hubUrl, {
                    accessTokenFactory: async () => {
                        try {
                            // Get the active account - same as interceptor does
                            const account = this.msalService.instance.getActiveAccount()
                            if (!account) {
                                console.warn('SignalR: No active account for authentication')
                                throw new Error('No active account available')
                            }

                            // Acquire token with the same scopes as the interceptor uses for localhost:7039
                            const scopes = [`api://${environment.msal.clientId}/access_as_user`]

                            const result = await firstValueFrom(this.msalService.acquireTokenSilent({
                                scopes: scopes,
                                account: account
                            }))

                            if (!result) {
                                console.error('SignalR: No token result received')
                                throw new Error('Failed to acquire token')
                            }

                            // Use access token or ID token as fallback - same logic as interceptor
                            const token = result.accessToken || result.idToken

                            if (!token) {
                                console.error('SignalR: No access token or ID token in result')
                                throw new Error('No token available in result')
                            }

                            return token

                        } catch (error) {
                            console.error('SignalR: Failed to get access token:', error)
                            throw error
                        }
                    },
                    skipNegotiation: false, // Ensure negotiate is called
                    // Enable all transport methods as fallbacks
                    transport: HttpTransportType.WebSockets | HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling
                })
                .withAutomaticReconnect([0, 2000, 10000, 30000])
                .configureLogging(LogLevel.Information)
                .build()

            this.setupEventHandlers()
            this.setupConnectionStateHandlers()

        } catch (error) {
            console.error('Failed to initialize SignalR connection:', error)
        }
    }

    /**
     * Get the SignalR hub URL based on environment configuration
     */
    private getHubUrl(): string {
        // Use the chat API base URL and append the hub endpoint
        const baseUrl = environment.chatApiBaseUrl

        // Try the most common SignalR hub endpoint patterns
        // Backend might be using /chathub instead of /chat-hub
        const hubEndpoint = '/chat-hub'  // Could also try '/chathub', '/hubs/chat', etc.

        const fullUrl = `${baseUrl}${hubEndpoint}`

        return fullUrl
    }

    /**
     * Setup SignalR event handlers for AI processing events
     */
    private setupEventHandlers() {
        if (!this.connection) return

        // AI processing started event
        this.connection.on('AIProcessingStarted', (data: AIProcessingStartedEvent) => {
            console.log('AI processing started:', data)
            this.aiProcessingStartedSubject.next(data)
        })

        // AI response received event
        this.connection.on('AIResponseReceived', (data: AIResponseReceivedEvent) => {
            console.log('AI response received:', data)
            this.aiResponseReceivedSubject.next(data)
        })

        // AI processing error event
        this.connection.on('AIProcessingError', (data: AIProcessingErrorEvent) => {
            console.error('AI processing error:', data)
            this.aiProcessingErrorSubject.next(data)
        })
    }

    /**
     * Setup connection state change handlers
     */
    private setupConnectionStateHandlers() {
        if (!this.connection) return

        this.connection.onclose((error) => {
            if (error) {
                console.error('Connection closed with error:', error.message || error)
            }
            this.connectionStateSubject.next(HubConnectionState.Disconnected)
        })

        this.connection.onreconnecting((error) => {
            if (error) {
                console.error('Reconnecting due to error:', error.message || error)
            }
            this.connectionStateSubject.next(HubConnectionState.Reconnecting)
        })

        this.connection.onreconnected((connectionId) => {
            this.connectionStateSubject.next(HubConnectionState.Connected)
        })
    }

    /**
     * Test if the SignalR hub endpoint is available
     */
    private async testHubEndpoint(): Promise<void> {
        const hubUrl = this.getHubUrl()

        try {
            // Try to access the negotiate endpoint - this should exist if SignalR hub is configured
            const negotiateUrl = `${hubUrl}/negotiate`

            const response = await firstValueFrom(
                this.httpClient.post(negotiateUrl, {}, {
                    observe: 'response'
                })
            )

        } catch (error) {
            console.error('SignalR: Hub endpoint test failed:', error)
            console.error('DIAGNOSE: SignalR hub might not be configured at:', hubUrl)
        }
    }

    /**
     * Connect to SignalR hub
     */
    public async connect(): Promise<void> {
        if (!this.connection) {
            this.initializeConnection()
        }

        // Test endpoint before attempting connection
        await this.testHubEndpoint()

        if (this.connection && this.connection.state === HubConnectionState.Disconnected) {
            try {
                await this.connection.start()
                this.connectionStateSubject.next(HubConnectionState.Connected)
            } catch (error) {
                console.error('Error connecting to SignalR hub:', error)
                console.error('Connection state:', this.connection?.state)
                console.error('Hub URL:', this.getHubUrl())

                // Provide more specific error diagnostics
                if (error instanceof Error) {
                    console.error('Error details:', {
                        message: error.message,
                        stack: error.stack
                    })

                    if (error.message.includes('WebSocket failed to connect')) {
                        console.error('DIAGNOSE: WebSocket connection failed. Possible causes:')
                        console.error('1. Backend SignalR hub is not running at:', this.getHubUrl())
                        console.error('2. Backend SignalR hub endpoint might be different (try /chathub instead of /chat-hub)')
                        console.error('3. Backend CORS configuration might not allow WebSocket connections')
                        console.error('4. Backend authentication might be rejecting the token')
                    }
                }

                this.connectionStateSubject.next(HubConnectionState.Disconnected)
                throw error
            }
        }
    }

    /**
     * Disconnect from SignalR hub
     */
    public async disconnect(): Promise<void> {
        if (this.connection && this.connection.state === HubConnectionState.Connected) {
            try {
                await this.connection.stop()
                this.connectionStateSubject.next(HubConnectionState.Disconnected)
            } catch (error) {
                console.error('Error disconnecting from SignalR hub:', error)
            }
        }
    }

    /**
     * Join a specific chat session to receive real-time updates
     */
    public async joinSession(sessionId: string | number): Promise<void> {
        if (this.connection && this.connection.state === HubConnectionState.Connected) {
            try {
                await this.connection.invoke('JoinSession', sessionId.toString())
            } catch (error) {
                console.error(`Error joining session ${sessionId}:`, error)
                throw error
            }
        } else {
            console.warn('Cannot join session - SignalR connection is not established')
        }
    }

    /**
     * Leave a specific chat session
     */
    public async leaveSession(sessionId: string | number): Promise<void> {
        if (this.connection && this.connection.state === HubConnectionState.Connected) {
            try {
                await this.connection.invoke('LeaveSession', sessionId.toString())
            } catch (error) {
                console.error(`Error leaving session ${sessionId}:`, error)
                throw error
            }
        }
    }

    /**
     * Check if SignalR connection is active
     */
    public get isConnected(): boolean {
        return this.connection?.state === HubConnectionState.Connected
    }

    /**
     * Get current connection state
     */
    public get connectionState(): HubConnectionState {
        return this.connection?.state || HubConnectionState.Disconnected
    }
}
