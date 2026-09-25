import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.scss',
})
export class ResetPassword implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  email: string | null = null;
  token: string | null = null;
  isLoading = signal(false);

  resetPasswordForm = this.fb.group({
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', [Validators.required]],
  }, { validators: this.passwordMatchValidator });

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.email = params['email'];
      this.token = params['token'];
      
      if (!this.email || !this.token) {
        this.notificationService.error('Link de redefinição inválido.');
        this.router.navigate(['/login']);
      }
    });
  }

  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;

    if (password !== confirmPassword) {
      return { passwordMismatch: true };
    }

    return null;
  }

  onSubmit(): void {
    if (this.resetPasswordForm.valid && this.email && this.token) {
      this.isLoading.set(true);

      const newPassword = this.resetPasswordForm.value.password;

      this.authService.resetPassword({ email: this.email, token: this.token, newPassword }).subscribe({
        next: () => {
          this.notificationService.success('Senha redefinida com sucesso! Você já pode fazer login.');
          this.isLoading.set(false);
          this.router.navigate(['/login']);
        },
        error: () => {
          this.notificationService.error('Ocorreu um erro ao redefinir a senha.');
          this.isLoading.set(false);
        }
      });
    } else {
      this.resetPasswordForm.markAllAsTouched();
    }
  }
}
