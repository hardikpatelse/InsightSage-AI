import { NgModule } from '@angular/core'
import { CommonModule } from '@angular/common'
import { Dashboard } from './dashboard'
import { DashboardRoutingModule } from './dashboard-routing.module'

@NgModule({
    imports: [
        DashboardRoutingModule,
        CommonModule
    ],
    exports: [
        Dashboard
    ],
    declarations: [
        Dashboard
    ],
    providers: []
})
export class DashboardModule { }