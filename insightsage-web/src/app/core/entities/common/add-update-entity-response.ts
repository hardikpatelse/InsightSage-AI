import { ApiResponse } from "../responses/api-response"
/**
 * Standard API response for add/update operations
 * Contains the Result ID of the added/updated entity
 **/
export class AddUpdateEntityResponse extends ApiResponse<number> {
}