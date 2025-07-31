export const environment = {
    production: true,
    authApiBaseUrl: 'https://api.your-production-domain.com', // Example API base URL, adjust as needed
    chatApiBaseUrl: 'https://chat-api.your-production-domain.com', // Chat API base URL for production
    msal: {
        clientId: 'YOUR_CLIENT_ID',  // From App registration
        authority: 'https://entra.microsoft.com/<YOUR_TENANT_ID>/oauth2/v2.0/authorize?p=signup_signin',
        redirectUri: 'https://your-production-domain.com',
        postLogoutRedirectUri: 'https://your-production-domain.com'
    }
}
