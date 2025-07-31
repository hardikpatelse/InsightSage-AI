import { Component } from '@angular/core'
import { MsalService } from '@azure/msal-angular'

@Component({
  selector: 'app-login',
  imports: [],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {

  constructor(private msalService: MsalService) {

  }

  login(): void {
    this.msalService.loginRedirect()
  }
}
