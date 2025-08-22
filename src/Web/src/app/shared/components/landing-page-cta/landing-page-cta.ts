import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'meme-landing-page-cta',
  imports: [ButtonModule],
  templateUrl: './landing-page-cta.html',
  styleUrl: './landing-page-cta.scss'
})
export class LandingPageCta {
  
  constructor(private router: Router) {}

  onCreateGame() {
    this.router.navigate(['/game'], { queryParams: { tab: 'create' } });
  }

  onJoinGame() {
    this.router.navigate(['/game'], { queryParams: { tab: 'join' } });
  }
}
