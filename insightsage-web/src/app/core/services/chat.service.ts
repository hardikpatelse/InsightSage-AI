import { Injectable } from '@angular/core'
import { Observable } from 'rxjs/internal/Observable'
import { DataProviderService } from './base/data-provider-service'
import { environment } from '../../../environments/environment'

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private messages: string[] = [];
  private baseUrl: string = environment.apiBaseUrl;

  constructor(private dataProviderService: DataProviderService) { }

  addMessage(message: string): void {
    this.messages.push(message)
  }

  getMessages(): string[] {
    return this.messages
  }

  clearMessages(): void {
    this.messages = []
  }

  sendPrompt(prompt: string): Observable<{ message: string }> {
    return this.dataProviderService.postData<{ message: string }>(this.baseUrl, 'api/chat/prompt', { message: prompt })
  }
}
