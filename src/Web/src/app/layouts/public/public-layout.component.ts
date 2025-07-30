import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { ConfigPanelComponent } from '../../shared/components/config-panel/config-panel.component';
import { AudioService } from '../../shared/services/audio.service';

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    TranslateModule,
    ConfigPanelComponent
  ],
  template: `
    <div class="public-layout" (click)="enableAudio()">
      <!-- Main Content Area -->
      <main class="main-content">
        <router-outlet></router-outlet>
      </main>

      <!-- Floating Configuration Panel -->
      <app-config-panel></app-config-panel>
    </div>
  `,
  styles: [`
    .public-layout {
      min-height: 100vh;
      position: relative;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    }

    .main-content {
      min-height: 100vh;
      width: 100%;
      position: relative;
      z-index: 1;
    }

    /* Ensure the config panel is above everything */
    :host ::ng-deep app-config-panel {
      position: relative;
      z-index: 1001;
    }
  `]
})
export class PublicLayoutComponent implements OnInit {
  private audioService = inject(AudioService);
  private audioEnabled = false;

  ngOnInit(): void {
    // Audio will be initialized automatically
  }

  enableAudio(): void {
    if (!this.audioEnabled) {
      this.audioService.enableAudio();
      this.audioEnabled = true;
    }
  }
}
