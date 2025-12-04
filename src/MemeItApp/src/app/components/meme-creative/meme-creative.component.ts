import { Component, Input, OnInit, OnDestroy, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormsModule } from '@angular/forms';
import { GameService } from '@services/game.service';
import { NotificationService } from '@services/notification.service';
import { MemeService } from '@services/meme.service';
import { MemeTemplate } from '@models/meme.model';

@Component({
    selector: 'memeit-meme-creative',
    imports: [
        CommonModule,
        FormsModule,
        MatButtonModule,
        MatCardModule,
        MatFormFieldModule,
        MatInputModule,
        MatProgressSpinnerModule
    ],
    templateUrl: './meme-creative.component.html',
    styleUrl: './meme-creative.component.scss',
})
export class MemeCreativeComponent implements OnInit, OnDestroy {
    @Input() gameCode: string = '';
    @Input() roundNumber: number = 1;
    @Output() memeSubmitted = new EventEmitter<void>();

    memeTemplate: MemeTemplate | null = null;
    textInputs: string[] = [];
    isLoading = true;
    errorMessage = '';
    memeChangesRemaining = 2;
    hasSubmittedMeme = false;

    constructor(
        private gameService: GameService,
        private memeService: MemeService,
        private notificationService: NotificationService
    ) { }

    ngOnInit(): void {
        this.loadPlayerRoundState();
    }

    ngOnDestroy(): void {
        // Cleanup if needed
    }

    private loadPlayerRoundState(): void {
        this.isLoading = true;

        this.gameService.getPlayerRoundState(this.gameCode).subscribe({
            next: (state) => {
                // If player already has a meme selected, load it
                if (state.selectedMemeTemplateId) {
                    this.loadMemeById(state.selectedMemeTemplateId);
                } else {
                    // Otherwise, load a random meme
                    this.loadRandomMeme();
                }
            },
            error: (error) => {
                console.error('Failed to load player round state:', error);
                this.isLoading = false;
                this.errorMessage = 'Failed to load game state. Please try again.';
                this.notificationService.error('Error', 'Failed to load game state');
            }
        });
    }

    private loadMemeById(memeId: string): void {
        this.memeService.getTemplateById(memeId).subscribe({
            next: (template) => {
                this.memeTemplate = this.ensureTemplateDimensions(template);
                this.textInputs = new Array(template.textAreas.length).fill('');
                this.isLoading = false;
                this.memeChangesRemaining = 0; // Already selected, no changes allowed
            },
            error: (error: any) => {
                console.error('Failed to load meme by ID:', error);
                // Fallback to random meme
                this.loadRandomMeme();
            }
        });
    }

    private ensureTemplateDimensions(template: MemeTemplate): MemeTemplate {
        // If dimensions are not set, use defaults or load from image
        if (!template.width || !template.height) {
            template.width = 800;
            template.height = 600;
        }
        return template;
    }

    private loadRandomMeme(): void {
        this.isLoading = true;

        this.memeService.getRandomTemplate().subscribe({
            next: (template) => {
                this.memeTemplate = this.ensureTemplateDimensions(template);
                this.textInputs = new Array(template.textAreas.length).fill('');
                this.isLoading = false;
                this.hasSubmittedMeme = false;

                // Select this meme template for the round
                if (template.id) {
                    this.gameService.selectMemeTemplate(this.gameCode, this.roundNumber, template.id).subscribe({
                        next: () => {
                            console.log('Meme template selected successfully');
                        },
                        error: (error) => {
                            console.error('Failed to select meme template:', error);
                        }
                    });
                }
            },
            error: (error) => {
                console.error('Failed to load random meme:', error);
                this.isLoading = false;
                this.errorMessage = 'Failed to load meme template. Please try again.';
                this.notificationService.error('Error', 'Failed to load meme template');
            }
        });
    }

    selectNewMeme(): void {
        if (this.memeChangesRemaining > 0) {
            this.memeChangesRemaining--;
            this.loadRandomMeme();

            if (this.memeChangesRemaining === 0) {
                this.notificationService.info('Notice', 'This is your last meme selection!');
            }
        }
    }

    get canSelectNewMeme(): boolean {
        return this.memeChangesRemaining > 0;
    }

    submitMeme(): void {
        if (!this.memeTemplate) return;

        // Map text inputs to text entries with generated text field IDs
        // Use a deterministic approach: combine template ID with index to generate consistent GUIDs
        const textEntries = this.memeTemplate.textAreas.map((textArea, index) => {
            // Generate a consistent GUID for each text field based on template ID and index
            const fieldId = this.generateTextFieldId(this.memeTemplate!.id, index);
            return {
                textFieldId: fieldId,
                value: this.textInputs[index] || ''
            };
        });

        this.gameService.submitMeme(this.gameCode, this.roundNumber, this.memeTemplate.id, textEntries).subscribe({
            next: () => {
                this.hasSubmittedMeme = true;
                this.notificationService.success('Success', 'Meme submitted successfully!');
                this.memeSubmitted.emit();
            },
            error: (error) => {
                console.error('Failed to submit meme:', error);
                this.notificationService.error('Error', 'Failed to submit meme. Please try again.');
            }
        });
    }

    private generateTextFieldId(templateId: string, index: number): string {
        // Create a deterministic GUID based on template ID and index
        // This ensures consistency between selecting and submitting
        const combined = `${templateId}-field-${index}`;
        // Simple hash to GUID (in production, use a proper UUID v5 or consistent hashing)
        return this.hashToGuid(combined);
    }

    private hashToGuid(input: string): string {
        // Simple deterministic GUID generation
        // Format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
        let hash = 0;
        for (let i = 0; i < input.length; i++) {
            hash = ((hash << 5) - hash) + input.charCodeAt(i);
            hash = hash & hash;
        }
        const hex = Math.abs(hash).toString(16).padStart(8, '0');
        // Create a GUID-like string
        return `${hex.slice(0, 8)}-${hex.slice(0, 4)}-${hex.slice(0, 4)}-${hex.slice(0, 4)}-${hex.padEnd(12, '0')}`;
    }

    wrapText(text: string, maxWidth: number, fontSize: number): string[] {
        if (!text) return [''];

        // Approximate character width based on font size (roughly 0.6 of fontSize for average characters)
        const avgCharWidth = fontSize * 0.6;
        const maxCharsPerLine = Math.floor(maxWidth / avgCharWidth);

        if (maxCharsPerLine <= 0) return [text];

        const words = text.split(' ');
        const lines: string[] = [];
        let currentLine = '';

        for (const word of words) {
            const testLine = currentLine ? `${currentLine} ${word}` : word;

            if (testLine.length <= maxCharsPerLine) {
                currentLine = testLine;
            } else {
                if (currentLine) {
                    lines.push(currentLine);
                }
                // If single word is too long, break it
                if (word.length > maxCharsPerLine) {
                    let remainingWord = word;
                    while (remainingWord.length > maxCharsPerLine) {
                        lines.push(remainingWord.substring(0, maxCharsPerLine));
                        remainingWord = remainingWord.substring(maxCharsPerLine);
                    }
                    currentLine = remainingWord;
                } else {
                    currentLine = word;
                }
            }
        }

        if (currentLine) {
            lines.push(currentLine);
        }

        return lines.length > 0 ? lines : [''];
    }
}
