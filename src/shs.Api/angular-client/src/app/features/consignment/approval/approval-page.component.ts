import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ApprovalService, ApprovalDetails } from './approval.service';

@Component({
  selector: 'oui-approval-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="approval-container">
      <!-- Header -->
      <div class="approval-header">
        <h1 class="logo">OUI System</h1>
        <p class="subtitle">Aprovação de Avaliação de Peças</p>
      </div>

      @if (loading()) {
        <div class="state-card">
          <div class="spinner"></div>
          <p>A carregar detalhes...</p>
        </div>
      } @else if (error()) {
        <div class="state-card state-error">
          <div class="state-icon">!</div>
          <h2>{{ errorTitle() }}</h2>
          <p>{{ error() }}</p>
        </div>
      } @else if (success()) {
        <div class="state-card state-success">
          <div class="state-icon state-icon-success">✓</div>
          <h2>{{ successTitle() }}</h2>
          <p>{{ success() }}</p>
        </div>
      } @else if (details()) {
        <!-- Supplier greeting -->
        <div class="card greeting-card">
          <p>Olá <b>{{ details()!.supplierName }}</b>,</p>
          <p>
            A avaliação das peças entregues no dia <b>{{ details()!.receptionDate | date: 'dd/MM/yyyy' }}</b>
            (Ref: <b>{{ details()!.receptionRef }}</b>) foi concluída.
            Por favor reveja os preços abaixo e confirme a sua aprovação.
          </p>
        </div>

        <!-- Summary -->
        <div class="summary-row">
          <div class="summary-card">
            <div class="summary-value">{{ details()!.items.length }}</div>
            <div class="summary-label">Peças</div>
          </div>
          <div class="summary-card summary-highlight">
            <div class="summary-value">{{ details()!.totalValue | currency: 'EUR' }}</div>
            <div class="summary-label">Valor Total</div>
          </div>
        </div>

        <!-- Items table -->
        <div class="card table-card">
          <div class="card-title-bar">
            <span class="card-title">Peças Avaliadas</span>
          </div>
          <div class="table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>#</th>
                  <th>ID</th>
                  <th>Nome</th>
                  <th>Marca</th>
                  <th>Tam.</th>
                  <th>Cor</th>
                  <th>Condição</th>
                  <th>Preço</th>
                </tr>
              </thead>
              <tbody>
                @for (item of details()!.items; track item.identificationNumber; let idx = $index) {
                  <tr>
                    <td class="cell-num">{{ idx + 1 }}</td>
                    <td class="cell-mono">{{ item.identificationNumber }}</td>
                    <td><b>{{ item.name }}</b></td>
                    <td>{{ item.brand }}</td>
                    <td>{{ item.size }}</td>
                    <td>{{ item.color }}</td>
                    <td>{{ getConditionLabel(item.condition) }}</td>
                    <td class="cell-price">{{ item.evaluatedPrice | currency: 'EUR' }}</td>
                  </tr>
                }
              </tbody>
              <tfoot>
                <tr>
                  <td colspan="7" class="foot-label">Total</td>
                  <td class="cell-price foot-value">{{ details()!.totalValue | currency: 'EUR' }}</td>
                </tr>
              </tfoot>
            </table>
          </div>
        </div>

        <!-- Action buttons -->
        <div class="action-section">
          @if (!showRejectForm()) {
            <button class="btn btn-approve" (click)="approve()" [disabled]="submitting()">
              {{ submitting() ? 'A processar...' : 'Aprovar Preços' }}
            </button>
            <button class="btn btn-reject-outline" (click)="showRejectForm.set(true)" [disabled]="submitting()">
              Não Concordo
            </button>
          } @else {
            <div class="reject-form">
              <label for="rejectMessage">Motivo (opcional):</label>
              <textarea id="rejectMessage" [(ngModel)]="rejectMessage" rows="3"
                placeholder="Indique o motivo da recusa ou sugestões de preço..."></textarea>
              <div class="reject-actions">
                <button class="btn btn-reject" (click)="reject()" [disabled]="submitting()">
                  {{ submitting() ? 'A processar...' : 'Enviar Recusa' }}
                </button>
                <button class="btn btn-outline" (click)="showRejectForm.set(false)" [disabled]="submitting()">
                  Cancelar
                </button>
              </div>
            </div>
          }
        </div>

        <!-- Expiry note -->
        <p class="expiry-note">
          Este link é válido até {{ details()!.expiresAt | date: 'dd/MM/yyyy HH:mm' }}.
        </p>
      }
    </div>
  `,
  styles: [`
    :host { display: block; }

    .approval-container {
      max-width: 720px;
      margin: 0 auto;
      padding: 32px 16px;
    }

    .approval-header {
      text-align: center;
      margin-bottom: 32px;
    }

    .logo {
      font-size: 24px;
      font-weight: 800;
      color: #6366f1;
      margin: 0;
    }

    .subtitle {
      font-size: 14px;
      color: #64748b;
      margin: 4px 0 0;
    }

    /* Cards */
    .card {
      background: #ffffff;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      padding: 20px;
      margin-bottom: 16px;
    }

    .table-card { padding: 0; }

    .card-title-bar {
      padding: 14px 20px;
      border-bottom: 1px solid #e2e8f0;
    }

    .card-title {
      font-size: 13px;
      font-weight: 600;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin: 0;
    }

    .greeting-card p {
      font-size: 14px;
      color: #475569;
      line-height: 1.6;
      margin: 0 0 8px;
    }

    .greeting-card p:last-child { margin-bottom: 0; }

    /* Summary */
    .summary-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
      margin-bottom: 16px;
    }

    .summary-card {
      background: #f8fafc;
      border: 1px solid #e2e8f0;
      border-radius: 12px;
      padding: 20px;
      text-align: center;
    }

    .summary-highlight {
      background: #eef2ff;
      border-color: #c7d2fe;
    }

    .summary-value {
      font-size: 28px;
      font-weight: 800;
      color: #1e293b;
    }

    .summary-highlight .summary-value { color: #6366f1; }

    .summary-label {
      font-size: 12px;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      font-weight: 600;
      margin-top: 4px;
    }

    /* Table */
    .table-wrapper { overflow-x: auto; }

    table { width: 100%; border-collapse: collapse; font-size: 13px; }

    th {
      background: #f8fafc;
      padding: 10px 14px;
      text-align: left;
      font-weight: 600;
      font-size: 11px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      color: #64748b;
      border-bottom: 1px solid #e2e8f0;
    }

    td {
      padding: 10px 14px;
      border-bottom: 1px solid #e2e8f0;
      vertical-align: middle;
    }

    tr:hover td { background: #f8fafc; }
    .cell-num { width: 40px; color: #94a3b8; text-align: center; }
    .cell-mono { font-family: monospace; font-size: 12px; color: #64748b; }
    .cell-price { font-weight: 600; white-space: nowrap; }

    tfoot td {
      background: #f8fafc;
      font-weight: 700;
      border-top: 2px solid #e2e8f0;
      border-bottom: none;
    }

    .foot-label { text-align: right; color: #475569; }
    .foot-value { color: #6366f1; }

    /* Actions */
    .action-section {
      display: flex;
      gap: 12px;
      justify-content: center;
      margin: 24px 0;
    }

    .btn {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 12px 28px;
      border-radius: 8px;
      font-size: 14px;
      font-weight: 600;
      cursor: pointer;
      border: 1px solid transparent;
      transition: all 0.15s;
    }

    .btn:disabled { opacity: 0.5; cursor: not-allowed; }

    .btn-approve {
      background: #16a34a;
      color: white;
      font-size: 16px;
      padding: 14px 40px;
    }

    .btn-approve:hover:not(:disabled) { background: #15803d; }

    .btn-reject-outline {
      background: white;
      color: #dc2626;
      border-color: #fecaca;
    }

    .btn-reject-outline:hover:not(:disabled) { background: #fef2f2; }

    .btn-reject {
      background: #dc2626;
      color: white;
    }

    .btn-reject:hover:not(:disabled) { background: #b91c1c; }

    .btn-outline {
      background: white;
      color: #1e293b;
      border-color: #e2e8f0;
    }

    .btn-outline:hover:not(:disabled) { background: #f8fafc; }

    /* Reject form */
    .reject-form {
      width: 100%;
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .reject-form label {
      font-size: 13px;
      font-weight: 600;
      color: #475569;
    }

    .reject-form textarea {
      width: 100%;
      padding: 10px 14px;
      border: 1px solid #e2e8f0;
      border-radius: 8px;
      font-size: 14px;
      font-family: inherit;
      resize: vertical;
    }

    .reject-form textarea:focus {
      outline: none;
      border-color: #6366f1;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
    }

    .reject-actions {
      display: flex;
      gap: 8px;
    }

    /* State cards */
    .state-card {
      text-align: center;
      padding: 48px 24px;
      background: white;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
    }

    .state-card p { font-size: 14px; color: #64748b; margin: 8px 0 0; }
    .state-card h2 { font-size: 20px; color: #1e293b; margin: 12px 0 4px; }

    .state-error { border-color: #fecaca; background: #fef2f2; }
    .state-error h2 { color: #991b1b; }
    .state-error p { color: #b91c1c; }

    .state-success { border-color: #bbf7d0; background: #f0fdf4; }
    .state-success h2 { color: #166534; }
    .state-success p { color: #15803d; }

    .state-icon {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 56px;
      height: 56px;
      border-radius: 50%;
      background: #fee2e2;
      color: #dc2626;
      font-size: 24px;
      font-weight: 700;
    }

    .state-icon-success {
      background: #dcfce7;
      color: #16a34a;
    }

    .spinner {
      width: 32px;
      height: 32px;
      border: 3px solid #e2e8f0;
      border-top-color: #6366f1;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
      margin: 0 auto 12px;
    }

    @keyframes spin { to { transform: rotate(360deg); } }

    .expiry-note {
      text-align: center;
      font-size: 12px;
      color: #94a3b8;
      margin: 0;
    }

    @media (max-width: 640px) {
      .summary-row { grid-template-columns: 1fr; }
      .action-section { flex-direction: column; align-items: stretch; }
      .btn-approve { text-align: center; justify-content: center; }
    }
  `]
})
export class ApprovalPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly approvalService = inject(ApprovalService);

  details = signal<ApprovalDetails | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  errorTitle = signal('Erro');
  success = signal<string | null>(null);
  successTitle = signal('Sucesso');
  submitting = signal(false);
  showRejectForm = signal(false);
  rejectMessage = '';

  ngOnInit(): void {
    const token = this.route.snapshot.paramMap.get('token');
    if (!token) {
      this.error.set('Link de aprovação inválido.');
      this.loading.set(false);
      return;
    }
    this.loadDetails(token);
  }

  private loadDetails(token: string): void {
    this.approvalService.getApprovalDetails(token).subscribe({
      next: (details) => {
        this.details.set(details);
        this.loading.set(false);
      },
      error: (err) => {
        this.errorTitle.set('Link Inválido');
        this.error.set(err.error?.error || 'Não foi possível carregar os detalhes da aprovação.');
        this.loading.set(false);
      }
    });
  }

  approve(): void {
    const token = this.route.snapshot.paramMap.get('token')!;
    this.submitting.set(true);
    this.approvalService.approve(token).subscribe({
      next: (result) => {
        this.successTitle.set('Aprovação Registada');
        this.success.set(result.message);
        this.details.set(null);
        this.submitting.set(false);
      },
      error: (err) => {
        this.errorTitle.set('Erro na Aprovação');
        this.error.set(err.error?.error || 'Erro ao processar a aprovação.');
        this.details.set(null);
        this.submitting.set(false);
      }
    });
  }

  reject(): void {
    const token = this.route.snapshot.paramMap.get('token')!;
    this.submitting.set(true);
    this.approvalService.reject(token, this.rejectMessage || undefined).subscribe({
      next: (result) => {
        this.successTitle.set('Resposta Registada');
        this.success.set(result.message);
        this.details.set(null);
        this.submitting.set(false);
      },
      error: (err) => {
        this.errorTitle.set('Erro');
        this.error.set(err.error?.error || 'Erro ao processar a recusa.');
        this.details.set(null);
        this.submitting.set(false);
      }
    });
  }

  getConditionLabel(condition: string): string {
    const labels: Record<string, string> = {
      Excellent: 'Excelente',
      VeryGood: 'Muito Bom',
      Good: 'Bom',
      Fair: 'Razoável',
      Poor: 'Mau',
    };
    return labels[condition] || condition;
  }
}
