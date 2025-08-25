import { Injectable } from '@angular/core'
import { Observable, BehaviorSubject, of } from 'rxjs'
import { map, catchError, tap, switchMap } from 'rxjs/operators'
import { ApiService } from './api.service'
import { environment } from '../../../environments/environment'
import { ChatMode, ChatModeSearchParams, ChatModeRequest } from '../entities/chat-mode'
import { ChatModeResponse, ChatModesResponse } from '../entities/responses/chat-mode-response'
import { AddUpdateEntityResponse } from '../entities/common/add-update-entity-response'
import { DeleteEntityResponse } from '../entities/common/delete-entity-response'

/**
 * Service for managing Chat Modes with comprehensive API integration
 * Based on the Swagger API specification for ChatMode endpoints
 */
@Injectable({
    providedIn: 'root'
})
export class ChatModeService {
    private baseUrl = environment.chatApiBaseUrl;
    private currentModeSubject = new BehaviorSubject<ChatMode | null>(null);
    private modesSubject = new BehaviorSubject<ChatMode[]>([]);
    private loadingSubject = new BehaviorSubject<boolean>(false);

    public currentMode$ = this.currentModeSubject.asObservable();
    public modes$ = this.modesSubject.asObservable();
    public loading$ = this.loadingSubject.asObservable();

    constructor(private apiService: ApiService) {
    }

    // ========================
    // API Methods
    // ========================

    /**
     * GET /api/ChatMode
     * Get all chat modes
     */
    getAllModes(): Observable<ChatMode[]> {
        this.loadingSubject.next(true)

        return this.apiService.get<ChatMode[]>(this.baseUrl, 'ChatMode').pipe(
            map(modes => {
                this.loadingSubject.next(false)
                // The interceptor automatically unwraps successful responses
                const modeArray = Array.isArray(modes) ? modes : []
                this.modesSubject.next(modeArray)
                return modeArray
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error('Error fetching all chat modes:', error)
                this.modesSubject.next([])
                return of([])
            })
        )
    }

    /**
     * GET /api/ChatMode/{id}
     * Get a specific chat mode by ID
     */
    getModeById(id: number): Observable<ChatMode> {
        this.loadingSubject.next(true)

        return this.apiService.get<ChatMode>(this.baseUrl, `ChatMode/${id}`).pipe(
            map(mode => {
                this.loadingSubject.next(false)
                // The interceptor automatically unwraps successful responses
                if (mode) {
                    this.currentModeSubject.next(mode)
                    return mode
                }
                throw new Error('Chat Mode not found')
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error('Error fetching chat mode by ID:', error)
                throw error
            })
        )
    }

    /**
     * POST /api/ChatMode
     * Create a new chat mode
     */
    createMode(mode: ChatModeRequest): Observable<number> {
        this.loadingSubject.next(true)

        return this.apiService.post<number>(this.baseUrl, 'ChatMode', mode).pipe(
            map(id => {
                this.loadingSubject.next(false)
                if (id && id > 0) {
                    // Refresh modes list (fire and forget)
                    this.getAllModes().subscribe()
                }
                return id
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error('Error creating chat mode:', error)
                throw error
            })
        )
    }

    /**
     * PUT /api/ChatMode/{id}
     * Update an existing chat mode
     */
    updateMode(id: number, mode: ChatModeRequest): Observable<number> {
        this.loadingSubject.next(true)

        const updatePayload = { ...mode, id }

        return this.apiService.put<number>(this.baseUrl, `ChatMode/${id}`, updatePayload).pipe(
            map(updatedId => {
                this.loadingSubject.next(false)
                if (updatedId && updatedId > 0) {
                    // Refresh current mode if it's the one being updated
                    if (this.currentModeSubject.value?.id === id) {
                        this.getModeById(id).subscribe()
                    }
                    // Refresh modes list
                    this.getAllModes().subscribe()
                    return updatedId
                }
                throw new Error('Failed to update chat mode')
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error('Error updating chat mode:', error)
                throw error
            })
        )
    }

    /**
     * DELETE /api/ChatMode/{id}
     * Delete a chat mode
     */
    deleteMode(id: number, modifiedBy: number = 0): Observable<string> {
        this.loadingSubject.next(true)

        const url = `ChatMode/${id}?modifiedBy=${modifiedBy}`
        return this.apiService.delete<string>(this.baseUrl, url).pipe(
            map(response => {
                this.loadingSubject.next(false)
                if (response) {
                    // Clear current mode if it's the one being deleted
                    if (this.currentModeSubject.value?.id === id) {
                        this.currentModeSubject.next(null)
                    }
                    // Refresh modes list
                    this.getAllModes().subscribe()
                    return response || 'Chat Mode deleted successfully'
                }
                throw new Error('Failed to delete chat mode')
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error('Error deleting chat mode:', error)
                throw error
            })
        )
    }

    /**
     * GET /api/ChatMode/active
     * Get all active chat modes
     */
    getActiveModes(): Observable<ChatMode[]> {
        this.loadingSubject.next(true)

        return this.apiService.get<ChatMode[]>(this.baseUrl, 'ChatMode/active').pipe(
            map(modes => {
                this.loadingSubject.next(false)
                // The interceptor automatically unwraps successful responses
                const activeModes = Array.isArray(modes) ? modes : []
                return activeModes
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error('Error fetching active chat modes:', error)
                return of([])
            })
        )
    }

