import { Injectable, inject, signal } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private key = 'access_token';

  // Creamos un signal que se inicializa con el estado actual del localStorage
  private _isLogged = signal<boolean>(!!localStorage.getItem(this.key));

  // Exponemos el signal como solo lectura
  logged = this._isLogged.asReadonly();

  set(token: string) {
    localStorage.setItem(this.key, token);
    this._isLogged.set(true); // <--- Notifica a todos los interesados
  }

  get(): string | null {
    return localStorage.getItem(this.key);
  }

  clear() {
    localStorage.removeItem(this.key);
    this._isLogged.set(false); // <--- Notifica que ya no hay sesión
  }
}

export const authGuard: CanActivateFn = () => {
  const store = inject(AuthStore);
  const router = inject(Router);

  if (store.logged()) return true;
  router.navigateByUrl('/login');
  return false;
};
