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
import { FormsModule } from '@angular/forms';
import { InputNumberModule } from 'primeng/inputnumber';
import { ColorPickerModule } from 'primeng/colorpicker';
import { CardModule } from 'primeng/card';
import { DraggableTextAreaComponent } from '../draggable-text-area/draggable-text-area.component';
import { MemeTextArea, UploadedFile } from '../../models/meme.models';

@Component({
  selector: 'app-media-preview',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    InputNumberModule,
    ColorPickerModule,
    CardModule,
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
  
  readonly fontOptions = [
    { label: 'Arial', value: 'Arial, sans-serif' },
    { label: 'Helvetica', value: 'Helvetica, sans-serif' },
    { label: 'Times New Roman', value: 'Times New Roman, serif' },
    { label: 'Georgia', value: 'Georgia, serif' },
    { label: 'Verdana', value: 'Verdana, sans-serif' },
    { label: 'Courier New', value: 'Courier New, monospace' },
    { label: 'Impact', value: 'Impact, sans-serif' },
    { label: 'Comic Sans MS', value: 'Comic Sans MS, cursive' }
  ];
  
  readonly selectedTextArea = computed(() => {
    const selectedId = this.selectedTextAreaId();
    return selectedId ? this.textAreas().find(ta => ta.id === selectedId) : null;
  });
  
  readonly propertiesPanelPosition = computed(() => {
    const textArea = this.selectedTextArea();
    const bounds = this.containerBounds();
    
    if (!textArea) return { x: 0, y: 0 };
    
    // Panel dimensions
    const panelWidth = 320;
    const panelHeight = 400;
    
    // Get container element to calculate its position
    const containerElement = this.container()?.nativeElement;
    const containerRect = containerElement?.getBoundingClientRect();
    
    // Calculate absolute position relative to the viewport
    const containerOffsetX = containerRect?.left || 0;
    const containerOffsetY = containerRect?.top || 0;
    
    // Position panel to the right of the text area by default
    let x = containerOffsetX + textArea.x + textArea.width + 10;
    
    // If panel would extend beyond the right edge of the viewport,
    // position it to the left of the text area instead
    if (x + panelWidth > window.innerWidth) {
      x = containerOffsetX + textArea.x - panelWidth - 10;
    }
    
    // If positioning to the left would place it outside the left edge,
    // position it at the right edge of the container
    if (x < 0) {
      x = containerOffsetX + bounds.width + 10;
    }
    
    // Position panel vertically aligned with text area top
    let y = containerOffsetY + textArea.y;
    
    // Adjust if panel would extend beyond viewport bottom
    if (y + panelHeight > window.innerHeight) {
      y = Math.max(0, window.innerHeight - panelHeight - 10);
    }
    
    // Ensure panel doesn't go above viewport top
    if (y < 0) {
      y = 10;
    }
    
    return { x, y };
  });
  
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
    // Update container bounds when computed size changes
    effect(() => {
      const size = this.containerSize();
      this.containerBounds.set(new DOMRect(0, 0, size.width, size.height));
    });
  }

  onMediaLoad(): void {
    // Media loaded - bounds are already updated by the effect
  }

  onContainerClick(event: MouseEvent): void {
    if (!this.uploadedFile()) return;
    
    const target = event.target as HTMLElement;
    
    // Check if the click is on a draggable text area or its children
    const isTextAreaClick = target.closest('app-draggable-text-area') !== null;
    
    // Check if the click is on the properties panel
    const isPropertiesPanelClick = target.closest('.properties-panel') !== null;
    
    // Check if the click is on the container background or media element
    const isBackgroundClick = target === this.container().nativeElement || 
                             target.classList.contains('media-element') ||
                             target.classList.contains('placeholder-area') ||
                             target.classList.contains('placeholder-content');
    
    if (isBackgroundClick && !isTextAreaClick && !isPropertiesPanelClick) {
      // Only add text area if clicking on empty background space
      this.addTextArea(event);
    } else if (!isTextAreaClick && !isPropertiesPanelClick && !target.classList.contains('add-text-button')) {
      // Only deselect if clicking outside text areas, properties panel, and not on the add button
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

  updateTextAreaProperty(property: keyof MemeTextArea, value: any): void {
    const selectedTextArea = this.selectedTextArea();
    if (!selectedTextArea) return;
    
    const updatedTextArea = { ...selectedTextArea, [property]: value };
    this.onTextAreaChange(updatedTextArea);
  }

  formatFileSize(bytes?: number): string {
    if (!bytes) return '0 B';
    
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return `${(bytes / Math.pow(1024, i)).toFixed(1)} ${sizes[i]}`;
  }
}
