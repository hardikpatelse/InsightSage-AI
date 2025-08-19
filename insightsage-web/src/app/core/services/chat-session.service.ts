import { Injectable } from '@angular/core'
import { Observable, BehaviorSubject, of } from 'rxjs'
import { map, catchError, tap, switchMap } from 'rxjs/operators'
import { ApiService } from './api.service'
import { environment } from '../../../environments/environment'
import { ChatSession, ChatSessionSearchParams } from '../entities/chat-session'
import { ChatSessionListResponse } from '../entities/responses/chat-sessions-response'
import { AddUpdateEntityResponse } from '../entities/common/add-update-entity-response'
import { ChatSessionResponse } from '../entities/responses/chat-session-response'
import { DeleteEntityResponse } from '../entities/common/delete-entity-response'
import { isApiSuccess } from '../entities/responses/api-response'
import { ChatSessionRequest } from '../entities/requests/chat-session-request'

@Injectable({
  providedIn: 'root'
})
export class ChatSessionService {
  private baseUrl = environment.chatApiBaseUrl;
  private currentSessionSubject = new BehaviorSubject<ChatSession | null>(null);
  private sessionsSubject = new BehaviorSubject<ChatSession[]>([]);

  public currentSession$ = this.currentSessionSubject.asObservable();
  public sessions$ = this.sessionsSubject.asObservable();

  constructor(private apiService: ApiService) {
  }

  /**
   * GET /api/Session
   * Get all chat sessions
   */
  getAllSessions(): Observable<ChatSession[]> {
    return this.apiService.get<ChatSession[]>(this.baseUrl, 'Session').pipe(
      map(sessions => {
        // The interceptor automatically unwraps successful responses
        const sessionArray = Array.isArray(sessions) ? sessions : []
        this.sessionsSubject.next(sessionArray)
        return sessionArray
      }),
      catchError(error => {
        console.error('Error fetching all sessions:', error)
        this.sessionsSubject.next([])
        return of([])
      })
    )
  }

  /**
   * POST /api/Session
   * Create a new chat session
   */
  createSession(session: ChatSessionRequest): Observable<number> {
    return this.apiService.post<number>(this.baseUrl, 'Session', session).pipe(
      map(id => {
        if (id && id > 0) {
          // Refresh sessions list (fire and forget)
          this.getUserSessions().subscribe()
        }
        return id
      }),
      catchError(error => {
        console.error('Error creating session:', error)
        throw error
      })
    )
  }

  /**
   * GET /api/Session/{id}
   * Get a specific chat session by ID
   */
  getSessionById(id: number): Observable<ChatSession> {
    return this.apiService.get<ChatSession>(this.baseUrl, `Session/${id}`).pipe(
      map(session => {
        // The interceptor automatically unwraps successful responses
        if (session) {
          this.currentSessionSubject.next(session)
          return session
        }
        throw new Error('Session not found')
      }),
      catchError(error => {
        console.error('Error fetching session by ID:', error)
        throw error
      })
    )
  }

  /**
   * PUT /api/Session/{id}
   * Update an existing chat session
   */
  updateSession(session: ChatSessionRequest): Observable<number> {
    return this.apiService.put<number>(this.baseUrl, `Session`, session).pipe(
      map(id => {
        if (id && id > 0) {
          // Refresh current session if it's the one being updated
          if (this.currentSessionSubject.value?.id === id) {
            this.getSessionById(id).subscribe()
          }
          // Refresh sessions list
          this.getUserSessions().subscribe()
          return id
        }
        throw new Error('Failed to update session')
      }),
      catchError(error => {
        console.error('Error updating session:', error)
        throw error
      })
    )
  }

  /**
   * DELETE /api/Session/{id}
   * Delete a chat session
   */
  deleteSession(id: number, modifiedBy: number = 0): Observable<string> {
    const url = `Session/${id}?modifiedBy=${modifiedBy}`
    return this.apiService.delete<string>(this.baseUrl, url).pipe(
      map(response => {
        if (response) {
          // Clear current session if it's the one being deleted
          if (this.currentSessionSubject.value?.id === id) {
            this.currentSessionSubject.next(null)
          }
          // Refresh sessions list
          this.getUserSessions().subscribe()
          return response || 'Session deleted successfully'
        }
        throw new Error('Failed to delete session')
      }),
      catchError(error => {
        console.error('Error deleting session:', error)
        throw error
      })
    )
  }

  /**
   * GET /api/Session/unique/{uniqueIdentity}
   * Get a chat session by unique identity (UUID)
   */
  getSessionByUniqueId(uniqueIdentity: string): Observable<ChatSession> {
    return this.apiService.get<ChatSession>(this.baseUrl, `Session/unique/${uniqueIdentity}`).pipe(
      map(session => {
        // The interceptor automatically unwraps successful responses
        if (session) {
          this.currentSessionSubject.next(session)
          return session
        }
        throw new Error('Failed to fetch session by unique ID')
      }),
      catchError(error => {
        console.error('Error fetching session by unique ID:', error)
        throw error
      })
    )
  }

