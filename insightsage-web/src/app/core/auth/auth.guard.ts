import { Injectable } from '@angular/core'
import { CanActivate } from '@angular/router'
import { MsalService } from '@azure/msal-angular'

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
    constructor(private msalService: MsalService) { }

    canActivate(): boolean {
        const account = this.msalService.instance.getActiveAccount()
        return !!account
    }
}
