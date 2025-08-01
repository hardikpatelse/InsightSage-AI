export const environment = {
    production: false,
    authApiBaseUrl: 'https://localhost:7038', // Example API base URL, adjust as needed
    chatApiBaseUrl: 'https://localhost:7039', // Example Chat API base URL, adjust as needed
    // MSAL configuration for Azure AD authentication
    msal: {
        clientId: 'YOUR_CLIENT_ID',  // From App registration
        authority: 'https://login.microsoftonline.com/<YOUR_TENANT_ID>',
        redirectUri: 'http://localhost:4200/chat',
        postLogoutRedirectUri: 'http://localhost:4200'
    }
}
