import { NgModule } from "@angular/core"
import { CommonModule } from "@angular/common"
import { FormsModule } from "@angular/forms"
import { Login } from "./login"
import { LoginRoutingModule } from "./login-routing.module"

@NgModule({
    imports: [
        LoginRoutingModule,
        CommonModule,
        FormsModule
    ],
    exports: [
        Login
    ],
    declarations: [
        Login
    ],
    providers: []
})
export class LoginModule { }