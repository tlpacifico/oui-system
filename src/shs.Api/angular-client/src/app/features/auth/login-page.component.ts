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
        <div class="logo-section">
          <h1 class="login-title">Oui Circular</h1>
        </div>

        @if (errorMessage) {
          <p class="error-message">{{ errorMessage }}</p>
        }

        @if (successMessage) {
          <p class="success-message">{{ successMessage }}</p>
        }

        @if (!showForgotPassword) {
          <form [formGroup]="form" (ngSubmit)="onSubmit()" class="login-form">
            <label class="field">
              <span>Email</span>
              <input type="email" formControlName="email" autocomplete="email" />
            </label>

            <label class="field">
              <span>Palavra-passe</span>
              <input type="password" formControlName="password" autocomplete="current-password" />
            </label>

            <button type="submit" class="btn-primary" [disabled]="loading || form.invalid">
              {{ loading ? 'A iniciar sessão...' : 'Entrar' }}
            </button>

            <button type="button" class="btn-link" (click)="toggleForgotPassword()">
              Esqueci a minha palavra-passe
            </button>
          </form>
        } @else {
          <form [formGroup]="forgotPasswordForm" (ngSubmit)="onSendResetEmail()" class="login-form">
            <p class="forgot-password-text">
              Introduza o seu email e enviaremos um link para redefinir a palavra-passe.
            </p>

            <label class="field">
              <span>Email</span>
              <input type="email" formControlName="email" autocomplete="email" />
            </label>

            <button
              type="submit"
              class="btn-primary"
              [disabled]="loadingReset || forgotPasswordForm.invalid"
            >
              {{ loadingReset ? 'A enviar...' : 'Enviar email de recuperação' }}
            </button>

            <button type="button" class="btn-link" (click)="toggleForgotPassword()">
              Voltar ao login
            </button>
          </form>
        }
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
        width: 420px;
        padding: 2.5rem;
        border-radius: 1rem;
        background: #0b1120;
        color: #e5e7eb;
        box-shadow: 0 20px 45px rgba(15, 23, 42, 0.8);
        border: 1px solid rgba(148, 163, 184, 0.3);
      }

      .logo-section {
        text-align: center;
        margin-bottom: 2rem;
      }

      .login-title {
        margin: 0;
        font-size: 2rem;
        font-weight: 700;
        background: linear-gradient(90deg, #6366f1, #8b5cf6);
        -webkit-background-clip: text;
        -webkit-text-fill-color: transparent;
        background-clip: text;
      }

      .error-message {
        margin: 0 0 1rem;
        padding: 0.75rem;
        border-radius: 0.5rem;
        background: rgba(239, 68, 68, 0.15);
        color: #fca5a5;
        font-size: 0.9rem;
        border: 1px solid rgba(239, 68, 68, 0.3);
      }

      .success-message {
        margin: 0 0 1rem;
        padding: 0.75rem;
        border-radius: 0.5rem;
        background: rgba(34, 197, 94, 0.15);
        color: #86efac;
        font-size: 0.9rem;
        border: 1px solid rgba(34, 197, 94, 0.3);
      }

      .login-form {
        display: flex;
        flex-direction: column;
        gap: 1rem;
      }

      .forgot-password-text {
        margin: 0 0 1rem;
        font-size: 0.9rem;
        color: #9ca3af;
        line-height: 1.5;
      }

      .field {
        display: flex;
        flex-direction: column;
        gap: 0.4rem;
        font-size: 0.9rem;
      }

      .field span {
        color: #d1d5db;
        font-weight: 500;
      }

      .field input {
        padding: 0.7rem 0.9rem;
        border-radius: 0.5rem;
        border: 1px solid #4b5563;
        background: #020617;
        color: #e5e7eb;
        font-size: 0.95rem;
        transition: all 0.2s;
      }

      .field input:focus {
        outline: none;
        border-color: #6366f1;
        box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.2);
      }

      .btn-primary {
        margin-top: 0.5rem;
        padding: 0.75rem;
        border-radius: 0.5rem;
        border: none;
        font-weight: 600;
        font-size: 1rem;
        background: linear-gradient(90deg, #6366f1, #8b5cf6);
        color: #f9fafb;
        cursor: pointer;
        transition: all 0.2s;
      }

      .btn-primary:hover:not(:disabled) {
        filter: brightness(1.1);
        transform: translateY(-1px);
      }

      .btn-primary:disabled {
        opacity: 0.6;
        cursor: not-allowed;
        transform: none;
      }

      .btn-link {
        margin-top: 0.5rem;
        padding: 0.5rem;
        border: none;
        background: transparent;
        color: #8b5cf6;
        font-size: 0.9rem;
        cursor: pointer;
        text-decoration: underline;
        transition: color 0.2s;
      }

      .btn-link:hover {
        color: #a78bfa;
      }
    `,
  ],
})
export class LoginPageComponent {
  form: FormGroup;
  forgotPasswordForm: FormGroup;
  loading = false;
  loadingReset = false;
  errorMessage = '';
  successMessage = '';
  showForgotPassword = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: AuthService,
    private readonly router: Router
  ) {
    this.form = this.fb.nonNullable.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });

    this.forgotPasswordForm = this.fb.nonNullable.group({
      email: ['', [Validators.required, Validators.email]],
    });

    if (this.auth.isAuthenticated()) {
      this.router.navigateByUrl('/dashboard');
    }
  }

  onSubmit(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (this.form.invalid || this.loading) return;

    this.loading = true;
    const { email, password } = this.form.getRawValue();

    this.auth.login(email, password).subscribe({
      next: (res) => {
        this.loading = false;
        if ('error' in res && res.error) {
          this.errorMessage = res.error;
          return;
        }
        this.router.navigateByUrl('/dashboard');
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'Erro de ligação ao Firebase. Verifique a sua ligação.';
      },
    });
  }

  toggleForgotPassword(): void {
    this.showForgotPassword = !this.showForgotPassword;
    this.errorMessage = '';
    this.successMessage = '';
    this.forgotPasswordForm.reset();
  }

  onSendResetEmail(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (this.forgotPasswordForm.invalid || this.loadingReset) return;

    this.loadingReset = true;
    const { email } = this.forgotPasswordForm.getRawValue();

    this.auth.sendPasswordResetEmail(email).subscribe({
      next: (res) => {
        this.loadingReset = false;
        if ('error' in res && res.error) {
          this.errorMessage = res.error;
          return;
        }
        this.successMessage =
          'Email de recuperação enviado com sucesso! Verifique a sua caixa de entrada.';
        this.forgotPasswordForm.reset();
      },
      error: () => {
        this.loadingReset = false;
        this.errorMessage = 'Erro ao enviar email. Verifique a sua ligação.';
      },
    });
  }
}
