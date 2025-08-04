import { Component, OnInit } from '@angular/core'
import { CommonModule } from '@angular/common'
import { MsalService } from '@azure/msal-angular'
import { UserService } from '../../core/services/user.service'

@Component({
    selector: 'app-auth-test',
    template: `
    <div class="p-6 max-w-4xl mx-auto">
      <h2 class="text-2xl font-bold mb-6">Authentication Test</h2>
      
      <div class="space-y-4">
        <div class="bg-gray-50 p-4 rounded-lg">
          <h3 class="text-lg font-semibold mb-2">Authentication Status</h3>
          <p><strong>Is Authenticated:</strong> {{ isAuthenticated }}</p>
          <p><strong>Active Account:</strong> {{ activeAccount ? activeAccount.username : 'None' }}</p>
          <p><strong>Account Name:</strong> {{ activeAccount ? activeAccount.name : 'None' }}</p>
        </div>

        <div class="bg-gray-50 p-4 rounded-lg">
          <h3 class="text-lg font-semibold mb-2">Token Test</h3>
          <button 
            (click)="testTokenAcquisition()" 
            class="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
            [disabled]="loading">
            {{ loading ? 'Testing...' : 'Test Token Acquisition' }}
          </button>
          <pre *ngIf="tokenResult" class="mt-2 bg-white p-2 rounded text-sm">{{ tokenResult | json }}</pre>
        </div>

        <div class="bg-gray-50 p-4 rounded-lg">
          <h3 class="text-lg font-semibold mb-2">Graph API Test</h3>
          <button 
            (click)="testGraphApi()" 
            class="bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600"
            [disabled]="loading">
            {{ loading ? 'Testing...' : 'Test Graph API Call' }}
          </button>
          <pre *ngIf="graphResult" class="mt-2 bg-white p-2 rounded text-sm">{{ graphResult | json }}</pre>
        </div>

        <div class="bg-gray-50 p-4 rounded-lg">
          <h3 class="text-lg font-semibold mb-2">Login</h3>
          <button 
            (click)="login()" 
            class="bg-indigo-500 text-white px-4 py-2 rounded hover:bg-indigo-600"
            [disabled]="loading">
            Login
          </button>
        </div>
      </div>
    </div>
  `,
    standalone: true,
    imports: [CommonModule]
})
export class AuthTestComponent implements OnInit {
    loading = false
    isAuthenticated = false
    activeAccount: any = null
    tokenResult: any = null
    graphResult: any = null

    constructor(
        private msalService: MsalService,
        private userService: UserService
    ) { }

    ngOnInit(): void {
        this.checkAuthStatus()
    }

    checkAuthStatus(): void {
        this.isAuthenticated = this.userService.isAuthenticated()
        this.activeAccount = this.msalService.instance.getActiveAccount()
    }

    async testTokenAcquisition(): Promise<void> {
        this.loading = true
        this.tokenResult = null

        try {
            const account = this.msalService.instance.getActiveAccount()
            if (!account) {
                this.tokenResult = { error: 'No active account' }
                this.loading = false
                return
            }

            const result = await this.msalService.acquireTokenSilent({
                scopes: ['user.read'],
                account: account
            }).toPromise()

            if (!result) {
                this.tokenResult = { error: 'No token result received' }
                this.loading = false
                return
            }

            this.tokenResult = {
                success: true,
                hasToken: !!result.accessToken,
                tokenLength: result.accessToken ? result.accessToken.length : 0,
                scopes: result.scopes,
                account: result.account?.username
            }
        } catch (error) {
            this.tokenResult = { error: error }
        }

        this.loading = false
    }

    testGraphApi(): void {
        this.loading = true
        this.graphResult = null

        this.userService.getUserProfile().subscribe({
            next: (profile) => {
                this.graphResult = {
                    success: true,
                    profile: profile
                }
                this.loading = false
            },
            error: (error) => {
                this.graphResult = {
                    error: error,
                    status: error.status,
                    message: error.message
                }
                this.loading = false
            }
        })
    }

    async login(): Promise<void> {
        try {
            await this.userService.login()
        } catch (error) {
            console.error('Login error:', error)
        }
    }
}
