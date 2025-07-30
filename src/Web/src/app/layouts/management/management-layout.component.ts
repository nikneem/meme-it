import { Component, signal, effect, Renderer2, inject } from '@angular/core';
import { CommonModule, DOCUMENT } from '@angular/common';
import { RouterModule, RouterOutlet } from '@angular/router';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-management-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, RouterOutlet, ButtonModule],
  templateUrl: './management-layout.component.html',
  styleUrl: './management-layout.component.scss'
})
export class ManagementLayoutComponent {
  protected isMobileMenuOpen = signal(false);
  
  private renderer = inject(Renderer2);
  private document = inject(DOCUMENT);

  constructor() {
    // Effect to handle body scroll when mobile menu is open
    effect(() => {
      if (this.isMobileMenuOpen()) {
        this.renderer.addClass(this.document.body, 'mobile-menu-open');
      } else {
        this.renderer.removeClass(this.document.body, 'mobile-menu-open');
      }
    });
  }

  protected toggleMobileMenu(): void {
    this.isMobileMenuOpen.update(value => !value);
  }

  protected closeMobileMenu(): void {
    this.isMobileMenuOpen.set(false);
  }
}
