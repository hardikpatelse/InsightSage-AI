import { Routes } from '@angular/router'
import { MsalGuard } from '@azure/msal-angular'

export const routes: Routes = [
    {
        path: 'login',
        loadChildren: () => import('./core/features/login/login.module').then(m => m.LoginModule)
    },
    {
        path: 'chat',
        loadChildren: () => import('./core/features/chat/chat.module').then(m => m.ChatModule),
        canActivate: [MsalGuard]
    },
    {
        path: 'dashboard',
        loadChildren: () => import('./core/features/dashboard/dashboard.module').then(m => m.DashboardModule),
        canActivate: [MsalGuard]
    },
    {
        path: '',
        redirectTo: '/login',
        pathMatch: 'full'
    },
    {
        path: '**',
        redirectTo: '/login'
    }
]
