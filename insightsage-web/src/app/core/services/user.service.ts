import { Injectable } from '@angular/core'
import { environment } from '../../../environments/environment'
import { DataProviderService } from './base/data-provider-service'
import { ApiService } from './api.service'
import { map, Observable, of, switchMap, BehaviorSubject, from, catchError, tap } from 'rxjs'
import { MsalService } from '@azure/msal-angular'
import { User } from '../entities/user'
import { AccountInfo } from '@azure/msal-browser'
import { NotificationService } from './notification.service'

@Injectable({
  providedIn: 'root'
})
export class UserService {

  private baseUrl: string = environment.authApiBaseUrl;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  private initializationPromise: Promise<void>
  private syncInProgress = false;
  private lastSyncTime: number = 0;
  private syncCooldownMs = 5000; // 5 seconds cooldown between syncs

  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private dataProviderService: DataProviderService,
    private apiService: ApiService,
    private msalService: MsalService,
    private notificationService: NotificationService
  ) {
    // Initialize MSAL and user on service creation
    this.initializationPromise = this.initializeMsal()
  }

  /**
   * Initialize MSAL and then check for existing user
   */
  private async initializeMsal(): Promise<void> {
    try {
      // Wait for MSAL instance to be ready
      await this.msalService.instance.initialize()
      console.log('MSAL initialized successfully')

      await this.handleRedirectPromise()

      // Initialize user after MSAL is ready
      this.initializeUser()
    } catch (error) {
      console.error('Failed to initialize MSAL:', error)
      throw error
    }
  }
  async handleRedirectPromise() {
    // Handle redirect promise if coming back from a redirect
    try {
      const response = await this.msalService.instance.handleRedirectPromise()
      if (response && response.account) {
        this.msalService.instance.setActiveAccount(response.account)
      }
    } catch (error) {
      console.warn('No redirect response to handle:', error)
    }

  }

  /**
   * Initialize user from MSAL account if already logged in
   */
  private initializeUser(): void {
    const account = this.msalService.instance.getActiveAccount()
    if (account) {
      const user = this.mapAccountToUser(account)
      console.log('User initialized from existing account:', user)

      // Try to sync enriched profile with backend to ensure user info is up to date
      this.syncUserProfileWithBackendCached(false).subscribe({
        next: (backendUser) => {
          console.log('User profile synced with backend on initialization:', backendUser)
          this.currentUserSubject.next(backendUser)
        },
        error: (profileSyncError) => {
          console.warn('Failed to sync user profile with backend on initialization, trying basic sync:', profileSyncError)
          // Fall back to basic sync if enriched sync fails
          this.syncUserWithBackend().subscribe({
            next: (backendUser) => {
              console.log('User synced with backend on initialization (basic):', backendUser)
              this.currentUserSubject.next(backendUser)
            },
            error: (basicSyncError) => {
              console.warn('Failed to sync user with backend on initialization, using local user data:', basicSyncError)
              // Fall back to local user data if all syncs fail
              this.currentUserSubject.next(user)
            }
          })
        }
      })
    }
  }

  /**
   * Get current user information from MSAL
   */
  getCurrentUser(): Observable<User | null> {
    return from(this.initializationPromise).pipe(
      switchMap(() => {
        const account = this.msalService.instance.getActiveAccount()
        if (account) {
          const user = this.mapAccountToUser(account)

          // Try to sync enriched profile with backend (but don't force if recently synced)
          return this.syncUserProfileWithBackendCached(false).pipe(
            map(backendUser => {
              this.currentUserSubject.next(backendUser)
              return backendUser
            }),
            catchError(profileSyncError => {
              console.warn('Failed to sync user profile with backend in getCurrentUser, trying basic sync:', profileSyncError)
              // Fall back to basic sync if enriched sync fails
              return this.syncUserWithBackend().pipe(
                map(backendUser => {
                  this.currentUserSubject.next(backendUser)
                  return backendUser
                }),
                catchError(basicSyncError => {
                  console.warn('Failed to sync user with backend in getCurrentUser, using local user data:', basicSyncError)
                  this.currentUserSubject.next(user)
                  return of(user)
                })
              )
            })
          )
        }
        return of(null)
      }),
      catchError(error => {
        console.error('Error getting current user:', error)
        return of(null)
      })
    )
  }

  /**
   * Get user information synchronously
   */
  getCurrentUserSync(): User | null {
    const account = this.msalService.instance.getActiveAccount()
    return account ? this.mapAccountToUser(account) : null
  }

  /**
   * Map MSAL AccountInfo to User interface
   */
  private mapAccountToUser(account: AccountInfo): User {
    return {
      id: 0, // You might want to get this from your backend
      userId: account.localAccountId || account.homeAccountId || '',
      name: account.name || account.username || 'Unknown User',
      email: account.username || (account.idTokenClaims?.emails ? account.idTokenClaims.emails[0] : '') || '',
      tenantId: account.tenantId || '',
      objectId: account.localAccountId || account.homeAccountId || ''
    }
  }

  /**
   * Login with redirect and update user info
   */
  async login(): Promise<void> {
    await this.initializationPromise

    const loginRequest = {
      scopes: ['user.read', 'profile', 'email', 'openid'],
      redirectStartPage: window.location.href
    }

    this.msalService.loginRedirect(loginRequest)
  }

  /**
   * Login with popup and get user info
   */
  loginPopup(): Observable<User | null> {
    return from(this.initializationPromise).pipe(
      switchMap(() => this.performPopupLogin()),
      catchError(error => {
        console.error('Error during popup login:', error)
        return of(null)
      })
    )
  }

  /**
   * Perform the actual popup login
   */
  private performPopupLogin(): Observable<User | null> {
    return new Observable(observer => {
      // Configure popup request
      const popupRequest = {
        scopes: ['user.read', 'profile', 'email', 'openid'],
        prompt: 'select_account'
      }

      this.msalService.loginPopup(popupRequest).subscribe({
        next: (result) => {
          console.log('Popup login result:', result)
          if (result.account) {
            // Set the account as active
            this.msalService.instance.setActiveAccount(result.account)
            const user = this.mapAccountToUser(result.account)

            // Try to sync enriched user profile with backend after successful login
            this.syncUserProfileWithBackendCached(true).subscribe({
              next: (backendUser) => {
                console.log('User profile synced with backend:', backendUser)
                this.notificationService.showSuccess(`Welcome back, ${backendUser.name}!`, 'Login Successful')
                // Update user with backend data (including user ID)
                this.currentUserSubject.next(backendUser)
                observer.next(backendUser)
                observer.complete()
              },
              error: (syncError) => {
                console.warn('Failed to sync user profile with backend, trying basic sync:', syncError)
                // Fall back to basic sync if enriched sync fails
                this.syncUserWithBackend().subscribe({
                  next: (backendUser) => {
                    console.log('User synced with backend (basic):', backendUser)
                    this.notificationService.showSuccess(`Welcome back, ${backendUser.name}!`, 'Login Successful')
                    this.currentUserSubject.next(backendUser)
                    observer.next(backendUser)
                    observer.complete()
                  },
                  error: (basicSyncError) => {
                    console.warn('Failed to sync user with backend, using local user data:', basicSyncError)
                    this.notificationService.showWarning('Login successful, but user data sync failed.', 'Partial Success')
                    // Fall back to local user data if all syncs fail
                    this.currentUserSubject.next(user)
                    observer.next(user)
                    observer.complete()
                  }
                })
              }
            })
          } else {
            observer.next(null)
            observer.complete()
          }
        },
        error: (error) => {
          console.error('Login failed:', error)
          observer.error(error)
        }
      })
    })
  }

  /**
   * Get detailed user profile from Microsoft Graph API and sync with backend
   */
  getUserProfile(): Observable<any> {
    const account = this.msalService.instance.getActiveAccount()
    if (!account) {
      console.error('No active account found for getUserProfile')
      throw new Error('No active account found')
    }

    console.log('Getting user profile for account:', account.username)

    return this.msalService.acquireTokenSilent({
      scopes: ['user.read'],
      account: account
    }).pipe(
      switchMap(result => {
        console.log('Token acquired for Graph API:', !!result.accessToken)
        const headers = { 'Authorization': `Bearer ${result.accessToken}` }
        return this.dataProviderService.getData<any>('https://graph.microsoft.com', 'v1.0/me', headers)
      }),
      tap(profile => {
        console.log('Microsoft Graph user profile:', profile)
      }),
      catchError(error => {
        console.error('Error getting user profile:', error)
        throw error
      })
    )
  }

  /**
   * Get detailed user profile and sync it with backend (with caching to prevent duplicate calls)
   */
  private syncUserProfileWithBackendCached(forceSync: boolean = false): Observable<User> {
    const now = Date.now()

    // If sync is already in progress, return the current user or wait
    if (this.syncInProgress && !forceSync) {
      return of(this.currentUserSubject.value).pipe(
        switchMap(currentUser => {
          if (currentUser) {
            return of(currentUser)
          }
          // If no current user, wait a bit and try again
          return new Observable<User>(observer => {
            const checkUser = () => {
              const user = this.currentUserSubject.value
              if (user || !this.syncInProgress) {
                observer.next(user!)
                observer.complete()
              } else {
                setTimeout(checkUser, 100)
              }
            }
            checkUser()
          })
        })
      )
    }

    // If recently synced and not forcing, return current user
    if (!forceSync && (now - this.lastSyncTime) < this.syncCooldownMs && this.currentUserSubject.value) {
      return of(this.currentUserSubject.value)
    }

    // Perform the actual sync
    this.syncInProgress = true
    return this.syncUserProfileWithBackend().pipe(
      tap(user => {
        this.lastSyncTime = now
        this.syncInProgress = false
      }),
      catchError(error => {
        this.syncInProgress = false
        throw error
      })
    )
  }

  /**
   * Get detailed user profile and sync it with backend
   */
  syncUserProfileWithBackend(): Observable<User> {
    return this.getUserProfile().pipe(
      switchMap(profile => {
        const account = this.msalService.instance.getActiveAccount()
        if (!account) {
          throw new Error('No active account found')
        }
        const enrichedUserData = {
          name: profile.displayName || account.name || 'Unknown User',
          userId: account.localAccountId || account.homeAccountId || '',
          email: profile.mail || profile.userPrincipalName || account.username || '',
          tenantId: account.tenantId || '',
          objectId: account.localAccountId || account.homeAccountId || '',
          jobTitle: profile.jobTitle || '',
          department: profile.department || '',
          officeLocation: profile.officeLocation || '',
          mobilePhone: profile.mobilePhone || '',
          businessPhones: profile.businessPhones || [],
          idTokenClaims: account.idTokenClaims,
          lastLoginDate: new Date().toISOString()
        }

        return this.apiService.post<User>(this.baseUrl, 'user/login', enrichedUserData)
      }),
      catchError(error => {
        console.error('Error syncing user profile with backend:', error)
        // Fall back to basic sync if profile fetch fails
        return this.syncUserWithBackend()
      })
    )
  }

  /**
   * Send user info to your backend API (handles both insert and update)
   */
  syncUserWithBackend(): Observable<User> {
    const account = this.msalService.instance.getActiveAccount()
    if (!account) {
      throw new Error('No active account found')
    }

    const userData = {
      name: account.name || 'Unknown User',
      userId: account.localAccountId || account.homeAccountId || '',
      email: account.username || '',
      tenantId: account.tenantId || '',
      objectId: account.localAccountId || account.homeAccountId || '',
      idTokenClaims: account.idTokenClaims,
      lastLoginDate: new Date().toISOString()
    }

    // This endpoint should handle both insert (if user doesn't exist) and update (if user exists)
    return this.apiService.post<User>(this.baseUrl, 'user/login', userData).pipe(
      map(response => {
        return response
      }),
      catchError(error => {
        console.error('Error syncing user with backend:', error)
        throw error
      })
    )
  }

  /**
   * Logout and clear user info
   */
  logout(): void {
    const currentUser = this.currentUserSubject.value
    this.currentUserSubject.next(null)
    this.clearSyncCache()

    if (currentUser) {
      this.notificationService.showInfo('You have been logged out successfully.', 'Goodbye!')
    }

    this.msalService.logoutRedirect()
  }

  /**
   * Clear sync cache (useful for logout or when forcing fresh data)
   */
  private clearSyncCache(): void {
    this.syncInProgress = false
    this.lastSyncTime = 0
  }

  /**
   * Manually refresh user data from backend
   */
  refreshUserData(): Observable<User | null> {
    const account = this.msalService.instance.getActiveAccount()
    if (!account) {
      return of(null)
    }

    return this.syncUserProfileWithBackendCached(true).pipe(
      map(backendUser => {
        this.currentUserSubject.next(backendUser)
        return backendUser
      }),
      catchError(error => {
        console.error('Failed to refresh user data:', error)
        return of(this.currentUserSubject.value)
      })
    )
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return this.msalService.instance.getActiveAccount() !== null
  }

}
