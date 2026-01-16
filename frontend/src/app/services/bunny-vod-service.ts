import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface BunnyEmbedResponse {
  embedUrl: string;
  startAtSeconds: number;
}

export interface BunnyStreamResponse {
  streamUrl: string;
  startAtSeconds: number;
}

@Injectable({ providedIn: 'root' })
export class BunnyVodService {
  private readonly baseUrl = 'https://localhost:44312/api/bunny';

  constructor(private http: HttpClient) {}

  getEmbedUrl(videoId: string, opts?: { ttlSeconds?: number }): Observable<BunnyEmbedResponse> {
    let params = new HttpParams();
    if (opts?.ttlSeconds != null) params = params.set('ttlSeconds', opts.ttlSeconds);

    return this.http.get<BunnyEmbedResponse>(
      `${this.baseUrl}/embed-url/${encodeURIComponent(videoId)}`,
      { params }
    );
  }

  getStreamUrl(videoId: string, opts?: { ttlSeconds?: number }): Observable<BunnyStreamResponse> {
    let params = new HttpParams();
    if (opts?.ttlSeconds != null) params = params.set('ttlSeconds', opts.ttlSeconds);

    return this.http.get<BunnyStreamResponse>(
      `${this.baseUrl}/stream-url/${encodeURIComponent(videoId)}`,
      { params }
    );
  }
}
