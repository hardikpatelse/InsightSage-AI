import { Component } from '@angular/core'
import { MsalService } from '@azure/msal-angular'

@Component({
  selector: 'app-auth',
  standalone: false,
  templateUrl: './auth.html',
  styleUrl: './auth.scss'
})
export class Auth {

  constructor(private msalService: MsalService) {

  }

  login(): void {
    this.msalService.loginRedirect()
  }

  logout(): void {
    this.msalService.logoutRedirect()
  }

}
