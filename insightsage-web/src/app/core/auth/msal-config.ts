import { Configuration } from '@azure/msal-browser'
import { environment } from '../../../environments/environment'

export const msalConfig: Configuration = {
    auth: {
        clientId: environment.msal.clientId,
        authority: environment.msal.authority,
        redirectUri: environment.msal.redirectUri,
        postLogoutRedirectUri: environment.msal.postLogoutRedirectUri
    },
    cache: {
        cacheLocation: 'localStorage',
        storeAuthStateInCookie: false
    }
}
