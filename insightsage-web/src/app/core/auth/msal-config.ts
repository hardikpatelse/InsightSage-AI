import { Configuration, LogLevel } from '@azure/msal-browser'
import { environment } from '../../../environments/environment'

export const msalConfig: Configuration = {
    auth: {
        clientId: environment.msal.clientId,
        authority: environment.msal.authority,
        redirectUri: environment.msal.redirectUri,
        postLogoutRedirectUri: environment.msal.postLogoutRedirectUri,
        navigateToLoginRequestUrl: false
    },
    cache: {
        cacheLocation: 'localStorage',
        storeAuthStateInCookie: false,
        secureCookies: false
    },
    system: {
        loggerOptions: {
            loggerCallback: (level: LogLevel, message: string, containsPii: boolean) => {
                if (containsPii) {
                    return
                }
                switch (level) {
                    case LogLevel.Error:
                        console.error('MSAL Error:', message)
                        return
                    case LogLevel.Info:
                        console.info('MSAL Info:', message)
                        return
                    case LogLevel.Verbose:
                        console.debug('MSAL Verbose:', message)
                        return
                    case LogLevel.Warning:
                        console.warn('MSAL Warning:', message)
                        return
                }
            },
            logLevel: LogLevel.Verbose,
            piiLoggingEnabled: false
        },
        windowHashTimeout: 60000,
        iframeHashTimeout: 6000,
        loadFrameTimeout: 0
    }
}
