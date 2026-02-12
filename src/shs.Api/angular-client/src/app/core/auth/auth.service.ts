import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, of } from 'rxjs';

const API_BASE = 'http://localhost:5018';
const TOKEN_KEY = 'oui_token';
const USER_KEY = 'oui_user';

export interface UserInfo {
  id: string;
  email: string;
  displayName: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  user: UserInfo;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenSignal = signal<string | null>(this.getStoredToken());
  private readonly userSignal = signal<UserInfo | null>(this.getStoredUser());

  readonly token = this.tokenSignal.asReadonly();
  readonly currentUser = this.userSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.tokenSignal());

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router
  ) {}

  login(email: string, password: string): Observable<LoginResponse | { error: string }> {
    return this.http.post<LoginResponse>(`${API_BASE}/api/auth/login`, { email, password }).pipe(
      tap((res) => {
        this.tokenSignal.set(res.token);
        this.userSignal.set(res.user);
        localStorage.setItem(TOKEN_KEY, res.token);
        localStorage.setItem(USER_KEY, JSON.stringify(res.user));
      }),
      catchError((err) => {
        const message =
          err.error?.error ?? err.status === 401
            ? 'Email ou palavra-passe incorretos.'
            : 'Erro ao iniciar sessão. Tente novamente.';
        return of({ error: message } as { error: string });
      })
    );
  }

  logout(): void {
    this.tokenSignal.set(null);
    this.userSignal.set(null);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.router.navigateByUrl('/login');
  }

  getStoredToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  private getStoredUser(): UserInfo | null {
    try {
      const raw = localStorage.getItem(USER_KEY);
      return raw ? (JSON.parse(raw) as UserInfo) : null;
    } catch {
      return null;
    }
  }
}
