import { Component, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';

import { FileUploadService } from '../../../shared/services/file-upload.service';
import { 
  MemeTextArea, 
  UploadedFile, 
  CreateMemeRequest, 
  GenerateUploadSasResponse,
  MemeTextAreaDto
} from '../../../shared/models/meme.models';
import { MemeApiService } from '../../../shared/services/meme-api.service';
import { FileDropZoneComponent } from '../../../shared/components/file-drop-zone/file-drop-zone.component';
import { MediaPreviewComponent } from '../../../shared/components/media-preview/media-preview.component';

@Component({
  selector: 'app-create-meme-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    InputTextModule,
    CardModule,
    ProgressSpinnerModule,
    ToastModule,
    FileDropZoneComponent,
    MediaPreviewComponent
  ],
  providers: [MessageService],
  templateUrl: './create-meme-page.component.html',
  styleUrl: './create-meme-page.component.scss'
})
export class CreateMemePageComponent {
  readonly uploadedFile = signal<UploadedFile | null>(null);
  readonly textAreas = signal<MemeTextArea[]>([]);
  readonly selectedTextAreaId = signal<string | null>(null);
  readonly isCreating = signal<boolean>(false);
  
  memeName = '';
  memeDescription = '';
  
  readonly maxFileSize = 50 * 1024 * 1024; // 50MB
  readonly acceptedTypes = ['image/jpeg', 'image/png', 'image/webp', 'video/mp4'];
  
  readonly canCreateMeme = computed(() => {
    return this.memeName.trim().length > 0 && 
           this.uploadedFile() !== null && 
           !this.isCreating();
  });

  constructor(
    private memeApiService: MemeApiService,
    private fileUploadService: FileUploadService,
    private messageService: MessageService,
    private router: Router
  ) {}

  async onFileSelected(file: File): Promise<void> {
    try {
      // Validate file
      const validation = this.fileUploadService.validateFile(file);
      if (!validation.isValid) {
        throw new Error(validation.error);
      }

      // Get dimensions and create preview
      const dimensions = await this.fileUploadService.getMediaDimensions(file);
      const url = this.fileUploadService.createPreviewUrl(file);

      const uploadedFile: UploadedFile = {
        file,
        url,
        type: file.type,
        dimensions
      };

      this.uploadedFile.set(uploadedFile);
      this.textAreas.set([]);
      this.selectedTextAreaId.set(null);
      
      this.messageService.add({
        severity: 'success',
        summary: 'File Ready',
        detail: 'Media file loaded successfully'
      });
    } catch (error) {
      this.messageService.add({
        severity: 'error',
        summary: 'Upload Error',
        detail: error instanceof Error ? error.message : 'Failed to process file'
      });
    }
  }

  onValidationError(error: string): void {
    this.messageService.add({
      severity: 'error',
      summary: 'Validation Error',
      detail: error
    });
  }

  onTextAreasChange(textAreas: MemeTextArea[]): void {
    this.textAreas.set(textAreas);
  }

  onTextAreaSelect(textAreaId: string | null): void {
    this.selectedTextAreaId.set(textAreaId);
  }

  clearMedia(): void {
    this.uploadedFile.set(null);
    this.textAreas.set([]);
    this.selectedTextAreaId.set(null);
    
    this.messageService.add({
      severity: 'info',
      summary: 'Media Cleared',
      detail: 'Upload a new media file to continue'
    });
  }

  clearAllText(): void {
    this.textAreas.set([]);
    this.selectedTextAreaId.set(null);
    
    this.messageService.add({
      severity: 'info',
      summary: 'Text Cleared',
      detail: 'All text areas have been removed'
    });
  }

  async createMeme(): Promise<void> {
    const file = this.uploadedFile();
    if (!file || !this.canCreateMeme()) {
      return;
    }

    this.isCreating.set(true);

    try {
      // Step 1: Get SAS token for upload
      const sasResponse = await this.memeApiService.generateUploadSas({
        fileName: file.file.name,
        contentType: file.file.type
      }).toPromise();

      if (!sasResponse) {
        throw new Error('Failed to get upload token');
      }

      // Step 2: Upload file to blob storage
      await this.uploadFileToBlob(file.file, sasResponse);

      // Step 3: Create meme with uploaded file reference
      const createRequest: CreateMemeRequest = {
        name: this.memeName.trim(),
        description: this.memeDescription.trim() || undefined,
        sourceImage: sasResponse.fileName,
        sourceWidth: file.dimensions?.width || 0,
        sourceHeight: file.dimensions?.height || 0,
        textareas: this.textAreas().map(ta => ({
          x: ta.x,
          y: ta.y,
          width: ta.width,
          height: ta.height,
          fontFamily: ta.fontFamily,
          fontSize: ta.fontSize,
          fontColor: ta.fontColor,
          maxLength: ta.maxLength
        } as MemeTextAreaDto))
      };

      const response = await this.memeApiService.createMeme(createRequest).toPromise();

      this.messageService.add({
        severity: 'success',
        summary: 'Meme Created',
        detail: `Meme "${this.memeName}" created successfully!`
      });

      // Navigate to meme details or list
      setTimeout(() => {
        this.router.navigate(['/management/memes']);
      }, 1500);

    } catch (error) {
      this.messageService.add({
        severity: 'error',
        summary: 'Creation Failed',
        detail: error instanceof Error ? error.message : 'Failed to create meme'
      });
    } finally {
      this.isCreating.set(false);
    }
  }

  private async uploadFileToBlob(file: File, sasResponse: GenerateUploadSasResponse): Promise<void> {
    const response = await fetch(sasResponse.sasUri, {
      method: 'PUT',
      headers: {
        'x-ms-blob-type': 'BlockBlob',
        'Content-Type': file.type
      },
      body: file
    });

    if (!response.ok) {
      throw new Error('Failed to upload file to storage');
    }
  }

  cancel(): void {
    this.router.navigate(['/management/memes']);
  }

  formatFileSize(bytes?: number): string {
    if (!bytes) return '0 B';
    
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return `${(bytes / Math.pow(1024, i)).toFixed(1)} ${sizes[i]}`;
  }
}
