import { Component, signal, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NotificationsComponent } from './components/notifications/notifications.component';
import { SettingsComponent } from './components/settings/settings.component';
import { AudioService } from './services/audio.service';

@Component({
  selector: 'memeit-root',
  imports: [RouterOutlet, NotificationsComponent, SettingsComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  protected readonly title = signal('MemeItApp');

  constructor(private audioService: AudioService) { }

  ngOnInit(): void {
    // AudioService is injected and will automatically initialize
    // Background music will start on first user interaction if enabled
  }
}
