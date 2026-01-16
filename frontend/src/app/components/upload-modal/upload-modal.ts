import { Component, signal, output, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BunnyVideoService } from '../../services/bunny-videos-service';

@Component({
  selector: 'app-upload-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './upload-modal.html',
  styleUrls: ['./upload-modal.css'],
})
export class UploadModalComponent {
  libraryId = input<number>();

  file = signal<File | null>(null);
  progress = signal<number>(0);

  closed = output<void>();
  uploaded = output<string>();

  constructor(private service: BunnyVideoService) {}

  onFileSelected(event: any) {
    const f = event.target.files[0];
    this.file.set(f ?? null);
  }

  upload() {
    if (!this.file()) return;

    this.progress.set(1);

    this.service.uploadVideo(this.libraryId()!, this.file()!).subscribe((event) => {
      if (event.type === 1 && event.total) {
        this.progress.set(Math.round((event.loaded / event.total) * 100));
      }

      if (event.type === 4) {
        this.uploaded.emit(event.body.videoId);
      }
    });
  }

  close() {
    this.closed.emit();
  }
}
