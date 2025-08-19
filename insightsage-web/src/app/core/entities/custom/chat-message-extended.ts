import { RolesEnum } from "@/core/enums/role"
import { ChatMessage } from "../chat-message"

export class ChatMessageExtended extends ChatMessage {
    roleEnum: RolesEnum = RolesEnum.User
    isUser: boolean = false
    isAssistant: boolean = false
    isSystem: boolean = false
    timestamp: Date | null = null

    constructor(init?: Partial<ChatMessageExtended>) {
        super(init)

        // Set the roleEnum based on the role string property
        this.setRoleEnum()

        // Set the timestamp from createdAt property
        this.setTimestamp()
    }

    /**
     * Sets the timestamp property from the createdAt property
     */
    private setTimestamp(): void {
        if (this.createdAt) {
            // If createdAt is a string, parse it to Date
            this.timestamp = typeof this.createdAt === 'string'
                ? new Date(this.createdAt)
                : this.createdAt as Date
        } else {
            // Default to current time if no createdAt is available
            this.timestamp = new Date()
        }
    }

    /**
     * Sets the roleEnum property based on the role string value
     */
    private setRoleEnum(): void {
        if (!this.role) {
            this.roleEnum = RolesEnum.User // Default to User if no role specified
            return
        }

        switch (this.role.toLowerCase()) {
            case 'user':
                this.roleEnum = RolesEnum.User
                this.isUser = true
                break
            case 'assistant':
                this.roleEnum = RolesEnum.Assistant
                this.isAssistant = true
                break
            case 'system':
                this.roleEnum = RolesEnum.System
                this.isSystem = true
                break
            default:
                this.roleEnum = RolesEnum.User // Default to User for unknown roles
                this.isUser = true // Default to User for unknown roles
                console.warn(`Unknown role: ${this.role}. Defaulting to User.`)
        }
    }

    /**
     * Method to update role and refresh roleEnum
     */
    setRole(value: string | null): void {
        this.role = value
        this.setRoleEnum()
    }

    /**
     * Method to update createdAt and refresh timestamp
     */
    setCreatedAt(value: string | Date | null): void {
        if (typeof value === 'string') {
            this.createdAt = value
        } else if (value instanceof Date) {
            this.createdAt = value.toISOString()
        } else {
            this.createdAt = undefined
        }
        this.setTimestamp()
    }

    /**
     * Static factory method to create ChatMessageExtended from ChatMessage
     */
    static fromChatMessage(chatMessage: ChatMessage): ChatMessageExtended {
        return new ChatMessageExtended({
            id: chatMessage.id,
            createdAt: chatMessage.createdAt,
            updatedAt: chatMessage.updatedAt,
            chatSessionId: chatMessage.chatSessionId,
            role: chatMessage.role,
            content: chatMessage.content
        })
    }

    /**
     * Static factory method to create multiple ChatMessageExtended from ChatMessage array
     */
    static fromChatMessages(chatMessages: ChatMessage[]): ChatMessageExtended[] {
        return chatMessages.map(msg => ChatMessageExtended.fromChatMessage(msg))
    }

    /**
     * Helper method to check if the message is from a user
     */
    isUserMessage(): boolean {
        return this.roleEnum === RolesEnum.User
    }

    /**
     * Helper method to check if the message is from an assistant
     */
    isAssistantMessage(): boolean {
        return this.roleEnum === RolesEnum.Assistant
    }

    /**
     * Helper method to check if the message is a system message
     */
    isSystemMessage(): boolean {
        return this.roleEnum === RolesEnum.System
    }

    /**
     * Get the display name for the role
     */
    getRoleDisplayName(): string {
        switch (this.roleEnum) {
            case RolesEnum.User:
                return 'User'
            case RolesEnum.Assistant:
                return 'Assistant'
            case RolesEnum.System:
                return 'System'
            default:
                return 'Unknown'
        }
    }

    /**
     * Get CSS class for role-based styling
     */
    getRoleStyleClass(): string {
        switch (this.roleEnum) {
            case RolesEnum.User:
                return 'user-message'
            case RolesEnum.Assistant:
                return 'assistant-message'
            case RolesEnum.System:
                return 'system-message'
            default:
                return 'unknown-message'
        }
    }

    /**
     * Format timestamp for display (HH:MM format)
     */
    formatTime(): string {
        if (!this.timestamp) {
            return ''
        }
        return this.timestamp.toLocaleTimeString('en-US', {
            hour: '2-digit',
            minute: '2-digit'
        })
    }

    /**
     * Format timestamp for display (full date and time)
     */
    formatDateTime(): string {
        if (!this.timestamp) {
            return ''
        }
        return this.timestamp.toLocaleString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        })
    }

    /**
     * Get relative time string (e.g., "2 minutes ago")
     */
    getRelativeTime(): string {
        if (!this.timestamp) {
            return ''
        }

        const now = new Date()
        const diffMs = now.getTime() - this.timestamp.getTime()
        const diffMins = Math.floor(diffMs / (1000 * 60))
        const diffHours = Math.floor(diffMs / (1000 * 60 * 60))
        const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24))

        if (diffMins < 1) {
            return 'Just now'
        } else if (diffMins < 60) {
            return `${diffMins} minute${diffMins === 1 ? '' : 's'} ago`
        } else if (diffHours < 24) {
            return `${diffHours} hour${diffHours === 1 ? '' : 's'} ago`
        } else {
            return `${diffDays} day${diffDays === 1 ? '' : 's'} ago`
        }
    }
}