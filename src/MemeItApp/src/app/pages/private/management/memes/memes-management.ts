import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { MemeService } from '@services/meme.service';
import { NotificationService } from '@services/notification.service';
import { MemeTemplate } from '@models/meme.model';

@Component({
    selector: 'memeit-memes-management',
    imports: [
        CommonModule,
        RouterLink,
        ReactiveFormsModule,
        MatButtonModule,
        MatCardModule,
        MatIconModule,
        MatInputModule,
        MatFormFieldModule,
        MatProgressSpinnerModule,
        MatPaginatorModule,
        MatDialogModule
    ],
    templateUrl: './memes-management.html',
    styleUrl: './memes-management.scss',
})
export class MemesManagementPage implements OnInit, OnDestroy {
    templates: MemeTemplate[] = [];
    filteredTemplates: MemeTemplate[] = [];
    paginatedTemplates: MemeTemplate[] = [];
    isLoading = false;
    searchControl = new FormControl('');

    // Pagination
    pageSize = 12;
    pageIndex = 0;
    totalItems = 0;

    private destroy$ = new Subject<void>();

    constructor(
        private memeService: MemeService,
        private notificationService: NotificationService,
        private dialog: MatDialog
    ) { }

    ngOnInit(): void {
        this.loadTemplates();
        this.setupSearch();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    loadTemplates(): void {
        this.isLoading = true;
        this.memeService.getTemplates()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (templates) => {
                    this.templates = templates;
                    this.applyFilters();
                    this.isLoading = false;
                },
                error: (error) => {
                    console.error('Failed to load meme templates:', error);
                    this.notificationService.error(
                        'Load Failed',
                        'Failed to load meme templates. Please try again.',
                        undefined,
                        5000
                    );
                    this.isLoading = false;
                }
            });
    }

    setupSearch(): void {
        this.searchControl.valueChanges
            .pipe(
                debounceTime(300),
                distinctUntilChanged(),
                takeUntil(this.destroy$)
            )
            .subscribe(() => {
                this.pageIndex = 0; // Reset to first page on search
                this.applyFilters();
            });
    }

    applyFilters(): void {
        const searchTerm = this.searchControl.value?.toLowerCase() || '';

        if (searchTerm) {
            this.filteredTemplates = this.templates.filter(template =>
                template.title.toLowerCase().includes(searchTerm)
            );
        } else {
            this.filteredTemplates = [...this.templates];
        }

        this.totalItems = this.filteredTemplates.length;
        this.updatePaginatedTemplates();
    }

    updatePaginatedTemplates(): void {
        const startIndex = this.pageIndex * this.pageSize;
        const endIndex = startIndex + this.pageSize;
        this.paginatedTemplates = this.filteredTemplates.slice(startIndex, endIndex);
    }

    onPageChange(event: PageEvent): void {
        this.pageIndex = event.pageIndex;
        this.pageSize = event.pageSize;
        this.updatePaginatedTemplates();
    }

    deleteTemplate(template: MemeTemplate, event: Event): void {
        event.stopPropagation();

        if (!confirm(`Are you sure you want to delete "${template.title}"? This action cannot be undone.`)) {
            return;
        }

        this.memeService.deleteTemplate(template.id)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: () => {
                    this.notificationService.success(
                        'Deleted',
                        `Meme template "${template.title}" has been deleted.`,
                        undefined,
                        3000
                    );
                    this.loadTemplates();
                },
                error: (error) => {
                    console.error('Failed to delete meme template:', error);
                    this.notificationService.error(
                        'Delete Failed',
                        'Failed to delete the meme template. Please try again.',
                        undefined,
                        5000
                    );
                }
            });
    }

    getTextAreaCount(template: MemeTemplate): number {
        return template.textAreas?.length || 0;
    }

    formatDate(dateString: string): string {
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    }

    clearSearch(): void {
        this.searchControl.setValue('');
    }
}
