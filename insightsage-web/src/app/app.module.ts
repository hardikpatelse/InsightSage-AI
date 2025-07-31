import {
    MsalModule,
    MsalRedirectComponent,
    MsalGuard,
    MsalInterceptor,
    MSAL_INSTANCE,
    MSAL_GUARD_CONFIG
} from '@azure/msal-angular'
import { InteractionType, PublicClientApplication } from '@azure/msal-browser'
import { HTTP_INTERCEPTORS, HttpInterceptor } from '@angular/common/http'
import { msalConfig } from './core/auth/msal-config'
import { App } from './app'
import { NgModule } from '@angular/core'

export function MSALInstanceFactory() {
    return new PublicClientApplication(msalConfig)
}

@NgModule({
    declarations: [],
    imports: [
        MsalModule
    ],
    providers: [
        {
            provide: HTTP_INTERCEPTORS,
            useClass: MsalInterceptor,
            multi: true
        },
        {
            provide: MSAL_INSTANCE,
            useFactory: MSALInstanceFactory
        },
        MsalGuard,
        {
            provide: MSAL_GUARD_CONFIG,
            useFactory: () => ({
                interactionType: InteractionType.Redirect,
                authRequest: {
                    scopes: []
                }
            })
        }
    ],
    bootstrap: [MsalRedirectComponent]
})
export class AppModule { }

