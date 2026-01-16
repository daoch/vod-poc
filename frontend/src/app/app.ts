import { Component, inject, input, signal } from '@angular/core';
import { VodPlayerComponent } from './components/vod-player/vod-player';
import { AuthStore } from './core/auth.service';
import { AuthService } from './services/auth-service';
import { LoginComponent } from './components/login/login';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [LoginComponent, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  public authStore = inject(AuthStore);
  private authApi = inject(AuthService);

  showLogin = false;

  videoId = signal<string>(this.getVideoIdFromUrl());

  constructor() {
    console.log('App inicializada con ID inmediato:', this.videoId());
  }

  private getVideoIdFromUrl(): string {
    const path = window.location.pathname;
    const match = path.match(/\/video\/([^\/]+)/);
    return match ? match[1] : '';
  }

  logout() {
    this.authApi.logout();
    this.showLogin = false;
  }
}
