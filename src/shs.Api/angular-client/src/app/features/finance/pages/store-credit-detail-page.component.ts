import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { FinanceService, StoreCreditDetail, StoreCreditTransaction } from '../finance.service';

@Component({
  selector: 'oui-store-credit-detail-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    @if (loading()) {
      <div class="state-message">A carregar...</div>
    } @else if (credit()) {
      <!-- Top bar: back + actions -->
      <div class="detail-topbar">
        <button class="btn btn-outline" (click)="goBack()">← Voltar</button>
        <div class="detail-topbar-right">
          @if (credit()!.status === 1 && credit()!.currentBalance > 0) {
            <button class="btn btn-outline" (click)="openAdjust()">Ajustar Saldo</button>
            <button class="btn btn-outline btn-danger-outline" (click)="confirmCancel()">Cancelar Crédito</button>
          }
        </div>
      </div>

      <!-- Credit header: supplier, status badge -->
      <div class="credit-header">
        <div class="credit-header-info">
          <span class="credit-supplier-label">{{ credit()!.supplierName }}</span>
          <h1 class="credit-title">Crédito em Loja</h1>
          <p class="credit-meta">Emitido em {{ credit()!.issuedOn | date: 'dd/MM/yyyy' }} por {{ credit()!.issuedBy }}</p>
        </div>
        <span class="badge badge-lg" [ngClass]="'badge-' + getStatusBadgeClass(credit()!.status)">
          {{ getStatusLabel(credit()!.status) }}
        </span>
      </div>

      <!-- KPI Stats -->
      <div class="stat-grid">
        <div class="card stat-card">
          <div class="stat-label">Valor Original</div>
          <div class="stat-value stat-price">{{ credit()!.originalAmount | currency: 'EUR' }}</div>
        </div>
        <div class="card stat-card">
          <div class="stat-label">Saldo Atual</div>
          <div class="stat-value" [class.stat-success]="credit()!.currentBalance > 0" [class.stat-muted]="credit()!.currentBalance === 0">{{ credit()!.currentBalance | currency: 'EUR' }}</div>
        </div>
        @if (credit()!.expiresOn) {
          <div class="card stat-card">
            <div class="stat-label">Validade</div>
            <div class="stat-value">{{ credit()!.expiresOn | date: 'dd/MM/yyyy' }}</div>
          </div>
        }
      </div>

      <!-- Two-column: Origin + Notes -->
      @if (credit()!.sourceSettlement || credit()!.notes) {
        <div class="detail-grid">
          @if (credit()!.sourceSettlement; as src) {
            <div class="card">
              <div class="card-title">Origem</div>
              <p class="card-text">Acerto {{ src.periodStart | date: 'dd/MM/yyyy' }} – {{ src.periodEnd | date: 'dd/MM/yyyy' }} ({{ src.totalSalesAmount | currency: 'EUR' }})</p>
            </div>
          }
          @if (credit()!.notes) {
            <div class="card">
              <div class="card-title">Notas</div>
              <p class="card-text">{{ credit()!.notes }}</p>
            </div>
          }
        </div>
      }

      <!-- Movements table -->
      <div class="card table-card">
        <div class="table-wrapper">
          @if (transactions().length === 0) {
            <div class="empty-state">Sem movimentos.</div>
          } @else {
            <table>
              <thead>
                <tr>
                  <th>Data</th>
                  <th class="cell-right">Valor</th>
                  <th class="cell-right">Saldo após</th>
                  <th>Tipo</th>
                  <th>Processado por</th>
                  <th>Notas</th>
                </tr>
              </thead>
              <tbody>
                @for (t of transactions(); track t.externalId) {
                  <tr>
                    <td>{{ t.transactionDate | date: 'dd/MM/yyyy HH:mm' }}</td>
                    <td class="cell-right" [class.cell-negative]="t.amount < 0">{{ t.amount | currency: 'EUR' }}</td>
                    <td class="cell-right">{{ t.balanceAfter | currency: 'EUR' }}</td>
                    <td>{{ getTransactionTypeLabel(t.transactionType) }}</td>
                    <td>{{ t.processedBy }}</td>
                    <td>{{ t.notes || '—' }}</td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </div>
      </div>
    } @else {
      <div class="state-message">Crédito não encontrado.</div>
    }

    @if (showAdjustModal()) {
      <div class="modal-backdrop" (click)="closeAdjustModal()"></div>
      <div class="modal">
        <div class="modal-header">
          <h3>Ajustar Saldo</h3>
          <button class="btn-close" (click)="closeAdjustModal()">×</button>
        </div>
        <div class="modal-body">
          <p>Saldo atual: <strong>{{ credit()!.currentBalance | currency: 'EUR' }}</strong></p>
          <div class="form-group">
            <label>Ajuste (positivo para adicionar, negativo para subtrair)</label>
            <input type="number" [(ngModel)]="adjustAmount" step="0.01" class="form-control" />
          </div>
          <div class="form-group">
            <label>Motivo</label>
            <input type="text" [(ngModel)]="adjustReason" class="form-control" placeholder="Ex: Correção de erro" />
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-outline" (click)="closeAdjustModal()">Cancelar</button>
          <button class="btn btn-primary" (click)="submitAdjust()" [disabled]="adjusting() || adjustAmount == null || adjustAmount === 0">Ajustar</button>
        </div>
      </div>
    }
  `,
  styles: [`
    :host { display: block; }

    /* ── Detail Topbar ── */
    .detail-topbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }

    .detail-topbar-right {
      display: flex;
      gap: 8px;
    }

    /* ── Credit Header ── */
    .credit-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 24px;
      gap: 20px;
    }

    .credit-header-info {
      flex: 1;
      min-width: 0;
    }

    .credit-supplier-label {
      font-size: 13px;
      color: #64748b;
      display: block;
      margin-bottom: 4px;
    }

    .credit-title {
      font-size: 24px;
      font-weight: 700;
      margin: 0 0 8px;
      color: #1e293b;
    }

    .credit-meta {
      font-size: 14px;
      color: #64748b;
      margin: 0;
      line-height: 1.5;
    }

    /* ── Stats Grid ── */
    .stat-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
      gap: 16px;
      margin-bottom: 24px;
    }

    .stat-card {
      text-align: center;
      padding: 20px;
    }

    .stat-label {
      font-size: 12px;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      font-weight: 600;
      margin-bottom: 8px;
    }

    .stat-value {
      font-size: 22px;
      font-weight: 800;
      color: #1e293b;
    }

    .stat-price { color: #6366f1; }
    .stat-success { color: #059669; }
    .stat-muted { color: #94a3b8; }

    /* ── Detail Grid ── */
    .detail-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 20px;
      margin-bottom: 24px;
    }

    /* ── Cards ── */
    .card {
      background: #ffffff;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      padding: 20px;
    }

    .card-title {
      font-size: 13px;
      font-weight: 600;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-bottom: 16px;
    }

    .card-text {
      font-size: 14px;
      color: #1e293b;
      margin: 0;
      line-height: 1.6;
    }

    .table-card {
      padding: 0;
    }

    /* ── Table ── */
    .table-wrapper {
      overflow-x: auto;
    }

    table {
      width: 100%;
      border-collapse: collapse;
      font-size: 13px;
    }

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
      padding: 12px 14px;
      border-bottom: 1px solid #e2e8f0;
      vertical-align: middle;
    }

    tr:hover td {
      background: #f1f5f9;
    }

    .cell-right { text-align: right; }
    .cell-negative { color: #dc2626; }

    /* ── Badges ── */
    .badge {
      display: inline-block;
      padding: 3px 10px;
      border-radius: 20px;
      font-size: 11px;
      font-weight: 600;
      white-space: nowrap;
    }

    .badge-lg {
      padding: 6px 16px;
      font-size: 13px;
    }

    .badge-success { background: #dcfce7; color: #166534; }
    .badge-warning { background: #fef3c7; color: #92400e; }
    .badge-danger { background: #fee2e2; color: #991b1b; }
    .badge-info { background: #dbeafe; color: #1e40af; }
    .badge-gray { background: #f1f5f9; color: #475569; }

    /* ── Buttons ── */
    .btn {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 8px 16px;
      border-radius: 8px;
      font-size: 13px;
      font-weight: 600;
      cursor: pointer;
      border: 1px solid transparent;
      transition: all 0.15s;
    }

    .btn-outline {
      background: white;
      color: #1e293b;
      border-color: #e2e8f0;
    }

    .btn-outline:hover {
      background: #f8fafc;
    }

    .btn-danger-outline {
      border-color: #fecaca;
      color: #991b1b;
    }

    .btn-danger-outline:hover {
      background: #fee2e2;
    }

    /* ── States ── */
    .state-message {
      text-align: center;
      padding: 4rem 2rem;
      color: #64748b;
      font-size: 15px;
      background: white;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
    }

    .empty-state {
      padding: 24px;
      text-align: center;
      color: #64748b;
      font-size: 14px;
    }

    /* ── Modal ── */
    .modal-backdrop { position: fixed; inset: 0; background: rgba(0,0,0,0.4); z-index: 100; }
    .modal { position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%); background: #fff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0,0,0,0.15); z-index: 101; min-width: 400px; max-width: 90vw; }
    .modal-header { display: flex; justify-content: space-between; align-items: center; padding: 16px 20px; border-bottom: 1px solid #e2e8f0; }
    .modal-header h3 { margin: 0; font-size: 18px; }
    .btn-close { background: none; border: none; font-size: 24px; cursor: pointer; color: #64748b; line-height: 1; }
    .modal-body { padding: 20px; }
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; font-weight: 500; margin-bottom: 6px; font-size: 14px; }
    .form-control { width: 100%; padding: 10px 12px; border: 1px solid #e2e8f0; border-radius: 8px; font-size: 14px; }
    .modal-footer { display: flex; justify-content: flex-end; gap: 12px; padding: 16px 20px; border-top: 1px solid #e2e8f0; }

    /* ── Responsive ── */
    @media (max-width: 1024px) {
      .stat-grid {
        grid-template-columns: repeat(2, 1fr);
      }
    }

    @media (max-width: 768px) {
      .detail-grid {
        grid-template-columns: 1fr;
      }

      .stat-grid {
        grid-template-columns: 1fr 1fr;
      }

      .credit-header {
        flex-direction: column;
      }
    }
  `],
})
export class StoreCreditDetailPageComponent implements OnInit {
  private readonly finance = inject(FinanceService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  credit = signal<StoreCreditDetail | null>(null);
  transactions = signal<StoreCreditTransaction[]>([]);
  loading = signal(true);
  adjusting = signal(false);
  showAdjustModal = signal(false);
  adjustAmount: number | null = null;
  adjustReason = '';

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.finance.getStoreCreditById(id).subscribe({
        next: (c) => {
          this.credit.set(c);
          this.finance.getStoreCreditTransactions(id).subscribe({
            next: (res) => this.transactions.set(res.transactions),
          });
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
    } else {
      this.loading.set(false);
    }
  }

  openAdjust(): void {
    this.adjustAmount = null;
    this.adjustReason = '';
    this.showAdjustModal.set(true);
  }

  closeAdjustModal(): void {
    this.showAdjustModal.set(false);
  }

  submitAdjust(): void {
    const c = this.credit();
    if (!c || this.adjustAmount == null || this.adjustAmount === 0) return;
    const newBalance = c.currentBalance + this.adjustAmount;
    if (newBalance < 0) {
      alert('O ajuste resultaria em saldo negativo.');
      return;
    }
    this.adjusting.set(true);
    this.finance.adjustStoreCredit(c.externalId, {
      adjustmentAmount: this.adjustAmount,
      reason: this.adjustReason || undefined,
    }).subscribe({
      next: () => {
        this.adjusting.set(false);
        this.closeAdjustModal();
        this.finance.getStoreCreditById(c.externalId).subscribe((updated) => this.credit.set(updated));
        this.finance.getStoreCreditTransactions(c.externalId).subscribe((res) => this.transactions.set(res.transactions));
      },
      error: (err) => {
        this.adjusting.set(false);
        alert(err.error?.error ?? 'Erro ao ajustar.');
      },
    });
  }

  confirmCancel(): void {
    if (!confirm('Tem a certeza que deseja cancelar este crédito? O saldo restante será anulado.')) return;
    const c = this.credit();
    if (!c) return;
    this.finance.cancelStoreCredit(c.externalId).subscribe({
      next: () => this.router.navigate(['/finance/credits']),
      error: (err) => alert(err.error?.error ?? 'Erro ao cancelar.'),
    });
  }

  getStatusLabel(status: number): string {
    const map: Record<number, string> = { 1: 'Ativo', 2: 'Esgotado', 3: 'Cancelado', 4: 'Expirado' };
    return map[status] ?? '—';
  }

  getStatusBadgeClass(status: number): string {
    const map: Record<number, string> = { 1: 'success', 2: 'gray', 3: 'danger', 4: 'warning' };
    return map[status] ?? 'gray';
  }

  getTransactionTypeLabel(type: number): string {
    const map: Record<number, string> = { 1: 'Emissão', 2: 'Uso', 3: 'Ajuste', 4: 'Expiração', 5: 'Cancelamento' };
    return map[type] ?? '—';
  }

  goBack(): void {
    this.router.navigate(['/finance/credits']);
  }
}
