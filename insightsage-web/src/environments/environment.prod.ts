export const environment = {
    production: true,
    authApiBaseUrl: 'https://api.your-production-domain.com', // Example API base URL, adjust as needed
    chatApiBaseUrl: 'https://chat-api.your-production-domain.com', // Chat API base URL for production
    msal: {
        clientId: 'YOUR_CLIENT_ID',  // From App registration
        authority: 'https://login.microsoftonline.com/<YOUR_TENANT_ID>/v2.0',
        redirectUri: 'https://your-production-domain.com',
        postLogoutRedirectUri: 'https://your-production-domain.com'
    }
}
