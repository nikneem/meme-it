import { Component } from '@angular/core';
import { AppTitleComponent } from "../../../shared/components/app-title-component/app-title-component";
import { LandingPageCta } from "../../../shared/components/landing-page-cta/landing-page-cta";
import { LandingPageShinyBackground } from "../../../shared/components/landing-page-shiny-background/landing-page-shiny-background";
import { GameFlowComponent, FunFeaturesComponent, PartyOccasionsComponent } from '../components';

@Component({
  selector: 'meme-home-landing-page',
  imports: [
    AppTitleComponent, 
    LandingPageCta, 
    LandingPageShinyBackground,
    GameFlowComponent,
    FunFeaturesComponent,
    PartyOccasionsComponent
  ],
  templateUrl: './home-landing-page.html',
  styleUrl: './home-landing-page.scss'
})
export class HomeLandingPage {

}
