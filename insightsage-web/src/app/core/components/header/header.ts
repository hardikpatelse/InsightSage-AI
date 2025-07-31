import { Component } from '@angular/core'
import { MsalService } from '@azure/msal-angular'

@Component({
  selector: 'app-header',
  imports: [],
  templateUrl: './header.html',
  styleUrl: './header.scss'
})
export class Header {
  tenantName: string = 'Default Tenant' // Placeholder for tenant name, can be dynamically set
  constructor(private msalService: MsalService) {

  }

  logout(): void {
    this.msalService.logoutRedirect()
  }
}
