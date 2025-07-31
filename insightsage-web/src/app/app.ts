import { Component, signal } from '@angular/core'
import { RouterOutlet } from '@angular/router'
import { MsalService } from '@azure/msal-angular'
import { Header } from "./core/components/header/header"

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Header],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('insightsage-web');
  user: any = null;
  constructor(private msalService: MsalService) {
    // Initialize user information
    this.user = this.msalService.instance.getActiveAccount()
    const claims = this.user ? this.user.idTokenClaims : null
    if (claims) {
      console.log('User claims:', claims)
    } else {
      console.warn('No active account found. User claims are unavailable.')
    }
// Removed unused userInfo object
  }

}
