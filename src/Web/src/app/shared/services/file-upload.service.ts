import { Injectable } from '@angular/core';

export interface FileValidationResult {
  isValid: boolean;
  error?: string;
}

@Injectable({
  providedIn: 'root'
})
export class FileUploadService {
  private readonly allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp', 'video/mp4'];
  private readonly maxFileSize = 50 * 1024 * 1024; // 50MB

  validateFile(file: File): FileValidationResult {
    // Check file type
    if (!this.allowedTypes.includes(file.type)) {
      return {
        isValid: false,
        error: 'Only JPG, PNG, WEBP, and MP4 files are allowed.'
      };
    }

    // Check file size
    if (file.size > this.maxFileSize) {
      return {
        isValid: false,
        error: 'File size must be less than 50MB.'
      };
    }

    return { isValid: true };
  }

  getMediaDimensions(file: File): Promise<{ width: number; height: number }> {
    return new Promise((resolve, reject) => {
      if (file.type.startsWith('image/')) {
        const img = new Image();
        img.onload = () => {
          resolve({ width: img.naturalWidth, height: img.naturalHeight });
        };
        img.onerror = reject;
        img.src = URL.createObjectURL(file);
      } else if (file.type.startsWith('video/')) {
        const video = document.createElement('video');
        video.onloadedmetadata = () => {
          resolve({ width: video.videoWidth, height: video.videoHeight });
        };
        video.onerror = reject;
        video.src = URL.createObjectURL(file);
      } else {
        reject(new Error('Unsupported file type'));
      }
    });
  }

  createPreviewUrl(file: File): string {
    return URL.createObjectURL(file);
  }

  revokePreviewUrl(url: string): void {
    URL.revokeObjectURL(url);
  }
}
