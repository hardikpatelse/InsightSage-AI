import { Routes } from '@angular/router'
import { Documentation } from './documentation/documentation'
import { Empty } from './empty/empty'
import { MsalGuard } from '@azure/msal-angular'

export default [
    { path: 'documentation', component: Documentation, canActivate: [MsalGuard] },
    { path: 'crud', loadChildren: () => import('./crud/crud.module').then(m => m.CrudModule), canActivate: [MsalGuard] },
    { path: 'empty', component: Empty, canActivate: [MsalGuard] },
    { path: '**', redirectTo: '/notfound' }
] as Routes
