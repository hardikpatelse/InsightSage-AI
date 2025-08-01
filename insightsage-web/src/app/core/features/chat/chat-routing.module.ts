import { RouterModule, Routes } from "@angular/router"
import { NgModule } from "@angular/core"
import { Chat } from "./chat"

const routes: Routes = [
    { path: '', component: Chat },
]

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class ChatRoutingModule { }
