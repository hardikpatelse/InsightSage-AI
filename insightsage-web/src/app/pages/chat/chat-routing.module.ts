import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { Chat } from './chat'

const routes: Routes = [
    { path: '', component: Chat },
    { path: ':sessionId', component: Chat }
]

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class ChatRoutingModule { }
