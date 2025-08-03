export interface User {
    id: number
    userId?: string
    name: string
    email: string
    tenantId: string
    objectId: string
    jobTitle?: string
    department?: string
    officeLocation?: string
    mobilePhone?: string
    businessPhones?: string[]
    lastLoginDate?: string
    createdDate?: string
    updatedDate?: string
}