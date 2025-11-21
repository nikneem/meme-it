import { Component, OnInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSliderModule } from '@angular/material/slider';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { Subject, takeUntil } from 'rxjs';
import { MemeService } from '@services/meme.service';
import { NotificationService } from '@services/notification.service';
import { TextAreaDefinition } from '@models/meme.model';

interface TextAreaEditable extends TextAreaDefinition {
    id: string;
    isSelected: boolean;
}

@Component({
    selector: 'memeit-create-meme',
    imports: [
        CommonModule,
        RouterLink,
        ReactiveFormsModule,
        FormsModule,
        MatButtonModule,
        MatCardModule,
        MatIconModule,
        MatInputModule,
        MatFormFieldModule,
        MatProgressSpinnerModule,
        MatSliderModule,
        MatSlideToggleModule
    ],
    templateUrl: './create-meme.html',
    styleUrl: './create-meme.scss',
})
export class CreateMemePage implements OnInit, OnDestroy {
    @ViewChild('imageCanvas', { static: false }) imageCanvas?: ElementRef<HTMLDivElement>;

    isUploading = false;
    isSaving = false;
    uploadedImageUrl: string | null = null;
    imageNaturalWidth = 0;
    imageNaturalHeight = 0;

    textAreas: TextAreaEditable[] = [];
    selectedTextArea: TextAreaEditable | null = null;

    isDragging = false;
    isResizing = false;
    dragStartX = 0;
    dragStartY = 0;
    resizeStartX = 0;
    resizeStartY = 0;

    templateForm = new FormGroup({
        title: new FormControl('', [Validators.required, Validators.minLength(3)])
    });

    private destroy$ = new Subject<void>();

    constructor(
        private memeService: MemeService,
        private notificationService: NotificationService,
        private router: Router
    ) { }

    ngOnInit(): void {
        // Setup event listeners for drag and resize
        document.addEventListener('mousemove', this.onMouseMove.bind(this));
        document.addEventListener('mouseup', this.onMouseUp.bind(this));
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
        document.removeEventListener('mousemove', this.onMouseMove.bind(this));
        document.removeEventListener('mouseup', this.onMouseUp.bind(this));
    }

    onFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        if (input.files && input.files[0]) {
            this.uploadImage(input.files[0]);
        }
    }

    onFileDrop(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();

        if (event.dataTransfer?.files && event.dataTransfer.files[0]) {
            this.uploadImage(event.dataTransfer.files[0]);
        }
    }

    onDragOver(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
    }

    uploadImage(file: File): void {
        if (!file.type.startsWith('image/')) {
            this.notificationService.error('Invalid File', 'Please upload an image file.', undefined, 3000);
            return;
        }

        this.isUploading = true;

        // First, get the SAS token
        this.memeService.generateUploadToken()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (tokenResponse) => {
                    // Upload the image to blob storage
                    this.memeService.uploadImage(tokenResponse.blobUrl, tokenResponse.sasToken, file)
                        .pipe(takeUntil(this.destroy$))
                        .subscribe({
                            next: () => {
                                this.uploadedImageUrl = tokenResponse.blobUrl;
                                this.isUploading = false;
                                this.notificationService.success('Uploaded', 'Image uploaded successfully', undefined, 2000);

                                // Load image to get dimensions
                                const img = new Image();
                                img.onload = () => {
                                    this.imageNaturalWidth = img.naturalWidth;
                                    this.imageNaturalHeight = img.naturalHeight;
                                };
                                img.src = this.uploadedImageUrl;
                            },
                            error: (error) => {
                                console.error('Upload failed:', error);
                                this.notificationService.error('Upload Failed', 'Failed to upload image. Please try again.', undefined, 5000);
                                this.isUploading = false;
                            }
                        });
                },
                error: (error) => {
                    console.error('Failed to get upload token:', error);
                    this.notificationService.error('Upload Failed', 'Failed to get upload token. Please try again.', undefined, 5000);
                    this.isUploading = false;
                }
            });
    }

    onCanvasClick(event: MouseEvent): void {
        if (!this.imageCanvas) return;

        const canvas = this.imageCanvas.nativeElement;
        const rect = canvas.getBoundingClientRect();
        const x = event.clientX - rect.left;
        const y = event.clientY - rect.top;

        // Check if clicking on an existing text area
        const clickedTextArea = this.textAreas.find(ta =>
            x >= ta.x && x <= ta.x + ta.width &&
            y >= ta.y && y <= ta.y + ta.height
        );

        if (clickedTextArea) {
            this.selectTextArea(clickedTextArea);
        } else {
            // Create new text area
            this.createTextArea(x, y);
        }
    }

    createTextArea(x: number, y: number): void {
        const newTextArea: TextAreaEditable = {
            id: `textarea-${Date.now()}`,
            x: Math.round(x),
            y: Math.round(y),
            width: 200,
            height: 60,
            fontSize: 24,
            fontColor: '#FFFFFF',
            borderSize: 2,
            borderColor: '#000000',
            isBold: true,
            isSelected: true
        };

        // Deselect all others
        this.textAreas.forEach(ta => ta.isSelected = false);

        this.textAreas.push(newTextArea);
        this.selectedTextArea = newTextArea;
    }

    selectTextArea(textArea: TextAreaEditable): void {
        this.textAreas.forEach(ta => ta.isSelected = false);
        textArea.isSelected = true;
        this.selectedTextArea = textArea;
    }

    deselectAll(event?: MouseEvent): void {
        if (event) {
            event.stopPropagation();
        }
        this.textAreas.forEach(ta => ta.isSelected = false);
        this.selectedTextArea = null;
    }

    onTextAreaMouseDown(event: MouseEvent, textArea: TextAreaEditable): void {
        event.stopPropagation();
        this.selectTextArea(textArea);

        this.isDragging = true;
        this.dragStartX = event.clientX - textArea.x;
        this.dragStartY = event.clientY - textArea.y;
    }

    onResizeHandleMouseDown(event: MouseEvent, textArea: TextAreaEditable): void {
        event.stopPropagation();
        this.selectTextArea(textArea);

        this.isResizing = true;
        this.resizeStartX = event.clientX;
        this.resizeStartY = event.clientY;
    }

    onMouseMove(event: MouseEvent): void {
        if (!this.selectedTextArea) return;

        if (this.isDragging && this.imageCanvas) {
            const canvas = this.imageCanvas.nativeElement;
            const rect = canvas.getBoundingClientRect();

            let newX = event.clientX - rect.left - this.dragStartX;
            let newY = event.clientY - rect.top - this.dragStartY;

            // Constrain to canvas bounds
            newX = Math.max(0, Math.min(newX, rect.width - this.selectedTextArea.width));
            newY = Math.max(0, Math.min(newY, rect.height - this.selectedTextArea.height));

            this.selectedTextArea.x = Math.round(newX);
            this.selectedTextArea.y = Math.round(newY);
        } else if (this.isResizing) {
            const deltaX = event.clientX - this.resizeStartX;
            const deltaY = event.clientY - this.resizeStartY;

            let newWidth = this.selectedTextArea.width + deltaX;
            let newHeight = this.selectedTextArea.height + deltaY;

            // Minimum size
            newWidth = Math.max(50, newWidth);
            newHeight = Math.max(30, newHeight);

            this.selectedTextArea.width = Math.round(newWidth);
            this.selectedTextArea.height = Math.round(newHeight);

            this.resizeStartX = event.clientX;
            this.resizeStartY = event.clientY;
        }
    }

    onMouseUp(): void {
        this.isDragging = false;
        this.isResizing = false;
    }

    deleteSelectedTextArea(): void {
        if (!this.selectedTextArea) return;

        this.textAreas = this.textAreas.filter(ta => ta.id !== this.selectedTextArea!.id);
        this.selectedTextArea = null;
    }

    formatLabel(value: number): string {
        return `${value}`;
    }

    onSubmit(): void {
        if (!this.templateForm.valid || !this.uploadedImageUrl) {
            this.notificationService.error('Validation Error', 'Please fill in all required fields and upload an image.', undefined, 3000);
            return;
        }

        if (this.textAreas.length === 0) {
            this.notificationService.error('Validation Error', 'Please add at least one text area.', undefined, 3000);
            return;
        }

        this.isSaving = true;

        // Convert to scale-independent coordinates (based on natural image size)
        const scaleX = this.imageNaturalWidth / (this.imageCanvas?.nativeElement.offsetWidth || 1);
        const scaleY = this.imageNaturalHeight / (this.imageCanvas?.nativeElement.offsetHeight || 1);

        const textAreas: TextAreaDefinition[] = this.textAreas.map(ta => ({
            x: Math.round(ta.x * scaleX),
            y: Math.round(ta.y * scaleY),
            width: Math.round(ta.width * scaleX),
            height: Math.round(ta.height * scaleY),
            fontSize: ta.fontSize,
            fontColor: ta.fontColor,
            borderSize: ta.borderSize,
            borderColor: ta.borderColor,
            isBold: ta.isBold
        }));

        const request = {
            title: this.templateForm.value.title!,
            imageUrl: this.uploadedImageUrl,
            textAreas
        };

        this.memeService.createTemplate(request)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: () => {
                    this.notificationService.success('Created', 'Meme template created successfully', undefined, 3000);
                    this.router.navigate(['/management/memes']);
                },
                error: (error) => {
                    console.error('Failed to create template:', error);
                    this.notificationService.error('Save Failed', 'Failed to create template. Please try again.', undefined, 5000);
                    this.isSaving = false;
                }
            });
    }
}
