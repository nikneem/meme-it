import { 
  Component, 
  input, 
  output, 
  ElementRef, 
  viewChild,
  computed
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { ColorPickerModule } from 'primeng/colorpicker';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MemeTextArea } from '../../models/meme.models';

export interface DragData {
  startX: number;
  startY: number;
  elementX: number;
  elementY: number;
  elementWidth: number;
  elementHeight: number;
}

@Component({
  selector: 'app-draggable-text-area',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    InputTextModule,
    InputNumberModule,
    ColorPickerModule,
    ButtonModule,
    CardModule
  ],
  templateUrl: './draggable-text-area.component.html',
  styleUrl: './draggable-text-area.component.scss'
})
export class DraggableTextAreaComponent {
  readonly textArea = input.required<MemeTextArea>();
  readonly isSelected = input<boolean>(false);
  readonly containerBounds = input.required<DOMRect>();
  
  readonly onTextAreaChange = output<MemeTextArea>();
  readonly onDelete = output<void>();
  readonly onSelect = output<void>();
  
  readonly draggableElement = viewChild.required<ElementRef<HTMLDivElement>>('draggableElement');
  
  private isDragging = false;
  private isResizing = false;
  private resizeDirection = '';
  private dragData: DragData | null = null;
  
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
  
  readonly propertiesPanelPosition = computed(() => {
    const textArea = this.textArea();
    const bounds = this.containerBounds();
    
    // Position panel to the right of the text area, or left if not enough space
    let x = textArea.x + textArea.width + 10;
    if (x + 320 > bounds.width) {
      x = Math.max(10, textArea.x - 330);
    }
    
    let y = textArea.y;
    if (y + 400 > bounds.height) {
      y = Math.max(10, bounds.height - 410);
    }
    
    return { x, y };
  });

  constructor() {
    // No initialization needed
  }

  onMouseDown(event: MouseEvent): void {
    event.preventDefault();
    event.stopPropagation();
    
    this.onSelect.emit();
    
    if (event.target === this.draggableElement().nativeElement) {
      this.startDragging(event);
    }
  }

  onResizeStart(event: MouseEvent, direction: string): void {
    event.preventDefault();
    event.stopPropagation();
    
    this.isResizing = true;
    this.resizeDirection = direction;
    this.dragData = {
      startX: event.clientX,
      startY: event.clientY,
      elementX: this.textArea().x,
      elementY: this.textArea().y,
      elementWidth: this.textArea().width,
      elementHeight: this.textArea().height
    };
    
    this.addGlobalListeners();
  }

  private startDragging(event: MouseEvent): void {
    this.isDragging = true;
    this.dragData = {
      startX: event.clientX,
      startY: event.clientY,
      elementX: this.textArea().x,
      elementY: this.textArea().y,
      elementWidth: this.textArea().width,
      elementHeight: this.textArea().height
    };
    
    this.addGlobalListeners();
  }

  private addGlobalListeners(): void {
    document.addEventListener('mousemove', this.onMouseMove);
    document.addEventListener('mouseup', this.onMouseUp);
  }

  private removeGlobalListeners(): void {
    document.removeEventListener('mousemove', this.onMouseMove);
    document.removeEventListener('mouseup', this.onMouseUp);
  }

  private onMouseMove = (event: MouseEvent): void => {
    if (!this.dragData) return;
    
    if (this.isDragging) {
      this.handleDrag(event);
    } else if (this.isResizing) {
      this.handleResize(event);
    }
  };

  private onMouseUp = (): void => {
    this.isDragging = false;
    this.isResizing = false;
    this.dragData = null;
    this.removeGlobalListeners();
  };

  private handleDrag(event: MouseEvent): void {
    if (!this.dragData) return;
    
    const deltaX = event.clientX - this.dragData.startX;
    const deltaY = event.clientY - this.dragData.startY;
    
    const newX = Math.max(0, Math.min(
      this.containerBounds().width - this.textArea().width,
      this.dragData.elementX + deltaX
    ));
    
    const newY = Math.max(0, Math.min(
      this.containerBounds().height - this.textArea().height,
      this.dragData.elementY + deltaY
    ));
    
    this.updateProperties({ x: newX, y: newY });
  }

  private handleResize(event: MouseEvent): void {
    if (!this.dragData) return;
    
    const deltaX = event.clientX - this.dragData.startX;
    const deltaY = event.clientY - this.dragData.startY;
    
    // Apply scaling factor to make resize less sensitive
    const resizeScale = 0.5;
    const scaledDeltaX = deltaX * resizeScale;
    const scaledDeltaY = deltaY * resizeScale;
    
    // Use original dimensions from dragData to avoid exponential growth
    let newX = this.dragData.elementX;
    let newY = this.dragData.elementY;
    let newWidth = this.dragData.elementWidth;
    let newHeight = this.dragData.elementHeight;
    
    switch (this.resizeDirection) {
      case 'nw':
        newX = Math.max(0, this.dragData.elementX + scaledDeltaX);
        newY = Math.max(0, this.dragData.elementY + scaledDeltaY);
        newWidth = Math.max(50, this.dragData.elementWidth - scaledDeltaX);
        newHeight = Math.max(20, this.dragData.elementHeight - scaledDeltaY);
        break;
      case 'ne':
        newY = Math.max(0, this.dragData.elementY + scaledDeltaY);
        newWidth = Math.max(50, this.dragData.elementWidth + scaledDeltaX);
        newHeight = Math.max(20, this.dragData.elementHeight - scaledDeltaY);
        break;
      case 'sw':
        newX = Math.max(0, this.dragData.elementX + scaledDeltaX);
        newWidth = Math.max(50, this.dragData.elementWidth - scaledDeltaX);
        newHeight = Math.max(20, this.dragData.elementHeight + scaledDeltaY);
        break;
      case 'se':
        newWidth = Math.max(50, this.dragData.elementWidth + scaledDeltaX);
        newHeight = Math.max(20, this.dragData.elementHeight + scaledDeltaY);
        break;
    }
    
    // Ensure the text area stays within bounds
    if (newX + newWidth > this.containerBounds().width) {
      newWidth = this.containerBounds().width - newX;
    }
    if (newY + newHeight > this.containerBounds().height) {
      newHeight = this.containerBounds().height - newY;
    }
    
    this.updateProperties({ x: newX, y: newY, width: newWidth, height: newHeight });
  }

  updateProperty(property: keyof MemeTextArea, value: any): void {
    const updatedTextArea = { ...this.textArea() };
    (updatedTextArea as any)[property] = value;
    this.onTextAreaChange.emit(updatedTextArea);
  }

  updateProperties(properties: Partial<MemeTextArea>): void {
    const updatedTextArea = { ...this.textArea(), ...properties };
    this.onTextAreaChange.emit(updatedTextArea);
  }
}
