import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  isLoading = signal(false);
  errorMessage = '';

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading.set(true);
      this.errorMessage = '';

      const credentials = {
        email: this.loginForm.value.email!,
        password: this.loginForm.value.password!
      };

      this.authService.login(credentials).subscribe({
        next: (response) => {
          console.log('========== LOGIN SUCCESS ==========');
          console.log('Resposta:', response);
          console.log('Token:', response.token);
          console.log(
            'Token no localStorage:',
            localStorage.getItem('monkorc_token')
          );

          this.notificationService.success('Login realizado com sucesso!');

          console.log('ANTES DO NAVIGATE');

          this.router.navigate(['/dashboard']).then(result => {
            console.log('RESULTADO NAVIGATE:', result);
            console.log('URL ATUAL:', this.router.url);
          });
        },

        error: (err) => {
          console.error('========== LOGIN ERROR ==========');
          console.error(err);

          this.isLoading.set(false);

          const errorMsg =
            typeof err.error === 'string'
              ? err.error
              : err.error?.message || 'Credenciais inválidas ou erro no servidor.';

          this.notificationService.error(errorMsg);
          this.errorMessage = errorMsg;
        },

        complete: () => {
          console.log('LOGIN OBSERVABLE COMPLETE');
        }
      });
    }
  }
}

