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
  constructor(
    private http: HttpClient
  ) {

  }

  getData<T>(baseUrl: string, apiEndPoint: string): Observable<T> {
    return this.http.get<T>(`${baseUrl}/${apiEndPoint}`, this.httpOptions)
  }

  postData<T>(baseUrl: string, apiEndPoint: string, data: any): Observable<T> {
    return this.http.post<T>(`${baseUrl}/${apiEndPoint}`, data, this.httpOptions)
  }

  patchData<T>(baseUrl: string, apiEndPoint: string, data: any): Observable<T> {
    return this.http.patch<T>(`${baseUrl}/${apiEndPoint}`, data, this.httpOptions)
  }

  deleteData<T>(baseUrl: string, apiEndPoint: string): Observable<T> {
    return this.http.delete<T>(`${baseUrl}/${apiEndPoint}`, this.httpOptions)
  }
}
