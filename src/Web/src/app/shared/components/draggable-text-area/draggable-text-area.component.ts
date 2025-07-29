import { 
  Component, 
  input, 
  output, 
  ElementRef, 
  viewChild,
  computed
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
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
    ButtonModule
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

  private onMouseUp = (event: MouseEvent): void => {
    // Prevent the mouse up event from bubbling to parent elements
    event.stopPropagation();
    
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
