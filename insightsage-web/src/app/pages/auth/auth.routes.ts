import { Routes } from '@angular/router'

export default [
    { path: 'access', loadChildren: () => import('./access/access.module').then(m => m.AccessModule) },
    { path: 'error', loadChildren: () => import('./error/error.module').then(m => m.ErrorModule) },
    { path: 'login', loadChildren: () => import('./login/login.module').then(m => m.LoginModule) }
] as Routes
