import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UploadModalComponent } from '../upload-modal/upload-modal';
import { BunnyVideoService } from '../../services/bunny-videos-service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-video-list-page',
  standalone: true,
  imports: [CommonModule, UploadModalComponent],
  templateUrl: './video-list-page.html',
  styleUrls: ['./video-list-page.css'],
})
export class VideoListPageComponent {
  libraryId = 576936;

  private router = inject(Router);
  private service = inject(BunnyVideoService);

  constructor() {
    this.load();
  }

  videos = signal<any[]>([]);
  loading = signal<boolean>(true);
  showUpload = signal<boolean>(false);

  load() {
    this.loading.set(true);

    this.service.listVideos(this.libraryId, 1, 100).subscribe((resp) => {
      this.videos.set(resp.items);
      this.loading.set(false);
    });
  }

  openUpload() {
    this.showUpload.set(true);
  }

  closeUpload() {
    this.showUpload.set(false);
  }

  onUploaded(videoId: string) {
    this.showUpload.set(false);
    this.load();
  }

  goToVideo(guid: string) {
    this.router.navigate(['/video', guid]);
  }
}
