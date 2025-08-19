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
