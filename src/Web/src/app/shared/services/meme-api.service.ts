import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  CreateMemeRequest, 
  CreateMemeResponse, 
  GenerateUploadSasRequest, 
  GenerateUploadSasResponse,
  MemeTemplate 
} from '../models/meme.models';

@Injectable({
  providedIn: 'root'
})
export class MemeApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/management/memes'; // This will be proxied in development

  generateUploadSas(request: GenerateUploadSasRequest): Observable<GenerateUploadSasResponse> {
    return this.http.post<GenerateUploadSasResponse>(`${this.baseUrl}/upload`, request);
  }

  uploadFile(sasUri: string, file: File): Observable<any> {
    const headers = {
      'x-ms-blob-type': 'BlockBlob',
      'Content-Type': file.type
    };
    
    return this.http.put(sasUri, file, { headers });
  }

  createMeme(request: CreateMemeRequest): Observable<CreateMemeResponse> {
    return this.http.post<CreateMemeResponse>(this.baseUrl, request);
  }

  getMeme(id: string): Observable<MemeTemplate> {
    return this.http.get<MemeTemplate>(`${this.baseUrl}/${id}`);
  }

  getMemes(): Observable<MemeTemplate[]> {
    return this.http.get<MemeTemplate[]>(this.baseUrl);
  }

  updateMeme(id: string, request: CreateMemeRequest): Observable<MemeTemplate> {
    return this.http.put<MemeTemplate>(`${this.baseUrl}/${id}`, request);
  }

  deleteMeme(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
