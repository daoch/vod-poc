import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface BunnyVideoItem {
  guid: string;
  title: string;
  dateUploaded: string;
  availableResolutions: string;
  length: number;
  storageSize: number;
  thumbnailLink: string;
  progressPercent?: number;
}

export interface BunnyVideoListResponse {
  currentPage: number;
  itemsPerPage: number;
  totalItems: number;
  hasMoreItems: boolean;
  items: BunnyVideoItem[];
}

@Injectable({ providedIn: 'root' })
export class BunnyVideoService {
  private baseUrl = 'https://localhost:44312/api/bunny/videos';

  constructor(private http: HttpClient) {}

  listVideos(libraryId: number, page = 1, itemsPerPage = 50, search?: string) {
    return this.http.get<BunnyVideoListResponse>(`${this.baseUrl}/list/${libraryId}`, {
      params: { page, itemsPerPage, search: search ?? '' },
    });
  }

  uploadVideo(libraryId: number, file: File): Observable<HttpEvent<any>> {
    const formData = new FormData();
    formData.append('file', file);

    const req = new HttpRequest('POST', `${this.baseUrl}/upload/${libraryId}`, formData, {
      reportProgress: true,
    });

    return this.http.request(req);
  }
}
