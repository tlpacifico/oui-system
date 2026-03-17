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
          <p class="login-tagline">Moda Circular · Portugal</p>
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
        background: linear-gradient(145deg, #1C1917 0%, #292524 50%, #1C1917 100%);
        position: relative;
      }

      .login-wrapper::before {
        content: '';
        position: absolute;
        inset: 0;
        background: url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='0.02'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E");
        pointer-events: none;
      }

      .login-card {
        width: 420px;
        padding: 2.5rem;
        border-radius: 16px;
        background: #292524;
        color: #E7E5E4;
        box-shadow: 0 25px 60px rgba(0, 0, 0, 0.5);
        border: 1px solid rgba(255, 255, 255, 0.06);
        position: relative;
        z-index: 1;
      }

      .logo-section {
        text-align: center;
        margin-bottom: 2rem;
      }

      .login-title {
        margin: 0;
        font-family: 'DM Serif Display', Georgia, serif;
        font-size: 2.2rem;
        font-weight: 400;
        color: #FAF9F7;
      }

      .login-tagline {
        margin: 6px 0 0;
        font-size: 0.75rem;
        letter-spacing: 2.5px;
        text-transform: uppercase;
        color: #78716C;
        font-weight: 500;
      }

      .error-message {
        margin: 0 0 1rem;
        padding: 0.75rem;
        border-radius: 10px;
        background: rgba(196, 91, 91, 0.15);
        color: #F0A8A8;
        font-size: 0.9rem;
        border: 1px solid rgba(196, 91, 91, 0.25);
      }

      .success-message {
        margin: 0 0 1rem;
        padding: 0.75rem;
        border-radius: 10px;
        background: rgba(91, 113, 83, 0.2);
        color: #A8C9A0;
        font-size: 0.9rem;
        border: 1px solid rgba(91, 113, 83, 0.3);
      }

      .login-form {
        display: flex;
        flex-direction: column;
        gap: 1rem;
      }

      .forgot-password-text {
        margin: 0 0 1rem;
        font-size: 0.9rem;
        color: #A8A29E;
        line-height: 1.5;
      }

      .field {
        display: flex;
        flex-direction: column;
        gap: 0.4rem;
        font-size: 0.9rem;
      }

      .field span {
        color: #D6D3D1;
        font-weight: 500;
      }

      .field input {
        padding: 0.7rem 0.9rem;
        border-radius: 10px;
        border: 1px solid #44403C;
        background: #1C1917;
        color: #E7E5E4;
        font-size: 0.95rem;
        font-family: 'DM Sans', sans-serif;
        transition: all 0.2s;
      }

      .field input:focus {
        outline: none;
        border-color: #5B7153;
        box-shadow: 0 0 0 3px rgba(91, 113, 83, 0.25);
      }

      .btn-primary {
        margin-top: 0.5rem;
        padding: 0.75rem;
        border-radius: 10px;
        border: none;
        font-weight: 600;
        font-size: 1rem;
        background: linear-gradient(135deg, #5B7153, #4A5E43);
        color: #FAF9F7;
        cursor: pointer;
        transition: all 0.2s;
        font-family: 'DM Sans', sans-serif;
      }

      .btn-primary:hover:not(:disabled) {
        filter: brightness(1.1);
        transform: translateY(-1px);
        box-shadow: 0 4px 12px rgba(91, 113, 83, 0.3);
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
        color: #C4956A;
        font-size: 0.9rem;
        cursor: pointer;
        text-decoration: underline;
        text-underline-offset: 2px;
        transition: color 0.2s;
        font-family: 'DM Sans', sans-serif;
      }

      .btn-link:hover {
        color: #D4A87A;
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
        // Load user roles and permissions
        //this.auth.loadUserAuthContext().subscribe({
          //next: () => {
            this.router.navigateByUrl('/dashboard');
          //},
          //error: (err) => {
            //console.error('Failed to load user permissions:', err);
            // Still navigate, but user might have limited access

          //}
        //});
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