    /**
     * GET /api/ChatMode/search
     * Search chat modes with optional parameters
     */
    searchModes(params?: ChatModeSearchParams): Observable<ChatMode[]> {
        this.loadingSubject.next(true)

        const queryParams = new URLSearchParams()
        if (params?.name) queryParams.append('label', params.name)
        if (params?.value) queryParams.append('value', params.value)
        if (params?.isActive !== undefined) queryParams.append('isActive', params.isActive.toString())

        const url = `ChatMode/search${queryParams.toString() ? '?' + queryParams.toString() : ''}`

        return this.apiService.get<ChatMode[]>(this.baseUrl, url).pipe(
            map(modes => {
                this.loadingSubject.next(false)
                if (modes) {
                    return Array.isArray(modes) ? modes : []
                }
                throw new Error('Failed to search chat modes')
            }),
            catchError(error => {
                this.loadingSubject.next(false)
                console.error('Error searching chat modes:', error)
                throw error
            })
        )
    }

    /**
     * GET /api/ChatMode/health
     * Check the health status of the chat mode service
     */
    checkHealth(): Observable<boolean> {
        return this.apiService.get<any>(this.baseUrl, 'ChatMode/health').pipe(
            map(() => true),
            catchError(error => {
                console.error('Chat Mode service health check failed:', error)
                return of(false)
            })
        )
    }

    // ========================
    // Utility Methods
    // ========================

    /**
     * Set the current active chat mode
     */
    setCurrentMode(mode: ChatMode | null): void {
        this.currentModeSubject.next(mode)
    }

    /**
     * Get the current active chat mode synchronously
     */
    getCurrentMode(): ChatMode | null {
        return this.currentModeSubject.value
    }

    /**
     * Get current loading state
     */
    isLoading(): boolean {
        return this.loadingSubject.value
    }

    /**
     * Set loading state
     */
    setLoadingState(loading: boolean): void {
        this.loadingSubject.next(loading)
    }

    /**
     * Clear all cached modes
     */
    clearCache(): void {
        this.currentModeSubject.next(null)
        this.modesSubject.next([])
    }

    /**
     * Refresh current mode data
     */
    refreshCurrentMode(): Observable<ChatMode | null> {
        const currentMode = this.getCurrentMode()
        if (currentMode?.id) {
            return this.getModeById(currentMode.id)
        }
        return new Observable(observer => {
            observer.next(null)
            observer.complete()
        })
    }

    /**
     * Get modes by capability
     */
    getModesByCapability(capability: string): Observable<ChatMode[]> {
        return this.getAllModes().pipe(
            map(modes =>
                modes.filter(mode =>
                    mode.capabilities?.includes(capability) || false
                )
            )
        )
    }

    /**
     * Get modes sorted by display order
     */
    getModesSortedByOrder(): Observable<ChatMode[]> {
        return this.getAllModes().pipe(
            map(modes =>
                modes.sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0))
            )
        )
    }

    /**
     * Get mode count
     */
    getModeCount(): Observable<number> {
        return this.getAllModes().pipe(
            map(modes => modes.length)
        )
    }

    /**
     * Get active mode count
     */
    getActiveModeCount(): Observable<number> {
        return this.getActiveModes().pipe(
            map(modes => modes.length)
        )
    }

    /**
     * Check if a mode name already exists
     */
    isModeNameExists(name: string): Observable<boolean> {
        return this.searchModes({ name }).pipe(
            map(modes => modes.some(mode =>
                mode.name?.toLowerCase() === name.toLowerCase()
            ))
        )
    }

    /**
     * Check if a mode value already exists
     */
    isModeValueExists(value: string): Observable<boolean> {
        return this.searchModes({ value }).pipe(
            map(modes => modes.some(mode =>
                mode.value?.toLowerCase() === value.toLowerCase()
            ))
        )
    }

    /**
     * Toggle mode active status
     */
    toggleModeStatus(id: number, isActive: boolean): Observable<number> {
        return this.getModeById(id).pipe(
            switchMap(mode => {
                const updatedMode: ChatModeRequest = {
                    ...mode,
                    isActive
                }
                return this.updateMode(id, updatedMode)
            })
        )
    }

    /**
     * Batch delete multiple modes
     */
    deleteModes(modeIds: number[], modifiedBy: number = 0): Observable<string[]> {
        const deleteObservables = modeIds.map(id =>
            this.deleteMode(id, modifiedBy)
        )

        return new Observable(observer => {
            Promise.all(deleteObservables.map(obs => obs.toPromise()))
                .then(results => {
                    observer.next(results as string[])
                    observer.complete()
                })
                .catch(error => observer.error(error))
        })
    }

    /**
     * Get mode by value
     */
    getModeByValue(value: string): Observable<ChatMode | null> {
        return this.getAllModes().pipe(
            map(modes => modes.find(mode => mode.value === value) || null)
        )
    }

    /**
     * Update mode sort order
     */
    updateModeOrder(id: number, sortOrder: number): Observable<number> {
        return this.getModeById(id).pipe(
            switchMap(mode => {
                const updatedMode: ChatModeRequest = {
                    ...mode,
                    sortOrder
                }
                return this.updateMode(id, updatedMode)
            })
        )
    }

    /**
     * Get the default mode (usually 'chat' or first active mode)
     */
    getDefaultMode(): Observable<ChatMode | null> {
        return this.getActiveModes().pipe(
            map(modes => {
                if (modes.length === 0) return null

                // Try to find 'chat' mode first
                const chatMode = modes.find(mode => mode.value === 'chat')
                if (chatMode) return chatMode

                // Otherwise return the first mode sorted by order
                return modes.sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0))[0]
            })
        )
    }
}
