import { Component, EventEmitter, inject, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth-service'; // Asegúrate que esta ruta sea correcta

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class LoginComponent {
  private authApi = inject(AuthService);

  // Datos del formulario
  loginData = { email: '', password: '' };

  // Estados de la interfaz
  isLoading = false;
  errorMessage = '';

  // Evento para avisar al AppComponent que cierre el modal/cuadro
  @Output() loginSuccess = new EventEmitter<void>();

  onLogin() {
    this.isLoading = true;
    this.errorMessage = ''; // Limpiar errores previos

    this.authApi.login(this.loginData).subscribe({
      next: () => {
        this.isLoading = false;
        this.loginSuccess.emit(); // Cierra el modal en el padre
      },
      error: (err) => {
        this.isLoading = false;
        // Manejo de errores basado en la respuesta del Backend C#
        if (err.status === 401) {
          this.errorMessage = 'Credenciales incorrectas.';
        } else {
          this.errorMessage = 'Ocurrió un error inesperado. Inténtalo de nuevo.';
        }
        console.error('Login error:', err);
      },
    });
  }
}
