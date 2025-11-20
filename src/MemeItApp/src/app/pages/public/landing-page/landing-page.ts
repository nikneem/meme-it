import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'memeit-landing-page',
  imports: [CommonModule, RouterLink],
  templateUrl: './landing-page.html',
  styleUrl: './landing-page.scss',
})
export class LandingPage {
  protected readonly features = [
    {
      title: 'Real-Time Chaos',
      description: 'Host live meme battles with instant updates across every player device.'
    },
    {
      title: 'Curated Templates',
      description: 'Browse a constantly refreshed gallery of meme formats pulled from trending culture.'
    },
    {
      title: 'Smart Voting',
      description: 'Anonymous scoring keeps things fair while spotlighting the funniest submissions.'
    },
    {
      title: 'Clip & Share',
      description: 'Export highlight reels or share single memez directly to your socials with one tap.'
    }
  ];

  protected readonly steps = [
    {
      label: '01',
      title: 'Create the Room',
      description: 'Spin up a private lobby in seconds and drop the join code in your chat.'
    },
    {
      label: '02',
      title: 'Craft the Memes',
      description: 'Everyone responds to the prompt with their spiciest take, gifs, or captions.'
    },
    {
      label: '03',
      title: 'Crown the MVP',
      description: 'Vote, roast, repeat—then share the leaderboard to immortalize the chaos.'
    }
  ];

  protected readonly testimonials = [
    {
      quote: 'Instant classic for game night. We laughed so hard we forgot to check our phones.',
      author: 'Valentina — Meme League Host'
    },
    {
      quote: 'Finally a party game that feels made for group chats and remote teams alike.',
      author: 'Noah — Remote Team Lead'
    }
  ];
}
