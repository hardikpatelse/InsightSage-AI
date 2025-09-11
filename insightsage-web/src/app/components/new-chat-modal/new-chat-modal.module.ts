import { NgModule } from '@angular/core'
import { CommonModule } from '@angular/common'
import { ReactiveFormsModule } from '@angular/forms'

// PrimeNG Imports
import { DialogModule } from 'primeng/dialog'
import { ButtonModule } from 'primeng/button'
import { InputTextModule } from 'primeng/inputtext'
import { SelectModule } from 'primeng/select'
import { ChipModule } from 'primeng/chip'

import { NewChatModalComponent } from './new-chat-modal.component'

@NgModule({
    declarations: [
        NewChatModalComponent
    ],
    imports: [
        CommonModule,
        ReactiveFormsModule,
        DialogModule,
        ButtonModule,
        InputTextModule,
        SelectModule,
        ChipModule
    ],
    exports: [
        NewChatModalComponent
    ]
})
export class NewChatModalModule { }
