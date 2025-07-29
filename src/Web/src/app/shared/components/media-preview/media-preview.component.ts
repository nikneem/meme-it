import { 
  Component, 
  input, 
  output, 
  signal, 
  ElementRef, 
  viewChild,
  computed,
  effect
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { DraggableTextAreaComponent } from '../draggable-text-area/draggable-text-area.component';
import { MemeTextArea, UploadedFile } from '../../models/meme.models';

@Component({
  selector: 'app-media-preview',
  standalone: true,
  imports: [
    CommonModule,
    DraggableTextAreaComponent
  ],
  templateUrl: './media-preview.component.html',
  styleUrl: './media-preview.component.scss'
})
export class MediaPreviewComponent {
  readonly uploadedFile = input<UploadedFile | null>(null);
  readonly textAreas = input<MemeTextArea[]>([]);
  readonly selectedTextAreaId = input<string | null>(null);
  
  readonly onTextAreasChange = output<MemeTextArea[]>();
  readonly onTextAreaSelect = output<string | null>();
  
  readonly container = viewChild.required<ElementRef<HTMLDivElement>>('container');
  readonly mediaElement = viewChild<ElementRef<HTMLImageElement | HTMLVideoElement>>('mediaElement');
  
  readonly containerBounds = signal<DOMRect>(new DOMRect(0, 0, 600, 400));
  
  readonly containerSize = computed(() => {
    const file = this.uploadedFile();
    if (!file?.dimensions) {
      return { width: 600, height: 400 };
    }
    
    const { width, height } = file.dimensions;
    const maxWidth = 800;
    const maxHeight = 600;
    
    // Calculate aspect ratio and scale to fit
    const aspectRatio = width / height;
    let displayWidth = width;
    let displayHeight = height;
    
    if (displayWidth > maxWidth) {
      displayWidth = maxWidth;
      displayHeight = displayWidth / aspectRatio;
    }
    
    if (displayHeight > maxHeight) {
      displayHeight = maxHeight;
      displayWidth = displayHeight * aspectRatio;
    }
    
    return {
      width: Math.round(displayWidth),
      height: Math.round(displayHeight)
    };
  });

  constructor() {
    // Update container bounds when size changes
    effect(() => {
      const size = this.containerSize();
      this.containerBounds.set(new DOMRect(0, 0, size.width, size.height));
    });
  }

  onMediaLoad(): void {
    // Update container bounds after media loads
    setTimeout(() => {
      const element = this.container().nativeElement;
      this.containerBounds.set(element.getBoundingClientRect());
    }, 0);
  }

  onContainerClick(event: MouseEvent): void {
    if (!this.uploadedFile()) return;
    
    // Only add text area if clicking on empty space
    const target = event.target as HTMLElement;
    if (target === this.container().nativeElement || 
        target.classList.contains('media-element') ||
        target.classList.contains('add-text-button')) {
      this.addTextArea(event);
    } else {
      // Deselect current text area
      this.onTextAreaSelect.emit(null);
    }
  }

  addTextArea(event: MouseEvent): void {
    event.stopPropagation();
    
    const rect = this.container().nativeElement.getBoundingClientRect();
    const x = Math.max(0, Math.min(rect.width - 150, event.clientX - rect.left - 75));
    const y = Math.max(0, Math.min(rect.height - 40, event.clientY - rect.top - 20));
    
    const newTextArea: MemeTextArea = {
      id: `text-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
      x: Math.round(x),
      y: Math.round(y),
      width: 150,
      height: 40,
      fontSize: 24,
      fontFamily: 'Arial, sans-serif',
      fontColor: '#FFFFFF',
      maxLength: 100
    };
    
    const updatedTextAreas = [...this.textAreas(), newTextArea];
    this.onTextAreasChange.emit(updatedTextAreas);
    this.onTextAreaSelect.emit(newTextArea.id);
  }

  onTextAreaChange(updatedTextArea: MemeTextArea): void {
    const updatedTextAreas = this.textAreas().map(ta => 
      ta.id === updatedTextArea.id ? updatedTextArea : ta
    );
    this.onTextAreasChange.emit(updatedTextAreas);
  }

  onDeleteTextArea(textAreaId: string): void {
    const updatedTextAreas = this.textAreas().filter(ta => ta.id !== textAreaId);
    this.onTextAreasChange.emit(updatedTextAreas);
    
    if (this.selectedTextAreaId() === textAreaId) {
      this.onTextAreaSelect.emit(null);
    }
  }

  onSelectTextArea(textAreaId: string): void {
    this.onTextAreaSelect.emit(textAreaId);
  }

  formatFileSize(bytes?: number): string {
    if (!bytes) return '0 B';
    
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return `${(bytes / Math.pow(1024, i)).toFixed(1)} ${sizes[i]}`;
  }
}
