import { Component, signal, OnInit } from '@angular/core'
import { Router, NavigationEnd, RouterOutlet } from '@angular/router'
import { MsalService } from '@azure/msal-angular'
import { Header } from "./core/components/header/header"
import { Sidebar } from './core/components/sidebar/sidebar'
import { NotificationComponent } from './core/components/notification/notification'
import { UserService } from './core/services/user.service'
import { User } from './core/entities/user'
import { CommonModule } from '@angular/common'
import { filter } from 'rxjs/operators'

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Header, CommonModule, NotificationComponent, Sidebar],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  protected readonly title = signal('insightsage-web');
  user: User | null = null;
  isLoginPage = false;

  constructor(
    private msalService: MsalService,
    private userService: UserService,
    private router: Router
  ) { }

  ngOnInit(): void {
    // Subscribe to user changes
    this.userService.currentUser$.subscribe(user => {
      this.user = user
      if (user) {
        console.log('Current user:', user)
        // If user is authenticated and on login page, redirect to chat
        if (this.isLoginPage) {
          this.router.navigate(['/chat'])
        }
      }
    })

    // Subscribe to route changes to track login page
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.isLoginPage = event.url.includes('/login')
      })

    // Initialize user if already logged in
    this.userService.getCurrentUser().subscribe()
  }

  login(): void {
    this.userService.login()
  }

  logout(): void {
    this.userService.logout()
  }
}
