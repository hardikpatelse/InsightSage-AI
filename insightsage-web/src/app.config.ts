import { HTTP_INTERCEPTORS, provideHttpClient, withFetch, withInterceptorsFromDi } from '@angular/common/http'
import { ApplicationConfig, importProvidersFrom, provideBrowserGlobalErrorListeners } from '@angular/core'
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async'
import { provideRouter, withEnabledBlockingInitialNavigation, withInMemoryScrolling } from '@angular/router'
import Aura from '@primeuix/themes/aura'
import { providePrimeNG } from 'primeng/config'
import { appRoutes } from './app.routes'
import {
    MsalModule,
    MsalService,
    MsalGuard,
    MSAL_INSTANCE,
    MSAL_GUARD_CONFIG,
    MSAL_INTERCEPTOR_CONFIG,
    MsalGuardConfiguration,
    MsalInterceptorConfiguration
} from '@azure/msal-angular'
import {
    IPublicClientApplication,
    PublicClientApplication,
    InteractionType,
    BrowserCacheLocation
} from '@azure/msal-browser'
import { msalConfig } from '@/msal/msal-config'
import { AuthInterceptor } from '@/core/interceptors/auth.interceptor'
import { ApiResponseInterceptor } from '@/core/interceptors/api-response.interceptor'
import { ErrorInterceptor } from '@/core/interceptors/error.interceptor'

export function MSALInstanceFactory(): IPublicClientApplication {
    return new PublicClientApplication(msalConfig)
}

export function MSALGuardConfigFactory(): MsalGuardConfiguration {
    return {
        interactionType: InteractionType.Redirect,
        authRequest: {
            scopes: ['user.read']
        }
    }
}

export function MSALInterceptorConfigFactory(): MsalInterceptorConfiguration {
    const protectedResourceMap = new Map<string, Array<string>>()
    protectedResourceMap.set('https://graph.microsoft.com/v1.0/me', ['user.read'])

    return {
        interactionType: InteractionType.Redirect,
        protectedResourceMap
    }
}

export const appConfig: ApplicationConfig = {
    providers: [
        provideRouter(appRoutes, withInMemoryScrolling({ anchorScrolling: 'enabled', scrollPositionRestoration: 'enabled' }), withEnabledBlockingInitialNavigation()),
        provideHttpClient(withInterceptorsFromDi()),
        provideAnimationsAsync(),
        provideBrowserGlobalErrorListeners(),
        providePrimeNG({ theme: { preset: Aura, options: { darkModeSelector: '.app-dark' } } }),
        importProvidersFrom(MsalModule),
        {
            provide: MSAL_INSTANCE,
            useFactory: MSALInstanceFactory
        },
        {
            provide: MSAL_GUARD_CONFIG,
            useFactory: MSALGuardConfigFactory
        },
        // Temporarily disable MSAL interceptor to avoid conflicts
        {
            provide: MSAL_INTERCEPTOR_CONFIG,
            useFactory: MSALInterceptorConfigFactory
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthInterceptor,
            multi: true
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: ApiResponseInterceptor,
            multi: true
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: ErrorInterceptor,
            multi: true
        },
        MsalService,
        MsalGuard
    ]
}
