import { Injectable } from '@angular/core'
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router'
import { Observable, of, catchError, map, tap } from 'rxjs'
import { MsalService } from '@azure/msal-angular'
import { UserService } from '../core/services/user.service'

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
    constructor(
        private msalService: MsalService,
        private userService: UserService,
        private router: Router
    ) { }

    canActivate(
        route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot
    ): Observable<boolean> | boolean {
        console.log('AuthGuard: Validating access to:', state.url)

        return this.userService.validateUserAuthentication().pipe(
            tap(isValid => {
                if (!isValid) {
                    console.log('AuthGuard: Authentication validation failed, redirecting to login')
                    this.router.navigate(['/auth/login'])
                } else {
                    console.log('AuthGuard: Authentication validation successful')
                }
            }),
            catchError(error => {
                console.error('AuthGuard: Error during authentication validation:', error)
                this.router.navigate(['/auth/login'])
                return of(false)
            })
        )
    }
}
