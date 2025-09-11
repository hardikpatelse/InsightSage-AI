import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core'
import { FormBuilder, FormGroup, Validators } from '@angular/forms'
import { AIModel } from '@/core/entities/ai-model'
import { ChatMode } from '@/core/entities/chat-mode'
import { ChatSessionRequest } from '@/core/entities/requests/chat-session-request'

@Component({
    selector: 'app-new-chat-modal',
    templateUrl: './new-chat-modal.component.html',
    styleUrls: ['./new-chat-modal.component.scss'],
    standalone: false
})
export class NewChatModalComponent implements OnInit {
    @Input() visible: boolean = false
    @Input() aiModels: AIModel[] = []
    @Input() chatModes: ChatMode[] = []

    @Output() visibleChange = new EventEmitter<boolean>()
    @Output() chatSessionCreate = new EventEmitter<ChatSessionRequest>()
    @Output() modalClose = new EventEmitter<void>()

    newChatForm: FormGroup

    constructor(private formBuilder: FormBuilder) {
        this.newChatForm = this.formBuilder.group({
            sessionName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
            aiModelId: [null, Validators.required],
            chatModeId: [null, Validators.required]
        })
    }

    ngOnInit(): void {
        // Set default selections if available
        if (this.aiModels.length > 0 && !this.newChatForm.get('aiModelId')?.value) {
            this.newChatForm.patchValue({ aiModelId: this.aiModels[0].id })
        }

        if (this.chatModes.length > 0 && !this.newChatForm.get('chatModeId')?.value) {
            this.newChatForm.patchValue({ chatModeId: this.chatModes[0].id })
        }
    }

    onSubmit(): void {
        if (this.newChatForm.valid) {
            const formValue = this.newChatForm.value

            const chatSessionRequest: ChatSessionRequest = {
                id: 0,
                name: formValue.sessionName,
                defaultAIModelId: formValue.aiModelId,
                defaultChatModeId: formValue.chatModeId
            }

            this.chatSessionCreate.emit(chatSessionRequest)
            this.onClose()
        } else {
            // Mark all fields as touched to show validation errors
            Object.keys(this.newChatForm.controls).forEach(key => {
                this.newChatForm.get(key)?.markAsTouched()
            })
        }
    }

    onClose(): void {
        this.visible = false
        this.visibleChange.emit(false)
        this.modalClose.emit()
        this.resetForm()
    }

    private resetForm(): void {
        this.newChatForm.reset()

        // Reset to default selections
        if (this.aiModels.length > 0) {
            this.newChatForm.patchValue({ aiModelId: this.aiModels[0].id })
        }

        if (this.chatModes.length > 0) {
            this.newChatForm.patchValue({ chatModeId: this.chatModes[0].id })
        }
    }

    // Helper methods for template
    isFieldInvalid(fieldName: string): boolean {
        const field = this.newChatForm.get(fieldName)
        return !!(field && field.invalid && (field.dirty || field.touched))
    }

    getFieldError(fieldName: string): string {
        const field = this.newChatForm.get(fieldName)
        if (field && field.errors) {
            if (field.errors['required']) {
                return `${this.getFieldDisplayName(fieldName)} is required`
            }
            if (field.errors['minlength']) {
                return `${this.getFieldDisplayName(fieldName)} must be at least ${field.errors['minlength'].requiredLength} characters`
            }
            if (field.errors['maxlength']) {
                return `${this.getFieldDisplayName(fieldName)} must not exceed ${field.errors['maxlength'].requiredLength} characters`
            }
        }
        return ''
    }

    private getFieldDisplayName(fieldName: string): string {
        switch (fieldName) {
            case 'sessionName': return 'Chat Session Name'
            case 'aiModelId': return 'AI Model'
            case 'chatModeId': return 'Chat Mode'
            default: return fieldName
        }
    }

    // Get selected model/mode for display
    getSelectedAIModel(): AIModel | null {
        const selectedId = this.newChatForm.get('aiModelId')?.value
        return this.aiModels.find(model => model.id === selectedId) || null
    }

    getSelectedChatMode(): ChatMode | null {
        const selectedId = this.newChatForm.get('chatModeId')?.value
        return this.chatModes.find(mode => mode.id === selectedId) || null
    }
}
