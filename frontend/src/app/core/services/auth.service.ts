import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = 'https://localhost:44329/api/Auth';

  // Signals for state management (Modern Angular)
  currentUser = signal<AuthResponse | null>(null);
  isAuthenticated = signal<boolean>(false);

  constructor(private http: HttpClient) {
    this.checkLocalStorage();
  }

  register(data: RegisterRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, data);
  }

  login(data: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, data).pipe(
      tap(response => {
        this.setSession(response);
      })
    );
  }

  logout(): void {
    localStorage.removeItem('monkorc_token');
    localStorage.removeItem('monkorc_user');
    this.currentUser.set(null);
    this.isAuthenticated.set(false);
  }

  /**
   * Decodifica o payload do JWT e verifica se o token está expirado.
   * Não faz chamada à API — ideal para guards de rota em SPA multi-tenant.
   */
  isTokenExpired(): boolean {
    const token = localStorage.getItem('monkorc_token');
    if (!token) return true;

    try {
      const payloadBase64 = token.split('.')[1];
      const payload = JSON.parse(atob(payloadBase64));
      const nowInSeconds = Math.floor(Date.now() / 1000);
      return payload.exp < nowInSeconds;
    } catch {
      return true;
    }
  }

  confirmEmail(email: string, token: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/confirm-email`, { params: { email, token } });
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, { email });
  }

  resetPassword(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/reset-password`, data);
  }

  getProfile(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/profile`);
  }

  updateProfile(data: any): Observable<AuthResponse> {
    return this.http.put<AuthResponse>(`${this.apiUrl}/profile`, data).pipe(
      tap(response => {
        this.setSession(response);
      })
    );
  }

  changePassword(data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/change-password`, data);
  }

  uploadAvatar(file: File): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ url: string }>(`${this.apiUrl}/upload-avatar`, formData);
  }

  private setSession(auth: AuthResponse): void {
    localStorage.setItem('monkorc_token', auth.token);
    localStorage.setItem('monkorc_user', JSON.stringify(auth));
    this.currentUser.set(auth);
    this.isAuthenticated.set(true);
  }

  private checkLocalStorage(): void {
    const token = localStorage.getItem('monkorc_token');
    const user = localStorage.getItem('monkorc_user');

    if (token && user) {
      this.currentUser.set(JSON.parse(user));
      this.isAuthenticated.set(true);
    }
  }
}
