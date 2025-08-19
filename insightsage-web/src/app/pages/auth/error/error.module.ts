import { NgModule } from '@angular/core'
import { CommonModule } from '@angular/common'
import { ButtonModule } from 'primeng/button'
import { RippleModule } from 'primeng/ripple'

import { ErrorRoutingModule } from './error-routing.module'
import { Error } from './error'
import { AppFloatingConfigurator } from '@/layout/component/app.floatingconfigurator'

@NgModule({
    declarations: [
        Error
    ],
    imports: [
        CommonModule,
        ButtonModule,
        RippleModule,
        ErrorRoutingModule,
        AppFloatingConfigurator
    ]
})
export class ErrorModule { }
