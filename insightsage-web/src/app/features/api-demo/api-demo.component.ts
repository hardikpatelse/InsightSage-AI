import { Component } from '@angular/core'
import { CommonModule } from '@angular/common'
import { ApiService } from '../../core/services/api.service'
import { ApiResponse } from '../../core/entities/api-response'
import { User } from '../../core/entities/user'
import { environment } from '../../../environments/environment'

@Component({
    selector: 'app-api-demo',
    template: `
    <div class="p-6 max-w-4xl mx-auto">
      <h2 class="text-2xl font-bold mb-6">API Response Handling Demo</h2>
      
      <div class="space-y-4">
        <div class="bg-gray-50 p-4 rounded-lg">
          <h3 class="text-lg font-semibold mb-2">Standard API Call (Auto-extracted result)</h3>
          <button 
            (click)="testStandardCall()" 
            class="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
            [disabled]="loading">
            {{ loading ? 'Loading...' : 'Test Standard API Call' }}
          </button>
          <pre *ngIf="standardResult" class="mt-2 bg-white p-2 rounded text-sm">{{ standardResult | json }}</pre>
        </div>

        <div class="bg-gray-50 p-4 rounded-lg">
          <h3 class="text-lg font-semibold mb-2">Full Response API Call (Complete response)</h3>
          <button 
            (click)="testFullResponseCall()" 
            class="bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600"
            [disabled]="loading">
            {{ loading ? 'Loading...' : 'Test Full Response Call' }}
          </button>
          <pre *ngIf="fullResponse" class="mt-2 bg-white p-2 rounded text-sm">{{ fullResponse | json }}</pre>
        </div>

        <div class="bg-gray-50 p-4 rounded-lg">
          <h3 class="text-lg font-semibold mb-2">Error Testing</h3>
          <button 
            (click)="testErrorCall()" 
            class="bg-red-500 text-white px-4 py-2 rounded hover:bg-red-600"
            [disabled]="loading">
            {{ loading ? 'Loading...' : 'Test Error Handling' }}
          </button>
          <p class="mt-2 text-sm text-gray-600">This will trigger an error to test the error handling system.</p>
        </div>
      </div>
    </div>
  `,
    standalone: true,
    imports: [CommonModule]
})
export class ApiDemoComponent {
    loading = false
    standardResult: any = null
    fullResponse: ApiResponse<any> | null = null

    constructor(private apiService: ApiService) { }

    testStandardCall(): void {
        this.loading = true
        this.standardResult = null

        // Example: Call an API that returns your standard format
        // The interceptor will automatically extract the 'result' field
        this.apiService.get<User>(environment.authApiBaseUrl, 'user/profile')
            .subscribe({
                next: (result) => {
                    this.standardResult = result
                    this.loading = false
                    console.log('Standard call result (auto-extracted):', result)
                },
                error: (error) => {
                    this.loading = false
                    console.error('Standard call error:', error)
                    // Error notifications will be shown automatically by the interceptor
                }
            })
    }

    testFullResponseCall(): void {
        this.loading = true
        this.fullResponse = null

        // Example: Get the full API response including status, errors, etc.
        this.apiService.getFullResponse<User>(environment.authApiBaseUrl, 'user/profile')
            .subscribe({
                next: (response) => {
                    this.fullResponse = response
                    this.loading = false
                    console.log('Full response:', response)
                },
                error: (error) => {
                    this.loading = false
                    console.error('Full response call error:', error)
                }
            })
    }

    testErrorCall(): void {
        this.loading = true

        // Example: Call an endpoint that will return an error
        this.apiService.get<any>(environment.authApiBaseUrl, 'nonexistent/endpoint')
            .subscribe({
                next: (result) => {
                    this.loading = false
                    console.log('Unexpected success:', result)
                },
                error: (error) => {
                    this.loading = false
                    console.error('Expected error:', error)
                    // The interceptor will automatically show error notifications
                }
            })
    }
}
