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
      <div class="page">
        <div class="detail-topbar">
          <button class="btn btn-outline" (click)="goBack()">← Voltar</button>
          <div class="detail-topbar-right">
            @if (credit()!.status === 1 && credit()!.currentBalance > 0) {
              <button class="btn btn-outline" (click)="openAdjust()">Ajustar Saldo</button>
              <button class="btn btn-outline btn-danger-outline" (click)="confirmCancel()">Cancelar Crédito</button>
            }
          </div>
        </div>

        <div class="credit-header">
          <h1>Crédito em Loja – {{ credit()!.supplierName }}</h1>
          <div class="header-meta">
            <span class="badge" [ngClass]="getStatusClass(credit()!.status)">{{ getStatusLabel(credit()!.status) }}</span>
            <span class="meta-sep">·</span>
            <span>Emitido em {{ credit()!.issuedOn | date: 'dd/MM/yyyy' }} por {{ credit()!.issuedBy }}</span>
          </div>
        </div>

        <div class="stat-grid">
          <div class="card stat-card">
            <div class="stat-label">Valor Original</div>
            <div class="stat-value">{{ credit()!.originalAmount | currency: 'EUR' }}</div>
          </div>
          <div class="card stat-card">
            <div class="stat-label">Saldo Atual</div>
            <div class="stat-value" [class.stat-success]="credit()!.currentBalance > 0">{{ credit()!.currentBalance | currency: 'EUR' }}</div>
          </div>
          @if (credit()!.expiresOn) {
            <div class="card stat-card">
              <div class="stat-label">Validade</div>
              <div class="stat-value">{{ credit()!.expiresOn | date: 'dd/MM/yyyy' }}</div>
            </div>
          }
        </div>

        @if (credit()!.sourceSettlement; as src) {
          <div class="card">
            <div class="card-title">Origem</div>
            <p>Acerto {{ src.periodStart | date: 'dd/MM/yyyy' }} – {{ src.periodEnd | date: 'dd/MM/yyyy' }} ({{ src.totalSalesAmount | currency: 'EUR' }})</p>
          </div>
        }

        @if (credit()!.notes) {
          <div class="card">
            <div class="card-title">Notas</div>
            <p>{{ credit()!.notes }}</p>
          </div>
        }

        <div class="card table-card">
          <div class="card-title-bar">
            <span class="card-title">Movimentos</span>
          </div>
          @if (transactions().length === 0) {
            <div class="empty-state">Sem movimentos.</div>
          } @else {
            <div class="table-wrapper">
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
                      <td class="cell-right" [class.negative]="t.amount < 0">{{ t.amount | currency: 'EUR' }}</td>
                      <td class="cell-right">{{ t.balanceAfter | currency: 'EUR' }}</td>
                      <td>{{ getTransactionTypeLabel(t.transactionType) }}</td>
                      <td>{{ t.processedBy }}</td>
                      <td>{{ t.notes || '—' }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
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
          <button class="btn btn-primary" (click)="submitAdjust()" [disabled]="adjusting()">Ajustar</button>
        </div>
      </div>
    }
  `,
  styles: [`
    .page { max-width: 900px; margin: 0 auto; }
    .detail-topbar { display: flex; justify-content: space-between; margin-bottom: 24px; }
    .detail-topbar-right { display: flex; gap: 8px; }
    .credit-header { margin-bottom: 24px; }
    .credit-header h1 { font-size: 22px; margin: 0; }
    .header-meta { font-size: 14px; color: #64748b; margin-top: 4px; }
    .meta-sep { margin: 0 8px; }
    .badge { padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 500; }
    .badge-active { background: #d1fae5; color: #065f46; }
    .badge-used { background: #e2e8f0; color: #475569; }
    .badge-cancelled { background: #fee2e2; color: #991b1b; }
    .stat-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(160px, 1fr)); gap: 16px; margin-bottom: 24px; }
    .stat-card { padding: 16px; text-align: center; }
    .stat-label { font-size: 12px; color: #64748b; margin-bottom: 4px; }
    .stat-value { font-size: 18px; font-weight: 700; }
    .stat-success { color: #059669; }
    .cell-right { text-align: right; }
    .cell-right.negative { color: #dc2626; }
    .empty-state { padding: 24px; text-align: center; color: #64748b; }
    .state-message { text-align: center; padding: 48px; color: #64748b; }
    .modal-backdrop { position: fixed; inset: 0; background: rgba(0,0,0,0.4); z-index: 100; }
    .modal { position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%); background: #fff; border-radius: 8px; box-shadow: 0 4px 20px rgba(0,0,0,0.15); z-index: 101; min-width: 400px; }
    .modal-header { display: flex; justify-content: space-between; align-items: center; padding: 16px 20px; border-bottom: 1px solid #e2e8f0; }
    .modal-header h3 { margin: 0; font-size: 18px; }
    .btn-close { background: none; border: none; font-size: 24px; cursor: pointer; color: #64748b; line-height: 1; }
    .modal-body { padding: 20px; }
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; font-weight: 500; margin-bottom: 6px; font-size: 14px; }
    .form-control { width: 100%; padding: 10px 12px; border: 1px solid #e2e8f0; border-radius: 6px; font-size: 14px; }
    .modal-footer { display: flex; justify-content: flex-end; gap: 12px; padding: 16px 20px; border-top: 1px solid #e2e8f0; }
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

  getStatusClass(status: number): string {
    const map: Record<number, string> = { 1: 'badge-active', 2: 'badge-used', 3: 'badge-cancelled', 4: 'badge-expired' };
    return map[status] ?? '';
  }

  getTransactionTypeLabel(type: number): string {
    const map: Record<number, string> = { 1: 'Emissão', 2: 'Uso', 3: 'Ajuste', 4: 'Expiração', 5: 'Cancelamento' };
    return map[type] ?? '—';
  }

  goBack(): void {
    this.router.navigate(['/finance/credits']);
  }
}
