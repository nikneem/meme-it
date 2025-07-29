import { Component, output, signal, ElementRef, viewChild, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { ProgressBarModule } from 'primeng/progressbar';
import { FileUploadService } from '../../services/file-upload.service';

export interface FileUploadResult {
  file: File;
  previewUrl: string;
  dimensions: { width: number; height: number };
}

@Component({
  selector: 'app-file-drop-zone',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    CardModule,
    MessageModule,
    ProgressBarModule
  ],
  templateUrl: './file-drop-zone.component.html',
  styleUrl: './file-drop-zone.component.scss'
})
export class FileDropZoneComponent {
  readonly maxSizeBytes = input<number>(50 * 1024 * 1024); // 50MB default
  readonly acceptedTypes = input<string[]>(['image/jpeg', 'image/png', 'image/webp', 'video/mp4']);
  
  private readonly fileUploadService = new FileUploadService();
  
  readonly onFileSelected = output<File>();
  readonly onValidationError = output<string>();
  
  readonly isDragging = signal(false);
  readonly isUploading = signal(false);
  readonly uploadProgress = signal(0);
  readonly errorMessage = signal<string | null>(null);
  
  readonly fileInput = viewChild.required<ElementRef<HTMLInputElement>>('fileInput');

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    if (!this.isUploading()) {
      this.isDragging.set(true);
    }
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(false);
    
    if (this.isUploading()) return;
    
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.processFile(files[0]);
    }
  }

  openFileSelector(): void {
    if (!this.isUploading()) {
      this.fileInput().nativeElement.click();
    }
  }

  onFileInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.processFile(input.files[0]);
    }
  }

  private processFile(file: File): void {
    this.errorMessage.set(null);
    
    // Validate file type
    if (!this.acceptedTypes().includes(file.type)) {
      const error = `Only ${this.acceptedTypes().join(', ')} files are allowed.`;
      this.errorMessage.set(error);
      this.onValidationError.emit(error);
      return;
    }
    
    // Validate file size
    if (file.size > this.maxSizeBytes()) {
      const error = `File size must be less than ${Math.round(this.maxSizeBytes() / (1024 * 1024))}MB.`;
      this.errorMessage.set(error);
      this.onValidationError.emit(error);
      return;
    }

    // File is valid, emit it
    this.onFileSelected.emit(file);
  }
}
