import { NgModule } from '@angular/core'
import { CommonModule } from '@angular/common'

// PrimeNG Imports
import { ButtonModule } from 'primeng/button'
import { InputTextModule } from 'primeng/inputtext'
import { ScrollPanelModule } from 'primeng/scrollpanel'
import { AvatarModule } from 'primeng/avatar'
import { CardModule } from 'primeng/card'
import { ChipModule } from 'primeng/chip'
import { ProgressBarModule } from 'primeng/progressbar'
import { SkeletonModule } from 'primeng/skeleton'
import { ChatHistory } from './chat-history'

@NgModule({
    declarations: [
        ChatHistory
    ],
    imports: [
        CommonModule,
        ButtonModule,
        InputTextModule,
        ScrollPanelModule,
        AvatarModule,
        CardModule,
        ChipModule,
        ProgressBarModule,
        SkeletonModule
    ],
    exports: [
        ChatHistory
    ]
})
export class ChatHistoryModule { }
