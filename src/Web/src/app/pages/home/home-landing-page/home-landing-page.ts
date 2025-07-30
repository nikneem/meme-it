import { Component } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { AppTitleComponent } from "../../../shared/components/app-title-component/app-title-component";
import { CardModule } from 'primeng/card';
import { LandingPageCta } from "../../../shared/components/landing-page-cta/landing-page-cta";
import { LandingPageShinyBackground } from "../../../shared/components/landing-page-shiny-background/landing-page-shiny-background";

@Component({
  selector: 'meme-home-landing-page',
  imports: [
    TranslateModule,
    AppTitleComponent, 
    CardModule, 
    LandingPageCta, 
    LandingPageShinyBackground
  ],
  templateUrl: './home-landing-page.html',
  styleUrl: './home-landing-page.scss'
})
export class HomeLandingPage {

}
