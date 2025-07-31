import { RouterModule, Routes } from "@angular/router"
import { NgModule } from "@angular/core"
import { Auth } from "./auth"

const routes: Routes = [
    { path: '', component: Auth },
]

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class AuthRoutingModule { }
