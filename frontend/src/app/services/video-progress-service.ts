import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UpsertProgressRequest {
  videoId: string;
  positionSeconds: number;
  durationSeconds: number;
}

export interface ProgressResponse {
  videoId: string;
  positionSeconds: number;
  durationSeconds: number;
  completed: boolean;
  updatedAt: string;
}

@Injectable({ providedIn: 'root' })
export class VideoProgressService {
  private readonly baseUrl = 'https://localhost:44312/api/progress';

  constructor(private http: HttpClient) {}

  upsert(req: UpsertProgressRequest): Observable<ProgressResponse> {
    return this.http.post<ProgressResponse>(this.baseUrl, req);
  }
}
