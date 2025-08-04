import { Injectable } from '@angular/core'
import { Observable, map, catchError } from 'rxjs'
import { HttpClient, HttpHeaders } from '@angular/common/http'
import { ApiResponse } from '../entities/api-response'

@Injectable({
    providedIn: 'root'
})
export class ApiService {
    private httpOptions = {
        headers: new HttpHeaders({
            'Content-Type': 'application/json',
        })
    }

    constructor(private http: HttpClient) { }

    /**
     * GET request that expects standard API response format
     * The interceptor will automatically extract the result on success
     */
    get<T>(baseUrl: string, endpoint: string, customHeaders?: any): Observable<T> {
        const options = customHeaders ? { headers: new HttpHeaders(customHeaders) } : this.httpOptions
        return this.http.get<T>(`${baseUrl}/${endpoint}`, options)
    }

    /**
     * POST request that expects standard API response format
     * The interceptor will automatically extract the result on success
     */
    post<T>(baseUrl: string, endpoint: string, data: any, customHeaders?: any): Observable<T> {
        const options = customHeaders
            ? { headers: new HttpHeaders({ ...this.httpOptions.headers, ...customHeaders }) }
            : this.httpOptions
        return this.http.post<T>(`${baseUrl}/${endpoint}`, data, options)
    }

    /**
     * PUT request that expects standard API response format
     * The interceptor will automatically extract the result on success
     */
    put<T>(baseUrl: string, endpoint: string, data: any, customHeaders?: any): Observable<T> {
        const options = customHeaders
            ? { headers: new HttpHeaders({ ...this.httpOptions.headers, ...customHeaders }) }
            : this.httpOptions
        return this.http.put<T>(`${baseUrl}/${endpoint}`, data, options)
    }

    /**
     * PATCH request that expects standard API response format
     * The interceptor will automatically extract the result on success
     */
    patch<T>(baseUrl: string, endpoint: string, data: any, customHeaders?: any): Observable<T> {
        const options = customHeaders
            ? { headers: new HttpHeaders({ ...this.httpOptions.headers, ...customHeaders }) }
            : this.httpOptions
        return this.http.patch<T>(`${baseUrl}/${endpoint}`, data, options)
    }

    /**
     * DELETE request that expects standard API response format
     * The interceptor will automatically extract the result on success
     */
    delete<T>(baseUrl: string, endpoint: string, customHeaders?: any): Observable<T> {
        const options = customHeaders ? { headers: new HttpHeaders(customHeaders) } : this.httpOptions
        return this.http.delete<T>(`${baseUrl}/${endpoint}`, options)
    }

    /**
     * GET request that returns the full API response (including status, errors, etc.)
     * Use this when you need access to the full response structure
     */
    getFullResponse<T>(baseUrl: string, endpoint: string, customHeaders?: any): Observable<ApiResponse<T>> {
        const options = customHeaders ? { headers: new HttpHeaders(customHeaders) } : this.httpOptions

        // Add a special header to bypass the response transformation
        const fullResponseOptions = {
            ...options,
            headers: new HttpHeaders({
                ...options.headers,
                'X-Full-Response': 'true'
            })
        }

        return this.http.get<ApiResponse<T>>(`${baseUrl}/${endpoint}`, fullResponseOptions)
    }

    /**
     * POST request that returns the full API response (including status, errors, etc.)
     * Use this when you need access to the full response structure
     */
    postFullResponse<T>(baseUrl: string, endpoint: string, data: any, customHeaders?: any): Observable<ApiResponse<T>> {
        const options = customHeaders
            ? { headers: new HttpHeaders({ ...this.httpOptions.headers, ...customHeaders }) }
            : this.httpOptions

        // Add a special header to bypass the response transformation
        const fullResponseOptions = {
            ...options,
            headers: new HttpHeaders({
                ...options.headers,
                'X-Full-Response': 'true'
            })
        }

        return this.http.post<ApiResponse<T>>(`${baseUrl}/${endpoint}`, data, fullResponseOptions)
    }
}
