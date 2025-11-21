import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { MemeTemplate, CreateMemeTemplateRequest, UpdateMemeTemplateRequest, UploadSasTokenResponse } from '../models/meme.model';
import { API_BASE_URL } from '../constants/api.constants';

@Injectable({
    providedIn: 'root'
})
export class MemeService {
    private readonly apiUrl = `${API_BASE_URL}/memes/templates`;

    constructor(private http: HttpClient) { }

    /**
     * Get all meme templates
     */
    getTemplates(): Observable<MemeTemplate[]> {
        return this.http.get<MemeTemplate[]>(this.apiUrl);
    }

    /**
     * Get a single meme template by ID
     */
    getTemplateById(id: string): Observable<MemeTemplate> {
        return this.http.get<MemeTemplate>(`${this.apiUrl}/${id}`);
    }

    /**
     * Get a random meme template
     */
    getRandomTemplate(): Observable<MemeTemplate> {
        return this.http.get<MemeTemplate>(`${API_BASE_URL}/memes/random`);
    }

    /**
     * Create a new meme template
     */
    createTemplate(request: CreateMemeTemplateRequest): Observable<{ id: string }> {
        return this.http.post<{ id: string }>(this.apiUrl, request);
    }

    /**
     * Update an existing meme template
     */
    updateTemplate(id: string, request: UpdateMemeTemplateRequest): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}`, request);
    }

    /**
     * Delete a meme template
     */
    deleteTemplate(id: string): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }

    /**
     * Search templates by name (client-side filtering)
     */
    searchTemplates(searchTerm: string): Observable<MemeTemplate[]> {
        return this.getTemplates().pipe(
            map(templates =>
                templates.filter(template =>
                    template.title.toLowerCase().includes(searchTerm.toLowerCase())
                )
            )
        );
    }

    /**
     * Generate a SAS token for uploading images
     */
    generateUploadToken(): Observable<UploadSasTokenResponse> {
        return this.http.post<UploadSasTokenResponse>(`${API_BASE_URL}/memes/upload-token`, {});
    }

    /**
     * Upload an image to Azure Blob Storage using a SAS token
     */
    uploadImage(blobUrl: string, sasToken: string, file: File): Observable<void> {
        const uploadUrl = `${blobUrl}?${sasToken}`;

        const headers = new HttpHeaders({
            'x-ms-blob-type': 'BlockBlob',
            'Content-Type': file.type || 'image/png'
        });

        return this.http.put<void>(uploadUrl, file, { headers });
    }
}
