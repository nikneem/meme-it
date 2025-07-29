import { Component } from '@angular/core';
import { Card } from "primeng/card";
import { ButtonModule } from 'primeng/button';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'meme-landing-page-cta',
  imports: [Card, ButtonModule, RouterModule],
  templateUrl: './landing-page-cta.html',
  styleUrl: './landing-page-cta.scss'
})
export class LandingPageCta {

}
