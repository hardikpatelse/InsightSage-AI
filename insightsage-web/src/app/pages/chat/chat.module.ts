import { NgModule } from '@angular/core'
import { CommonModule } from '@angular/common'
import { FormsModule, ReactiveFormsModule } from '@angular/forms'

import { ChatRoutingModule } from './chat-routing.module'
import { Chat } from './chat'
import { ChatHistoryModule } from '../../components/chat-history/chat-history.module'

// PrimeNG Imports
import { ButtonModule } from 'primeng/button'
import { InputTextModule } from 'primeng/inputtext'
import { ScrollPanelModule } from 'primeng/scrollpanel'
import { SelectModule } from 'primeng/select'
import { TooltipModule } from 'primeng/tooltip'
import { AvatarModule } from 'primeng/avatar'
import { CardModule } from 'primeng/card'
import { DividerModule } from 'primeng/divider'
import { ChipModule } from 'primeng/chip'
import { ProgressBarModule } from 'primeng/progressbar'
import { SkeletonModule } from 'primeng/skeleton'
import { FluidModule } from 'primeng/fluid'

@NgModule({
    declarations: [
        Chat
    ],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        ChatRoutingModule,
        ButtonModule,
        InputTextModule,
        ScrollPanelModule,
        SelectModule,
        TooltipModule,
        AvatarModule,
        CardModule,
        DividerModule,
        ChipModule,
        ProgressBarModule,
        SkeletonModule,
        FluidModule,
        ChatHistoryModule
    ]
})
export class ChatModule { }
