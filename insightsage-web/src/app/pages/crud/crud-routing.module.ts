import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { Crud } from './crud'

const routes: Routes = [
    { path: '', component: Crud }
]

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class CrudRoutingModule { }
