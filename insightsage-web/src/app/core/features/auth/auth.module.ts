import { NgModule } from "@angular/core"
import { CommonModule } from "@angular/common"
import { Auth } from "./auth"
import { AuthRoutingModule } from "./auth-routing.module"

@NgModule({
    imports: [
        AuthRoutingModule,
        CommonModule
    ],
    exports: [
        Auth
    ],
    declarations: [
        Auth
    ],
    providers: []
})
export class AuthModule { }