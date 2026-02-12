import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'oui-login-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="login-wrapper">
      <div class="login-card">
        <h1 class="login-title">OUI System</h1>
        <p class="login-subtitle">Inicie sessão para aceder ao sistema.</p>

        @if (errorMessage) {
          <p class="error-message">{{ errorMessage }}</p>
        }

        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="login-form">
          <label class="field">
            <span>Email</span>
            <input type="email" formControlName="email" autocomplete="email" />
          </label>

          <label class="field">
            <span>Palavra-passe</span>
            <input type="password" formControlName="password" autocomplete="current-password" />
          </label>

          <button type="submit" class="btn-primary" [disabled]="loading">
            {{ loading ? 'A iniciar sessão...' : 'Entrar' }}
          </button>
        </form>
      </div>
    </div>
  `,
  styles: [
    `
      .login-wrapper {
        min-height: 100vh;
        display: flex;
        align-items: center;
        justify-content: center;
        background: #0f172a;
      }

      .login-card {
        width: 360px;
        padding: 2rem;
        border-radius: 1rem;
        background: #0b1120;
        color: #e5e7eb;
        box-shadow: 0 20px 45px rgba(15, 23, 42, 0.8);
        border: 1px solid rgba(148, 163, 184, 0.3);
      }

      .login-title {
        margin: 0 0 0.25rem;
        font-size: 1.75rem;
        font-weight: 700;
      }

      .login-subtitle {
        margin: 0 0 1.5rem;
        font-size: 0.9rem;
        color: #9ca3af;
      }

      .error-message {
        margin: 0 0 1rem;
        padding: 0.5rem 0.75rem;
        border-radius: 0.5rem;
        background: rgba(239, 68, 68, 0.2);
        color: #fca5a5;
        font-size: 0.9rem;
      }

      .login-form {
        display: flex;
        flex-direction: column;
        gap: 1rem;
      }

      .field {
        display: flex;
        flex-direction: column;
        gap: 0.25rem;
        font-size: 0.85rem;
      }

      .field span {
        color: #d1d5db;
      }

      .field input {
        padding: 0.6rem 0.75rem;
        border-radius: 0.5rem;
        border: 1px solid #4b5563;
        background: #020617;
        color: #e5e7eb;
        font-size: 0.9rem;
      }

      .field input:focus {
        outline: none;
        border-color: #6366f1;
        box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.5);
      }

      .btn-primary {
        margin-top: 0.5rem;
        padding: 0.65rem 0.75rem;
        border-radius: 0.5rem;
        border: none;
        font-weight: 600;
        font-size: 0.95rem;
        background: linear-gradient(90deg, #6366f1, #8b5cf6);
        color: #f9fafb;
        cursor: pointer;
      }

      .btn-primary:hover:not(:disabled) {
        filter: brightness(1.1);
      }

      .btn-primary:disabled {
        opacity: 0.7;
        cursor: not-allowed;
      }
    `,
  ],
})
export class LoginPageComponent {
  form: FormGroup;
  loading = false;
  errorMessage = '';

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: AuthService,
    private readonly router: Router
  ) {
    this.form = this.fb.nonNullable.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });
    if (this.auth.isAuthenticated()) {
      this.router.navigateByUrl('/');
    }
  }

  onSubmit(): void {
    this.errorMessage = '';
    if (this.form.invalid || this.loading) return;

    this.loading = true;
    const { email, password } = this.form.getRawValue();

    this.auth.login(email, password).subscribe({
      next: (res) => {
        this.loading = false;
        if ('error' in res) {
          this.errorMessage = res.error;
          return;
        }
        this.router.navigateByUrl('/');
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'Erro de ligação. Verifique se a API está a correr.';
      },
    });
  }
}
