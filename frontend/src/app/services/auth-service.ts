import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { AuthStore } from '../core/auth.service';

interface AuthResponse {
  accessToken: string;
  expiresInSeconds: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private authStore = inject(AuthStore);
  private readonly API_URL = 'https://localhost:44312/api/auth';

  login(credentials: any): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.API_URL}/login`, credentials)
      .pipe(tap((res) => this.authStore.set(res.accessToken)));
  }

  logout() {
    this.authStore.clear();
  }
}
