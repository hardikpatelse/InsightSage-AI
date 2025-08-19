import { NgModule } from '@angular/core'
import { CommonModule } from '@angular/common'
import { ButtonModule } from 'primeng/button'
import { RippleModule } from 'primeng/ripple'

import { AccessRoutingModule } from './access-routing.module'
import { Access } from './access'
import { AppFloatingConfigurator } from '@/layout/component/app.floatingconfigurator'

@NgModule({
    declarations: [
        Access
    ],
    imports: [
        CommonModule,
        ButtonModule,
        RippleModule,
        AccessRoutingModule,
        AppFloatingConfigurator
    ]
})
export class AccessModule { }
