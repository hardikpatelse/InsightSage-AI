import { HttpClient, HttpHeaders } from '@angular/common/http'
import { Injectable } from '@angular/core'
import { Observable } from 'rxjs/internal/Observable'

@Injectable({
  providedIn: 'root'
})
export class DataProviderService {
  private httpOptions = {
    headers: new HttpHeaders(
      {
        'Content-Type': 'application/json',
      }
    )
  };

  constructor(private http: HttpClient) { }

  getData<T>(baseUrl: string, apiEndPoint: string, customHeaders?: any): Observable<T> {
    const options = customHeaders ? { headers: new HttpHeaders(customHeaders) } : this.httpOptions
    return this.http.get<T>(`${baseUrl}/${apiEndPoint}`, options)
  }

  postData<T>(baseUrl: string, apiEndPoint: string, data: any, customHeaders?: any): Observable<T> {
    const options = customHeaders
      ? { headers: new HttpHeaders({ ...this.httpOptions.headers, ...customHeaders }) }
      : this.httpOptions
    return this.http.post<T>(`${baseUrl}/${apiEndPoint}`, data, options)
  }

  patchData<T>(baseUrl: string, apiEndPoint: string, data: any, customHeaders?: any): Observable<T> {
    const options = customHeaders
      ? { headers: new HttpHeaders({ ...this.httpOptions.headers, ...customHeaders }) }
      : this.httpOptions
    return this.http.patch<T>(`${baseUrl}/${apiEndPoint}`, data, options)
  }

  deleteData<T>(baseUrl: string, apiEndPoint: string, customHeaders?: any): Observable<T> {
    const options = customHeaders ? { headers: new HttpHeaders(customHeaders) } : this.httpOptions
    return this.http.delete<T>(`${baseUrl}/${apiEndPoint}`, options)
  }
}
