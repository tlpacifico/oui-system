import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { PosService, RegisterResponse, CloseRegisterResponse } from './pos.service';

@Component({
  selector: 'oui-pos-register-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page">
      @if (loading()) {
        <div class="loading">A verificar caixa...</div>
      } @else if (!registerOpen()) {
        <!-- No register open: show open form -->
        <div class="center-card">
          <div class="card open-card">
            <div class="card-icon">💰</div>
            <h1>Abrir Caixa</h1>
            <p class="card-subtitle">Insira o valor inicial para abrir a caixa registadora.</p>

            <div class="form-group">
              <label>Valor de Abertura (€)</label>
              <input
                type="number"
                class="form-control form-control-lg"
                [(ngModel)]="openingAmount"
                min="0"
                step="0.01"
                placeholder="0.00"
                (keydown.enter)="openRegister()"
              />
            </div>

            @if (error()) {
              <div class="alert alert-error">{{ error() }}</div>
            }

            <button
              class="btn btn-primary btn-lg"
              (click)="openRegister()"
              [disabled]="submitting()"
            >
              {{ submitting() ? 'A abrir...' : 'Abrir Caixa' }}
            </button>
          </div>
        </div>
      } @else if (closeResult()) {
        <!-- Register just closed: show summary -->
        <div class="center-card">
          <div class="card close-summary">
            <div class="success-icon">&#10003;</div>
            <h1>Caixa Fechada</h1>
            <p class="card-subtitle">Caixa #{{ closeResult()!.registerNumber }} — {{ closeResult()!.operatorName }}</p>

            <div class="summary-grid">
              <div class="summary-item">
                <span class="summary-label">Vendas</span>
                <span class="summary-value">{{ closeResult()!.salesCount }}</span>
              </div>
              <div class="summary-item">
                <span class="summary-label">Faturação</span>
                <span class="summary-value">{{ closeResult()!.totalRevenue | currency: 'EUR' }}</span>
              </div>
              <div class="summary-item">
                <span class="summary-label">Esperado (Dinheiro)</span>
                <span class="summary-value">{{ closeResult()!.expectedCash | currency: 'EUR' }}</span>
              </div>
              <div class="summary-item">
                <span class="summary-label">Contado</span>
                <span class="summary-value">{{ closeResult()!.countedCash | currency: 'EUR' }}</span>
              </div>
              <div class="summary-item full-width" [class.discrepancy-ok]="closeResult()!.discrepancy === 0" [class.discrepancy-bad]="closeResult()!.discrepancy !== 0">
                <span class="summary-label">Discrepância</span>
                <span class="summary-value">{{ closeResult()!.discrepancy | currency: 'EUR' }}</span>
              </div>
            </div>

            @if (closeResult()!.salesByPaymentMethod && objectKeys(closeResult()!.salesByPaymentMethod).length > 0) {
              <div class="method-breakdown">
                <h3>Por Método de Pagamento</h3>
                @for (key of objectKeys(closeResult()!.salesByPaymentMethod); track key) {
                  <div class="method-row">
                    <span>{{ getPaymentLabel(key) }}</span>
                    <span class="method-value">{{ closeResult()!.salesByPaymentMethod[key] | currency: 'EUR' }}</span>
                  </div>
                }
              </div>
            }

            <div class="close-actions">
              <button class="btn btn-primary" (click)="resetToOpen()">Abrir Nova Caixa</button>
              <a class="btn btn-outline" routerLink="/pos/sales">Ver Vendas</a>
            </div>
          </div>
        </div>
      } @else {
        <!-- Register is open: show info and actions -->
        <div class="page-header">
          <div>
            <h1>Caixa #{{ register()!.registerNumber }}</h1>
            <p class="subtitle">{{ register()!.operatorName }} · Aberta {{ register()!.openedAt | date: 'dd/MM/yyyy HH:mm' }}</p>
          </div>
          <div class="header-actions">
            <a class="btn btn-primary btn-lg" routerLink="/pos/sale">Iniciar Venda</a>
          </div>
        </div>

        <!-- KPIs -->
        <div class="stat-grid">
          <div class="stat-card">
            <div class="stat-value">{{ register()!.salesCount }}</div>
            <div class="stat-label">Vendas</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ register()!.salesTotal | currency: 'EUR' }}</div>
            <div class="stat-label">Faturação</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ register()!.openingAmount | currency: 'EUR' }}</div>
            <div class="stat-label">Abertura</div>
          </div>
          <div class="stat-card stat-open">
            <div class="stat-value dot-pulse"></div>
            <div class="stat-label">Aberta</div>
          </div>
        </div>

        <!-- Quick actions -->
        <div class="actions-row">
          <a class="action-card" routerLink="/pos/sale">
            <span class="action-icon">🛒</span>
            <span class="action-label">Nova Venda</span>
          </a>
          <a class="action-card" routerLink="/pos/sales">
            <span class="action-icon">📋</span>
            <span class="action-label">Vendas de Hoje</span>
          </a>
          <button class="action-card action-close" (click)="showCloseDialog.set(true)">
            <span class="action-icon">🔒</span>
            <span class="action-label">Fechar Caixa</span>
          </button>
        </div>

        <!-- Close dialog -->
        @if (showCloseDialog()) {
          <div class="overlay" (click)="showCloseDialog.set(false)">
            <div class="dialog" (click)="$event.stopPropagation()">
              <h2>Fechar Caixa</h2>
              <p class="dialog-subtitle">Conte o dinheiro na caixa e insira o valor abaixo.</p>

              <div class="form-group">
                <label>Valor Contado (€)</label>
                <input
                  type="number"
                  class="form-control form-control-lg"
                  [(ngModel)]="closingAmount"
                  min="0"
                  step="0.01"
                  placeholder="0.00"
                  (keydown.enter)="closeRegister()"
                />
              </div>

              <div class="form-group">
                <label>Notas (opcional)</label>
                <textarea
                  class="form-control"
                  rows="2"
                  [(ngModel)]="closingNotes"
                  placeholder="Observações sobre o fecho..."
                ></textarea>
              </div>

              @if (error()) {
                <div class="alert alert-error">{{ error() }}</div>
              }

              <div class="dialog-actions">
                <button class="btn btn-outline" (click)="showCloseDialog.set(false)">Cancelar</button>
                <button
                  class="btn btn-danger"
                  (click)="closeRegister()"
                  [disabled]="submitting()"
                >
                  {{ submitting() ? 'A fechar...' : 'Confirmar Fecho' }}
                </button>
              </div>
            </div>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .page { max-width: 900px; margin: 0 auto; }
    .loading { text-align: center; padding: 80px; color: #64748b; font-size: 16px; }

    .center-card { display: flex; justify-content: center; align-items: center; min-height: 60vh; }
    .card { background: #fff; border: 1px solid #e2e8f0; border-radius: 16px; padding: 40px; }
    .open-card { max-width: 420px; width: 100%; text-align: center; }
    .card-icon { font-size: 48px; margin-bottom: 12px; }
    .open-card h1 { font-size: 24px; margin: 0 0 8px; }
    .card-subtitle { font-size: 14px; color: #64748b; margin: 0 0 28px; }

    .form-group { margin-bottom: 16px; text-align: left; }
    .form-group label { display: block; font-size: 13px; font-weight: 600; color: #374151; margin-bottom: 6px; }
    .form-control { width: 100%; padding: 10px 12px; border: 1px solid #d1d5db; border-radius: 8px; font-size: 14px; font-family: inherit; box-sizing: border-box; }
    .form-control-lg { padding: 14px 16px; font-size: 20px; text-align: center; }
    .form-control:focus { outline: none; border-color: #6366f1; box-shadow: 0 0 0 3px rgba(99,102,241,0.1); }

    .alert { padding: 12px 16px; border-radius: 8px; font-size: 13px; margin-bottom: 16px; }
    .alert-error { background: #fef2f2; color: #dc2626; border: 1px solid #fecaca; }

    .btn { display: inline-flex; align-items: center; justify-content: center; gap: 6px; padding: 8px 16px; border: 1px solid #d1d5db; border-radius: 8px; font-size: 13px; font-weight: 600; cursor: pointer; background: #fff; color: #374151; text-decoration: none; transition: all 0.15s; }
    .btn:hover { background: #f8fafc; }
    .btn:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-primary { background: #6366f1; color: #fff; border-color: #6366f1; }
    .btn-primary:hover { background: #4f46e5; }
    .btn-lg { padding: 14px 28px; font-size: 15px; width: 100%; }
    .btn-outline { background: transparent; }
    .btn-danger { background: #ef4444; color: #fff; border-color: #ef4444; }
    .btn-danger:hover { background: #dc2626; }

    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
    .page-header h1 { font-size: 24px; font-weight: 700; margin: 0; }
    .subtitle { font-size: 13px; color: #64748b; margin: 4px 0 0; }

    .stat-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin-bottom: 24px; }
    .stat-card { background: #fff; border: 1px solid #e2e8f0; border-radius: 12px; padding: 24px; text-align: center; }
    .stat-value { font-size: 28px; font-weight: 700; color: #1e293b; }
    .stat-label { font-size: 12px; color: #64748b; margin-top: 4px; }
    .stat-open { border-color: #22c55e; background: #f0fdf4; }
    .dot-pulse { width: 12px; height: 12px; background: #22c55e; border-radius: 50%; display: inline-block; animation: pulse 2s infinite; }
    @keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.4; } }

    .actions-row { display: grid; grid-template-columns: repeat(3, 1fr); gap: 16px; }
    .action-card { display: flex; flex-direction: column; align-items: center; gap: 12px; padding: 32px 16px; background: #fff; border: 1px solid #e2e8f0; border-radius: 12px; text-decoration: none; color: #1e293b; cursor: pointer; transition: all 0.2s; font-family: inherit; font-size: inherit; }
    .action-card:hover { border-color: #6366f1; background: #eef2ff; transform: translateY(-2px); box-shadow: 0 4px 12px rgba(0,0,0,0.05); }
    .action-icon { font-size: 32px; }
    .action-label { font-size: 14px; font-weight: 600; }
    .action-close:hover { border-color: #ef4444; background: #fef2f2; }

    .overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; }
    .dialog { background: #fff; border-radius: 16px; padding: 32px; max-width: 440px; width: 90%; }
    .dialog h2 { font-size: 20px; margin: 0 0 8px; }
    .dialog-subtitle { font-size: 13px; color: #64748b; margin: 0 0 24px; }
    .dialog-actions { display: flex; gap: 12px; margin-top: 20px; }
    .dialog-actions .btn { flex: 1; }

    .close-summary { max-width: 500px; width: 100%; text-align: center; }
    .success-icon { width: 56px; height: 56px; border-radius: 50%; background: #f0fdf4; color: #22c55e; font-size: 28px; display: flex; align-items: center; justify-content: center; margin: 0 auto 16px; border: 2px solid #bbf7d0; }
    .close-summary h1 { font-size: 22px; margin: 0 0 4px; }

    .summary-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin: 24px 0; text-align: left; }
    .summary-item { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 12px 16px; }
    .summary-item.full-width { grid-column: 1 / -1; text-align: center; }
    .summary-label { display: block; font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; color: #64748b; margin-bottom: 4px; }
    .summary-value { font-size: 18px; font-weight: 700; color: #1e293b; }
    .discrepancy-ok { background: #f0fdf4; border-color: #bbf7d0; }
    .discrepancy-ok .summary-value { color: #16a34a; }
    .discrepancy-bad { background: #fef2f2; border-color: #fecaca; }
    .discrepancy-bad .summary-value { color: #dc2626; }

    .method-breakdown { text-align: left; margin-bottom: 24px; }
    .method-breakdown h3 { font-size: 13px; font-weight: 600; margin: 0 0 8px; color: #374151; }
    .method-row { display: flex; justify-content: space-between; padding: 8px 12px; border-bottom: 1px solid #f1f5f9; font-size: 13px; }
    .method-value { font-weight: 600; }

    .close-actions { display: flex; gap: 12px; }
    .close-actions .btn { flex: 1; }

    @media (max-width: 768px) {
      .stat-grid { grid-template-columns: repeat(2, 1fr); }
      .actions-row { grid-template-columns: 1fr; }
    }
  `]
})
export class PosRegisterPageComponent implements OnInit {
  private readonly posService = inject(PosService);
  private readonly router = inject(Router);

  loading = signal(true);
  registerOpen = signal(false);
  register = signal<RegisterResponse | null>(null);
  closeResult = signal<CloseRegisterResponse | null>(null);
  showCloseDialog = signal(false);
  submitting = signal(false);
  error = signal<string | null>(null);

  openingAmount = 0;
  closingAmount = 0;
  closingNotes = '';

  objectKeys = Object.keys;

  ngOnInit(): void {
    this.checkRegister();
  }

  private checkRegister(): void {
    this.loading.set(true);
    this.posService.getCurrentRegister().subscribe({
      next: (res) => {
        this.registerOpen.set(res.open);
        this.register.set(res.register ?? null);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }

  openRegister(): void {
    this.submitting.set(true);
    this.error.set(null);
    this.posService.openRegister(this.openingAmount).subscribe({
      next: (reg) => {
        this.submitting.set(false);
        this.register.set(reg);
        this.registerOpen.set(true);
      },
      error: (err) => {
        this.submitting.set(false);
        this.error.set(err.error?.error || 'Erro ao abrir caixa.');
      },
    });
  }

  closeRegister(): void {
    if (!this.register()) return;
    this.submitting.set(true);
    this.error.set(null);
    this.posService.closeRegister(
      this.register()!.externalId,
      this.closingAmount,
      this.closingNotes || undefined
    ).subscribe({
      next: (result) => {
        this.submitting.set(false);
        this.showCloseDialog.set(false);
        this.closeResult.set(result);
        this.registerOpen.set(false);
        this.register.set(null);
      },
      error: (err) => {
        this.submitting.set(false);
        this.error.set(err.error?.error || 'Erro ao fechar caixa.');
      },
    });
  }

  resetToOpen(): void {
    this.closeResult.set(null);
    this.openingAmount = 0;
    this.closingAmount = 0;
    this.closingNotes = '';
    this.error.set(null);
  }

  getPaymentLabel(method: string): string {
    const labels: Record<string, string> = {
      Cash: 'Dinheiro', CreditCard: 'Cartão Crédito', DebitCard: 'Cartão Débito',
      PIX: 'PIX', StoreCredit: 'Crédito Loja',
    };
    return labels[method] || method;
  }
}
