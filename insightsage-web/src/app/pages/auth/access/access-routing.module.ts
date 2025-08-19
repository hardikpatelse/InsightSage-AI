import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { Access } from './access'

const routes: Routes = [
    { path: '', component: Access }
]

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class AccessRoutingModule { }
