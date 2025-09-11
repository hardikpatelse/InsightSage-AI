import { User } from '@/core/entities/user'
import { NotificationService } from '@/core/services/notification.service'
import { UserService } from '@/core/services/user.service'
import { Component } from '@angular/core'
import { Router } from '@angular/router'

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  email: string = '';
  password: string = '';
  checked: boolean = false;
  user: User | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private userService: UserService,
    private router: Router,
    private notificationService: NotificationService
  ) { }

  ngOnInit(): void {
    console.log('Login component initialized')
    // Check if user is already logged in
    this.userService.currentUser$.subscribe(user => {
      console.log('Current user updated:', user)
      this.user = user
      if (user) {
        // User is already logged in, redirect to chat
        console.log('User already logged in, redirecting to chat')
        this.router.navigate(['/chat'])
        // .then(success => {
        //   console.log('Auto-navigation result:', success)
        // }).catch(navError => {
        //   console.error('Auto-navigation error:', navError)
        // })
      }
    })

    // Initialize user check
    console.log('Getting current user...')
    this.userService.getCurrentUser().subscribe({
      next: (user) => {
        console.log('getCurrentUser result:', user)
      },
      error: (error) => {
        console.error('getCurrentUser error:', error)
      }
    })
  }


  loginWithPopup(): void {
    this.isLoading = true
    this.errorMessage = null
    console.log('Login popup initiated...')

    this.userService.loginPopup().subscribe({
      next: (user) => {
        console.log('Login popup successful:', user)
        this.isLoading = false
        if (user) {
          console.log('Navigating to /chat...')
          // Use window.location as fallback if Angular routing fails
          this.router.navigate(['/chat']).then(success => {
            console.log('Navigation result:', success)
            if (!success) {
              console.log('Angular navigation failed, trying window.location')
              window.location.href = '/chat'
            }
          }).catch(navError => {
            console.error('Navigation error:', navError)
            console.log('Using window.location as fallback')
            window.location.href = '/chat'
          })
        } else {
          console.warn('User is null after login')
        }
      },
      error: (error) => {
        console.error('Login popup error:', error)
        this.isLoading = false
        this.errorMessage = 'Login failed. Please check your credentials and try again.'
        this.notificationService.showError('Login failed. Please check your credentials and try again.', 'Authentication Error')
        console.error('Login failed:', error)
      }
    })
  }
}
