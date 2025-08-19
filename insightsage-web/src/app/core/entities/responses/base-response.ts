import { HttpStatusCode } from "../../enums/http-status-code"

export class BaseResponse {
    /**
     * HTTP status code (200 for success, others for various error types)
     */
    status!: HttpStatusCode

    /**
     * Array of error messages (empty for successful requests)
     */
    errors?: string[] | null

    /**
    * Detailed exception information (null for successful requests)
    */
    exceptionDetails?: string | null
}