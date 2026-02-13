import { Injectable, signal, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, from, of } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import {
  Auth,
  signInWithEmailAndPassword,
  signOut,
  User,
  onAuthStateChanged,
  sendPasswordResetEmail,
} from '@angular/fire/auth';

const USER_KEY = 'oui_user';
const FAILED_ATTEMPTS_KEY = 'oui_failed_attempts';
const BLOCK_UNTIL_KEY = 'oui_block_until';
const MAX_ATTEMPTS = 5;
const BLOCK_DURATION_MS = 15 * 60 * 1000; // 15 minutes

export interface UserInfo {
  id: string;
  email: string;
  displayName: string;
}

export interface LoginResponse {
  user: UserInfo;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);

  private readonly userSignal = signal<UserInfo | null>(this.getStoredUser());
  private readonly authLoadedSignal = signal<boolean>(false);

  readonly currentUser = this.userSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.userSignal());
  readonly authLoaded = this.authLoadedSignal.asReadonly();

  constructor() {
    // Listen to Firebase auth state changes
    onAuthStateChanged(this.auth, (user) => {
      if (user) {
        const userInfo: UserInfo = {
          id: user.uid,
          email: user.email ?? '',
          displayName: user.displayName ?? user.email ?? '',
        };
        this.userSignal.set(userInfo);
        localStorage.setItem(USER_KEY, JSON.stringify(userInfo));
      } else {
        this.userSignal.set(null);
        localStorage.removeItem(USER_KEY);
      }
      this.authLoadedSignal.set(true);
    });
  }

  login(email: string, password: string): Observable<LoginResponse | { error: string }> {
    // Check if user is blocked
    const blockUntil = localStorage.getItem(BLOCK_UNTIL_KEY);
    if (blockUntil) {
      const blockTime = parseInt(blockUntil, 10);
      if (Date.now() < blockTime) {
        const minutesLeft = Math.ceil((blockTime - Date.now()) / 60000);
        return of({
          error: `Demasiadas tentativas falhadas. Tente novamente em ${minutesLeft} minuto${minutesLeft > 1 ? 's' : ''}.`,
        });
      } else {
        // Block period expired, clear it
        this.clearFailedAttempts();
      }
    }

    return from(signInWithEmailAndPassword(this.auth, email, password)).pipe(
      map((userCredential) => {
        this.clearFailedAttempts();
        const user = userCredential.user;
        const userInfo: UserInfo = {
          id: user.uid,
          email: user.email ?? '',
          displayName: user.displayName ?? user.email ?? '',
        };
        this.userSignal.set(userInfo);
        localStorage.setItem(USER_KEY, JSON.stringify(userInfo));
        return { user: userInfo } as LoginResponse;
      }),
      catchError((error) => {
        this.incrementFailedAttempts();
        let message = 'Erro ao iniciar sessão. Tente novamente.';

        if (error.code === 'auth/invalid-credential' || error.code === 'auth/wrong-password') {
          message = 'Email ou palavra-passe incorretos.';
        } else if (error.code === 'auth/user-not-found') {
          message = 'Utilizador não encontrado.';
        } else if (error.code === 'auth/too-many-requests') {
          message = 'Demasiadas tentativas. Tente novamente mais tarde.';
        } else if (error.code === 'auth/invalid-email') {
          message = 'Email inválido.';
        }

        return of({ error: message });
      })
    );
  }

  logout(): Observable<void> {
    return from(signOut(this.auth)).pipe(
      tap(() => {
        this.userSignal.set(null);
        localStorage.removeItem(USER_KEY);
        this.router.navigateByUrl('/login');
      })
    );
  }

  sendPasswordResetEmail(email: string): Observable<{ success?: boolean; error?: string }> {
    return from(sendPasswordResetEmail(this.auth, email)).pipe(
      map(() => ({ success: true })),
      catchError((error) => {
        let message = 'Erro ao enviar email de recuperação.';
        if (error.code === 'auth/user-not-found') {
          message = 'Email não encontrado.';
        } else if (error.code === 'auth/invalid-email') {
          message = 'Email inválido.';
        }
        return of({ error: message });
      })
    );
  }

  private incrementFailedAttempts(): void {
    const attemptsStr = localStorage.getItem(FAILED_ATTEMPTS_KEY);
    const attempts = attemptsStr ? parseInt(attemptsStr, 10) : 0;
    const newAttempts = attempts + 1;

    if (newAttempts >= MAX_ATTEMPTS) {
      const blockUntil = Date.now() + BLOCK_DURATION_MS;
      localStorage.setItem(BLOCK_UNTIL_KEY, blockUntil.toString());
      localStorage.setItem(FAILED_ATTEMPTS_KEY, '0');
    } else {
      localStorage.setItem(FAILED_ATTEMPTS_KEY, newAttempts.toString());
    }
  }

  private clearFailedAttempts(): void {
    localStorage.removeItem(FAILED_ATTEMPTS_KEY);
    localStorage.removeItem(BLOCK_UNTIL_KEY);
  }

  async getIdToken(): Promise<string | null> {
    // Wait for Firebase Auth to finish restoring the session on page refresh
    await this.auth.authStateReady();
    const user = this.auth.currentUser;
    if (!user) return null;
    try {
      return await user.getIdToken();
    } catch {
      return null;
    }
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
