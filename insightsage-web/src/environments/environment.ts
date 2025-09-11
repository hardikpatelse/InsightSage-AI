export const environment = {
    production: false,
    backendDomain: 'localhost', // Update this to match your backend API domain
    authApiBaseUrl: 'https://localhost:7038/api', // Example API base URL, adjust as needed
    chatApiBaseUrl: 'https://localhost:7039/api', // Example Chat API base URL, adjust as needed
    // MSAL configuration for Azure AD authentication
    msal: {
        clientId: '2d58b530-b2d9-47a5-8536-d8ac7bac9030',  // From App registration
        authority: 'https://login.microsoftonline.com/7ea1b63f-1f4a-4c38-a048-96de9c3ad2ea/v2.0',
        redirectUri: 'http://localhost:4200',
        postLogoutRedirectUri: 'http://localhost:4200'
    }
}
