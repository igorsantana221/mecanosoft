import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);
  private router = inject(Router);
  private http = inject(HttpClient);

  registerForm = this.fb.group({
    name: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    companyName: ['', [Validators.required]],
    cnpj: ['', [Validators.required]],
    phone: ['', [Validators.required]],
    cep: ['', [Validators.required, Validators.minLength(8)]],
    street: ['', [Validators.required]],
    number: ['', [Validators.required]],
    complement: [''],
    neighborhood: ['', [Validators.required]],
    city: ['', [Validators.required]],
    state: ['', [Validators.required, Validators.maxLength(2)]]
  });

  isLoading = signal(false);
  isLoadingCep = false;
  errorMessage = '';

  searchCep(): void {
    const cep = this.registerForm.get('cep')?.value?.replace(/\D/g, '');
    if (cep && cep.length === 8) {
      this.isLoadingCep = true;
      this.http.get<any>(`https://viacep.com.br/ws/${cep}/json/`).subscribe({
        next: (data) => {
          this.isLoadingCep = false;
          if (!data.erro) {
            this.registerForm.patchValue({
              street: data.logradouro,
              neighborhood: data.bairro,
              city: data.localidade,
              state: data.uf
            });
          } else {
            this.notificationService.error('CEP não encontrado.');
          }
        },
        error: () => {
          this.isLoadingCep = false;
          this.notificationService.error('Erro ao buscar o CEP.');
        }
      });
    }
  }

  onSubmit(): void {
    if (this.registerForm.valid) {
      this.isLoading.set(true);
      this.errorMessage = '';

      const userData = {
        name: this.registerForm.value.name!,
        email: this.registerForm.value.email!,
        password: this.registerForm.value.password!,
        companyName: this.registerForm.value.companyName!,
        cnpj: this.registerForm.value.cnpj!,
        phone: this.registerForm.value.phone!,
        cep: this.registerForm.value.cep!,
        street: this.registerForm.value.street!,
        number: this.registerForm.value.number!,
        complement: this.registerForm.value.complement || undefined,
        neighborhood: this.registerForm.value.neighborhood!,
        city: this.registerForm.value.city!,
        state: this.registerForm.value.state!
      };

      this.authService.register(userData).subscribe({
        next: () => {
          this.notificationService.success('Cadastro realizado! Por favor, verifique seu e-mail.');
          this.isLoading.set(false);
          this.router.navigate(['/login']);
        },
        error: (err) => {
          this.isLoading.set(false);
          const errorMsg = typeof err.error === 'string' ? err.error : err.error?.message || 'Erro ao realizar cadastro. Tente novamente.';
          this.notificationService.error(errorMsg);
          this.errorMessage = errorMsg;
        }
      });
    } else {
      this.registerForm.markAllAsTouched();
    }
  }
}