  /**
   * GET /api/Session/user
   * Get all chat sessions for the current user
   */
  getUserSessions(): Observable<ChatSession[]> {
    console.log('ChatSessionService - getUserSessions called')
    return this.apiService.get<ChatSession[]>(this.baseUrl, 'Session/user').pipe(
      map(sessions => {
        console.log('ChatSessionService - getUserSessions response (unwrapped by interceptor):', sessions)

        // The interceptor automatically unwraps successful responses, so we get the array directly
        const sessionArray = Array.isArray(sessions) ? sessions : []

        console.log('ChatSessionService - updating sessionsSubject with:', sessionArray)
        this.sessionsSubject.next(sessionArray)
        return sessionArray
      }),
      catchError(error => {
        console.error('ChatSessionService - Error fetching user sessions:', error)
        this.sessionsSubject.next([])
        throw error
      })
    )
  }

  /**
   * GET /api/Session/search/user
   * Search chat sessions for the current user by name
   */
  searchUserSessions(params?: ChatSessionSearchParams): Observable<ChatSession[]> {
    const queryParams = params?.name ? `?name=${encodeURIComponent(params.name)}` : ''
    const url = `Session/search/user${queryParams}`

    return this.apiService.get<ChatSession[]>(this.baseUrl, url).pipe(
      map(sessions => {
        if (sessions) {
          return sessions
        }
        throw new Error('Failed to search user sessions')
      }),
      catchError(error => {
        console.error('Error searching user sessions:', error)
        throw error
      })
    )
  }

  /**
   * GET /api/Session/health
   * Check the health status of the session service
   */
  checkHealth(): Observable<boolean> {
    return this.apiService.get<any>(this.baseUrl, 'Session/health').pipe(
      map(() => true),
      catchError(error => {
        console.error('Health check failed:', error)
        throw error
      })
    )
  }

  // Utility methods for state management

  /**
   * Set the current active session
   */
  setCurrentSession(session: ChatSession | null): void {
    this.currentSessionSubject.next(session)
  }

  /**
   * Get the current active session synchronously
   */
  getCurrentSession(): ChatSession | null {
    return this.currentSessionSubject.value
  }

  /**
   * Clear all cached sessions
   */
  clearCache(): void {
    this.currentSessionSubject.next(null)
    this.sessionsSubject.next([])
  }

  /**
   * Create a new session with a default name
   */
  createNewSession(name?: string): Observable<ChatSession> {
    const sessionName = name || `Chat Session ${new Date().toLocaleString()}`
    const newSession: ChatSessionRequest = {
      name: sessionName,
      id: 0
    }

    return this.createSession(newSession).pipe(
      map(id => {

        const createdSession: ChatSession = {
          ...newSession,
          id: id,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
          uniqueIdentity: '',
          userId: ''
        }
        this.setCurrentSession(createdSession)
        return createdSession
      })
    )
  }

  /**
   * Refresh current session data
   */
  refreshCurrentSession(): Observable<ChatSession | null> {
    const currentSession = this.getCurrentSession()
    if (currentSession?.id) {
      return this.getSessionById(currentSession.id)
    }
    return new Observable(observer => {
      observer.next(null)
      observer.complete()
    })
  }

  /**
   * Duplicate an existing session
   */
  duplicateSession(sessionId: number, newName?: string): Observable<ChatSession> {
    return this.getSessionById(sessionId).pipe(
      map(session => {
        const duplicatedName = newName || `${session.name} (Copy)`
        return { name: duplicatedName }
      }),
      switchMap(sessionRequest => this.createSession({ ...sessionRequest, id: 0 })),
      switchMap(id => {
        const duplicatedSession: ChatSession = {
          id,
          name: newName || `Copy of session ${sessionId}`,
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
          uniqueIdentity: '',
          userId: ''
        }
        return of(duplicatedSession)
      })
    )
  }

  /**
   * Get session count for the current user
   */
  getUserSessionCount(): Observable<number> {
    return this.getUserSessions().pipe(
      map(sessions => sessions.length)
    )
  }

  /**
   * Get the most recently created session
   */
  getMostRecentSession(): Observable<ChatSession | null> {
    return this.getUserSessions().pipe(
      map(sessions => {
        if (sessions.length === 0) return null

        return sessions.reduce((latest, current) => {
          const latestDate = new Date(latest.createdAt || 0)
          const currentDate = new Date(current.createdAt || 0)
          return currentDate > latestDate ? current : latest
        })
      })
    )
  }

  /**
   * Rename a session
   */
  renameSession(sessionId: number, newName: string): Observable<number> {
    return this.getSessionById(sessionId).pipe(
      switchMap(session => {
        const updatedSession: ChatSessionRequest = {
          ...session,
          name: newName,
          id: sessionId
        }
        return this.updateSession(updatedSession)
      })
    )
  }

  /**
   * Batch delete multiple sessions
   */
  deleteSessions(sessionIds: number[], modifiedBy: number = 0): Observable<string[]> {
    const deleteObservables = sessionIds.map(id =>
      this.deleteSession(id, modifiedBy)
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
   * Check if a session name already exists for the user
   */
  isSessionNameExists(name: string): Observable<boolean> {
    return this.searchUserSessions({ name }).pipe(
      map(sessions => sessions.some(session =>
        session.name?.toLowerCase() === name.toLowerCase()
      ))
    )
  }
}
