import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-recovery',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './recovery.html',
  styleUrl: './recovery.scss',
})
export class Recovery {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);

  recoveryForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });

  isLoading = signal(false);

  onSubmit(): void {
    if (this.recoveryForm.valid) {
      this.isLoading.set(true);

      const email = this.recoveryForm.value.email!;

      this.authService.forgotPassword(email).subscribe({
        next: () => {
          this.notificationService.success('Se o e-mail existir, você receberá as instruções em breve.');
          this.isLoading.set(false);
          this.recoveryForm.reset();

        },
        error: () => {
          this.notificationService.error('Ocorreu um erro ao tentar recuperar a senha.');
          this.isLoading.set(false);
        }
      });
    } else {
      this.recoveryForm.markAllAsTouched();
      this.isLoading.set(false);
    }
  }
}
