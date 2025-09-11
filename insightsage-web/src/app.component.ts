import { User } from '@/core/entities/user'
import { UserService } from '@/core/services/user.service'
import { Component, signal } from '@angular/core'
import { NavigationEnd, Router, RouterModule } from '@angular/router'
import { MsalService } from '@azure/msal-angular'
import { filter } from 'rxjs'

@Component({
    selector: 'app-root',
    standalone: true,
    imports: [RouterModule],
    template: `<router-outlet></router-outlet>`
})
export class AppComponent {
    protected readonly title = signal('insightsage-web');
    user: User | null = null;
    isLoginPage = false;
    isAuthPage = false;
    isLandingPage = false;
    isNotFoundPage = false;

    constructor(
        private msalService: MsalService,
        private userService: UserService,
        private router: Router
    ) { }

    ngOnInit() {
        // Subscribe to user changes
        this.userService.currentUser$.subscribe(user => {
            this.user = user
            if (user) {
                // If user is authenticated and on login page, redirect to dashboard
                if (this.isLoginPage) {
                    this.router.navigate(['/'])
                }
            } else {
                // If no user and not on auth/landing/notfound pages, redirect to login
                if (!this.isAuthPage && !this.isLandingPage && !this.isNotFoundPage) {
                    this.router.navigate(['/auth/login'])
                }
            }
        })

        // Subscribe to route changes to track current page
        this.router.events
            .pipe(filter(event => event instanceof NavigationEnd))
            .subscribe((event: NavigationEnd) => {
                this.isLoginPage = event.url.includes('/login')
                this.isAuthPage = event.url.startsWith('/auth')
                this.isLandingPage = event.url === '/landing'
                this.isNotFoundPage = event.url === '/notfound'
            })

        // Initialize user if already logged in
        this.userService.getCurrentUser().subscribe({
            error: (error) => {
                console.error('App: Error initializing user:', error)
                // If there's an error getting user, redirect to login
                if (!this.isAuthPage && !this.isLandingPage && !this.isNotFoundPage) {
                    this.router.navigate(['/auth/login'])
                }
            }
        })
    }
}
