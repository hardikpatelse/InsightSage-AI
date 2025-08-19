export class AIModel {
    id: number
    name: string
    value: string
    description: string

    constructor(id: number, name: string, value: string, description: string) {
        this.id = id
        this.name = name
        this.value = value
        this.description = description
    }
}