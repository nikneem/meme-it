import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService, MessageService } from 'primeng/api';
import { MemeApiService } from '../../../shared/services/meme-api.service';
import { MemeTemplateListItem } from '../../../shared/models/meme.models';

@Component({
  selector: 'app-meme-list-page',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    TableModule,
    ConfirmDialogModule,
    ToastModule,
    TooltipModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './meme-list-page.component.html',
  styleUrl: './meme-list-page.component.scss'
})
export class MemeListPageComponent implements OnInit {
  private readonly memeApiService = inject(MemeApiService);
  private readonly router = inject(Router);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly messageService = inject(MessageService);

  memeTemplates = signal<MemeTemplateListItem[]>([]);
  isLoading = signal(false);

  async ngOnInit(): Promise<void> {
    await this.loadMemeTemplates();
  }

  async loadMemeTemplates(): Promise<void> {
    this.isLoading.set(true);
    try {
      const templates = await this.memeApiService.getMemes().toPromise();
      this.memeTemplates.set(templates || []);
    } catch (error) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to load meme templates'
      });
      console.error('Failed to load meme templates:', error);
    } finally {
      this.isLoading.set(false);
    }
  }

  createNewMeme(): void {
    this.router.navigate(['/management/memes/create']);
  }

  confirmDelete(meme: MemeTemplateListItem): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete "${meme.name}"? This action cannot be undone.`,
      header: 'Delete Confirmation',
      icon: 'pi pi-exclamation-triangle',
      rejectButtonStyleClass: 'p-button-text',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.deleteMeme(meme.id);
      }
    });
  }

  private async deleteMeme(id: string): Promise<void> {
    try {
      await this.memeApiService.deleteMeme(id).toPromise();
      
      this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'Meme template deleted successfully'
      });
      
      // Reload the list
      await this.loadMemeTemplates();
    } catch (error) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to delete meme template'
      });
      console.error('Failed to delete meme template:', error);
    }
  }

  formatDate(date: string): string {
    return new Date(date).toLocaleDateString();
  }

  onImageError(event: Event): void {
    const target = event.target as HTMLImageElement;
    if (target) {
      target.src = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjQiIGhlaWdodD0iNjQiIHZpZXdCb3g9IjAgMCA2NCA2NCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHJlY3Qgd2lkdGg9IjY0IiBoZWlnaHQ9IjY0IiBmaWxsPSIjRjNGNEY2Ii8+CjxwYXRoIGQ9Ik0yMS4zMzMzIDQyLjY2NjdIMTguNjY2N0MxNy45MzMzIDQyLjY2NjcgMTcuMzMzMyA0Mi4wNjY3IDE3LjMzMzMgNDEuMzMzM1YzOC42NjY3QzE3LjMzMzMgMzcuOTMzMyAxNy45MzMzIDM3LjMzMzMgMTguNjY2NyAzNy4zMzMzSDIxLjMzMzNDMjIuMDY2NyAzNy4zMzMzIDIyLjY2NjcgMzcuOTMzMyAyMi42NjY3IDM4LjY2NjdWNDEuMzMzM0MyMi42NjY3IDQyLjA2NjcgMjIuMDY2NyA0Mi42NjY3IDIxLjMzMzMgNDIuNjY2N1oiIGZpbGw9IiM5Q0E0QUYiLz4KPC9zdmc+';
    }
  }
}
