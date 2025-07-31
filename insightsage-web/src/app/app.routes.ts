import { Routes } from '@angular/router'
import { MsalGuard } from '@azure/msal-angular'
import { Chat } from './core/features/chat/chat'
import { Dashboard } from './core/features/dashboard/dashboard'

export const routes: Routes = [
    {
        path: 'chat',
        component: Chat,
        canActivate: [MsalGuard]
    },
    {
        path: 'dashboard',
        component: Dashboard,
        canActivate: [MsalGuard]
    },
    { path: '', redirectTo: 'chat', pathMatch: 'full' }
]
