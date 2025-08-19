import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { Error } from './error'

const routes: Routes = [
    { path: '', component: Error }
]

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class ErrorRoutingModule { }
