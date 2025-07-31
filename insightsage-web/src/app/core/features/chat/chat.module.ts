import { NgModule } from "@angular/core"
import { CommonModule } from "@angular/common"
import { Chat } from "./chat"
import { ChatRoutingModule } from "./chat-routing.module"
import { FormsModule } from "@angular/forms"

@NgModule({
    imports: [
        ChatRoutingModule,
        CommonModule,
        FormsModule
    ],
    exports: [
        Chat
    ],
    declarations: [
        Chat
    ],
    providers: []
})
export class ChatModule { }