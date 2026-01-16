import { Component, effect, input, signal, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import {
  BunnyEmbedResponse,
  BunnyStreamResponse,
  BunnyVodService,
} from '../../services/bunny-vod-service';
import { AuthStore } from '../../core/auth.service';
import Hls from 'hls.js';
import { Observable, Subscription } from 'rxjs';
import { VideoProgressService } from '../../services/video-progress-service';

type ViewState = 'idle' | 'loading' | 'ready' | 'error' | 'unauthorized';

@Component({
  selector: 'app-vod-player',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './vod-player.html',
  styleUrls: ['./vod-player.css'],
})
export class VodPlayerComponent implements OnDestroy {
  private vod = inject(BunnyVodService);
  private sanitizer = inject(DomSanitizer);
  public auth = inject(AuthStore);

  private progressApi: VideoProgressService = inject(VideoProgressService);

  videoId = input<string>('');
  playerMode: 'bunny' | 'video' = 'video';

  state = signal<ViewState>('idle');
  safeUrl = signal<SafeResourceUrl | null>(null);

  private lastIdLoaded = '';
  private pendingStartAt = 0;

  private hls: Hls | null = null;

  private videoEl: HTMLVideoElement | null = null;
  private detachVideoListeners: (() => void) | null = null;

  private lastSentAtSeconds = 0;
  private lastSentWallClock = 0;
  private progressSub: Subscription | null = null;

  constructor() {
    effect(() => {
      const id = this.videoId()?.trim();
      const logged = this.auth.logged();

      if (!id) {
        this.resetPlayer('idle');
        return;
      }

      if (!logged) {
        this.resetPlayer('unauthorized');
        return;
      }

      if (id !== this.lastIdLoaded) {
        this.load(id);
      }
    });
  }

  ngOnDestroy(): void {
    this.flushProgress('destroy');
    this.cleanupPlayback();
  }

  private resetPlayer(newState: ViewState) {
    this.flushProgress('reset');

    this.state.set(newState);
    this.safeUrl.set(null);
    this.lastIdLoaded = '';
    this.pendingStartAt = 0;

    this.cleanupPlayback();
  }

  private cleanupPlayback() {
    this.destroyHls();
    this.detachTracking();
    this.progressSub?.unsubscribe();
    this.progressSub = null;
  }

  private destroyHls() {
    if (this.hls) {
      this.hls.destroy();
      this.hls = null;
    }
  }

  private detachTracking() {
    if (this.detachVideoListeners) {
      this.detachVideoListeners();
      this.detachVideoListeners = null;
    }
    this.videoEl = null;
  }

  reload() {
    const id = this.videoId()?.trim();
    if (id) this.load(id);
  }

  private load(id: string) {
    if (this.state() === 'loading' && id === this.lastIdLoaded) return;

    this.flushProgress('change-video');

    this.state.set('loading');
    this.lastIdLoaded = id;

    const mode = this.playerMode;

    let request: Observable<BunnyEmbedResponse | BunnyStreamResponse>;

    if (mode === 'video') {
      request = this.vod.getStreamUrl(id, { ttlSeconds: 900 });
    } else {
      request = this.vod.getEmbedUrl(id, { ttlSeconds: 900 });
    }

    request.subscribe({
      next: (res) => {
        let url: string | null = null;

        if (mode === 'video' && 'streamUrl' in res) {
          url = res.streamUrl;
          this.pendingStartAt = res.startAtSeconds ?? 0;
        }

        if (mode === 'bunny' && 'embedUrl' in res) {
          url = res.embedUrl;
          this.pendingStartAt = res.startAtSeconds ?? 0;
        }

        if (!url) {
          this.state.set('error');
          return;
        }

        if (mode === 'bunny') {
          this.safeUrl.set(this.sanitizer.bypassSecurityTrustResourceUrl(url));
        }

        this.state.set('ready');

        if (mode === 'video') {
          setTimeout(() => this.initHlsPlayer(url!), 0);
        }
      },

      error: (err: any) => {
        this.lastIdLoaded = '';
        this.state.set(err.status === 401 ? 'unauthorized' : 'error');
      },
    });
  }

  private initHlsPlayer(url: string) {
    const video = document.getElementById('html5-player') as HTMLVideoElement;
    if (!video) return;

    this.cleanupPlayback();

    this.videoEl = video;
    this.lastSentAtSeconds = 0;
    this.lastSentWallClock = 0;

    const applySeek = () => {
      if (this.pendingStartAt > 0 && isFinite(video.duration)) {
        const target = Math.min(this.pendingStartAt, Math.max(0, video.duration - 2));
        video.currentTime = target;
        this.pendingStartAt = 0;
      }
    };

    video.addEventListener('loadedmetadata', applySeek, { once: true });

    if (Hls.isSupported()) {
      this.hls = new Hls();
      this.hls.loadSource(url);
      this.hls.attachMedia(video);

      this.hls.on(Hls.Events.MANIFEST_PARSED, () => applySeek());
    } else {
      video.src = url;
    }

    this.attachProgressTracking(video);

    video.play().catch(() => {});
  }

  private attachProgressTracking(video: HTMLVideoElement) {
    const onTimeUpdate = () => this.maybeSendProgress('timeupdate');
    const onPause = () => this.flushProgress('pause');
    const onEnded = () => this.flushProgress('ended');
    const onSeeked = () => this.maybeSendProgress('seeked');

    const onVisibility = () => {
      if (document.visibilityState === 'hidden') this.flushProgress('hidden');
    };
    const onPageHide = () => this.flushProgress('pagehide');

    video.addEventListener('timeupdate', onTimeUpdate);
    video.addEventListener('pause', onPause);
    video.addEventListener('ended', onEnded);
    video.addEventListener('seeked', onSeeked);

    document.addEventListener('visibilitychange', onVisibility);
    window.addEventListener('pagehide', onPageHide);

    this.detachVideoListeners = () => {
      video.removeEventListener('timeupdate', onTimeUpdate);
      video.removeEventListener('pause', onPause);
      video.removeEventListener('ended', onEnded);
      video.removeEventListener('seeked', onSeeked);

      document.removeEventListener('visibilitychange', onVisibility);
      window.removeEventListener('pagehide', onPageHide);
    };
  }

  private maybeSendProgress(reason: string) {
    const v = this.videoEl;
    const id = this.videoId()?.trim();
    if (!v || !id) return;

    if (!isFinite(v.duration) || v.duration <= 0) return;

    const current = v.currentTime;
    if (!isFinite(current) || current < 0) return;

    const now = Date.now();

    const advancedEnough = current - this.lastSentAtSeconds >= 5;
    const timeEnough = now - this.lastSentWallClock >= 8000;

    if (!advancedEnough && !timeEnough) return;

    this.sendProgressSnapshot(id, current, v.duration, reason);
  }

  private flushProgress(reason: string) {
    const v = this.videoEl;
    const id = this.videoId()?.trim();
    if (!v || !id) return;

    if (!isFinite(v.duration) || v.duration <= 0) return;
    if (!isFinite(v.currentTime) || v.currentTime < 0) return;

    this.sendProgressSnapshot(id, v.currentTime, v.duration, reason);
  }

  private sendProgressSnapshot(
    videoId: string,
    positionSeconds: number,
    durationSeconds: number,
    reason: string
  ) {
    this.progressSub?.unsubscribe();

    this.lastSentAtSeconds = positionSeconds;
    this.lastSentWallClock = Date.now();

    this.progressSub = this.progressApi
      .upsert({
        videoId,
        positionSeconds,
        durationSeconds,
      })
      .subscribe({
        next: () => {},
        error: () => {},
      });
  }
}
