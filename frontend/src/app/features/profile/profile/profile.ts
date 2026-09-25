import { Component, inject, OnInit, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.scss'
})
export class Profile implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);

  user = this.authService.currentUser;

  profileForm = this.fb.group({
    name: ['', [Validators.required]],
    email: [{ value: '', disabled: true }, [Validators.required, Validators.email]],
    phone: ['']
  });

  passwordForm = this.fb.group({
    currentPassword: ['', [Validators.required, Validators.minLength(6)]],
    newPassword: ['', [Validators.required, Validators.minLength(6)]],
    confirmNewPassword: ['', [Validators.required]]
  }, {
    validators: (group) => {
      const pass = group.get('newPassword')?.value;
      const confirmPass = group.get('confirmNewPassword')?.value;
      return pass === confirmPass ? null : { passwordMismatch: true };
    }
  });

  isSubmittingProfile = signal(false);
  isSubmittingPassword = signal(false);
  isUploadingAvatar = signal(false);

  profileSuccess = signal<string | null>(null);
  profileError = signal<string | null>(null);

  passwordSuccess = signal<string | null>(null);
  passwordError = signal<string | null>(null);

  previewAvatarUrl = signal<string | null>(null);

  constructor() {
    // Atualiza o formulário se o usuário logado mudar
    effect(() => {
      const currentUser = this.user();
      if (currentUser) {
        this.profileForm.patchValue({
          name: currentUser.name,
          email: currentUser.email,
          phone: this.formatPhone(currentUser.phone || '')
        });
        this.previewAvatarUrl.set(currentUser.avatarUrl || null);
      }
    });
  }

  ngOnInit() {
    const currentUser = this.user();
    if (currentUser) {
      this.profileForm.patchValue({
        name: currentUser.name,
        email: currentUser.email,
        phone: this.formatPhone(currentUser.phone || '')
      });
      this.previewAvatarUrl.set(currentUser.avatarUrl || null);
    }
  }

  getInitials(name: string | undefined): string {
    if (!name) return '??';
    const parts = name.trim().split(' ');
    if (parts.length > 1) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  }

  formatPhone(value: string): string {
    const digits = value.replace(/\D/g, '');
    if (digits.length === 0) return '';
    
    let trimmed = digits;
    if (trimmed.length > 13) {
      trimmed = trimmed.substring(0, 13);
    }

    let formatted = '';
    if (trimmed.length > 0) {
      formatted += '+' + trimmed.substring(0, 2);
    }
    if (trimmed.length > 2) {
      formatted += ' (' + trimmed.substring(2, 4);
    }
    if (trimmed.length > 4) {
      formatted += ') ' + trimmed.substring(4, 9);
    }
    if (trimmed.length > 9) {
      formatted += '-' + trimmed.substring(9, 13);
    }
    return formatted;
  }

  onPhoneInput(event: Event) {
    const input = event.target as HTMLInputElement;
    const formatted = this.formatPhone(input.value);
    input.value = formatted;
    this.profileForm.get('phone')?.setValue(formatted, { emitEvent: false });
  }

  onFileSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    if (file.size > 5 * 1024 * 1024) {
      alert('A imagem não pode ter mais de 5MB.');
      return;
    }

    this.isUploadingAvatar.set(true);

    this.authService.uploadAvatar(file).subscribe({
      next: (res) => {
        const fullUrl = `https://localhost:44329${res.url}`;
        this.previewAvatarUrl.set(fullUrl);
        this.isUploadingAvatar.set(false);
        
        // Salva a nova url da foto atualizando o perfil imediatamente
        this.updateUserProfile(this.profileForm.value.name || '', this.profileForm.value.phone || '', fullUrl);
      },
      error: (err) => {
        console.error('Erro no upload do avatar', err);
        alert('Erro ao enviar foto.');
        this.isUploadingAvatar.set(false);
      }
    });
  }

  onSubmitProfile() {
    if (this.profileForm.invalid) return;

    this.isSubmittingProfile.set(true);
    this.profileSuccess.set(null);
    this.profileError.set(null);

    const { name, phone } = this.profileForm.value;

    this.updateUserProfile(name || '', phone || '', this.previewAvatarUrl() || undefined);
  }

  private updateUserProfile(name: string, phone: string, avatarUrl?: string) {
    this.authService.updateProfile({ name, phone, avatarUrl }).subscribe({
      next: () => {
        this.profileSuccess.set('Perfil atualizado com sucesso!');
        this.isSubmittingProfile.set(false);
      },
      error: (err) => {
        console.error('Erro ao atualizar perfil', err);
        this.profileError.set('Erro ao salvar alterações.');
        this.isSubmittingProfile.set(false);
      }
    });
  }

  onSubmitPassword() {
    if (this.passwordForm.invalid) return;

    this.isSubmittingPassword.set(true);
    this.passwordSuccess.set(null);
    this.passwordError.set(null);

    const { currentPassword, newPassword } = this.passwordForm.value;

    this.authService.changePassword({ currentPassword, newPassword }).subscribe({
      next: () => {
        this.passwordSuccess.set('Senha alterada com sucesso!');
        this.passwordForm.reset();
        this.isSubmittingPassword.set(false);
      },
      error: (err) => {
        console.error('Erro ao alterar senha', err);
        this.passwordError.set(err.error || 'Erro ao alterar senha. Verifique sua senha atual.');
        this.isSubmittingPassword.set(false);
      }
    });
  }
}
