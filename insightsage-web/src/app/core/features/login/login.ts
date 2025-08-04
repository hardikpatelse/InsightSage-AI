import { Component, OnInit } from '@angular/core'
import { Router } from '@angular/router'
import { UserService } from '../../services/user.service'
import { NotificationService } from '../../services/notification.service'
import { User } from '../../entities/user'

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login implements OnInit {
  user: User | null = null;
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private userService: UserService,
    private router: Router,
    private notificationService: NotificationService
  ) { }

  ngOnInit(): void {
    // Check if user is already logged in
    this.userService.currentUser$.subscribe(user => {
      this.user = user
      if (user) {
        // User is already logged in, redirect to chat
        this.router.navigate(['/chat'])
      }
    })

    // Initialize user check
    this.userService.getCurrentUser().subscribe()
  }

  async login(): Promise<void> {
    this.isLoading = true
    this.errorMessage = null
    try {
      await this.userService.login()
      // Note: After redirect login, user will be redirected back to the app
      // and the MSAL guard will handle the authentication
    } catch (error) {
      this.isLoading = false
      this.errorMessage = 'Login failed. Please try again.'
      this.notificationService.showError('Failed to initiate login. Please try again.', 'Login Error')
      console.error('Login failed:', error)
    }
  }

  loginWithPopup(): void {
    this.isLoading = true
    this.errorMessage = null
    this.userService.loginPopup().subscribe({
      next: (user) => {
        this.isLoading = false
        if (user) {
          console.log('Login successful:', user)
          this.router.navigate(['/chat'])
        }
      },
      error: (error) => {
        this.isLoading = false
        this.errorMessage = 'Login failed. Please check your credentials and try again.'
        this.notificationService.showError('Login failed. Please check your credentials and try again.', 'Authentication Error')
        console.error('Login failed:', error)
      }
    })
  }
}
