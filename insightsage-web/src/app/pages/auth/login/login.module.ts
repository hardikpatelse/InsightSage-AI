import { NgModule } from '@angular/core'
import { CommonModule } from '@angular/common'
import { FormsModule } from '@angular/forms'
import { ButtonModule } from 'primeng/button'
import { CheckboxModule } from 'primeng/checkbox'
import { InputTextModule } from 'primeng/inputtext'
import { PasswordModule } from 'primeng/password'
import { RippleModule } from 'primeng/ripple'

import { Login } from './login'
import { AppFloatingConfigurator } from '@/layout/component/app.floatingconfigurator'
import { LoginRoutingModule } from './login-routing.module'

@NgModule({
    declarations: [
        Login
    ],
    imports: [
        CommonModule,
        FormsModule,
        ButtonModule,
        CheckboxModule,
        InputTextModule,
        PasswordModule,
        RippleModule,
        LoginRoutingModule,
        AppFloatingConfigurator
    ]
})
export class LoginModule { }
