import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { FinanceService, StoreCreditItem, SupplierCashBalanceResponse, SupplierStoreCreditsResponse } from '../finance.service';
import { SupplierService } from '../../inventory/services/supplier.service';
import { SupplierListItem } from '../../../core/models/supplier.model';

@Component({
  selector: 'oui-store-credits-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page">
      <div class="page-header">
        <div>
          <h1>Créditos em Loja</h1>
          <p class="subtitle">Créditos e saldo para resgate em dinheiro dos fornecedores</p>
        </div>
      </div>

      <!-- Supplier selector -->
      <div class="card form-card">
        <div class="form-group">
          <label>Fornecedor</label>
          <select
            [(ngModel)]="selectedSupplierId"
            (ngModelChange)="onSupplierChange()"
            class="form-control"
          >
            <option [ngValue]="null">Selecione o fornecedor</option>
            @for (s of suppliers(); track s.externalId) {
              <option [ngValue]="s.id">{{ s.initial }} – {{ s.name }}</option>
            }
          </select>
        </div>
      </div>

      @if (loading()) {
        <div class="loading">A carregar...</div>
      } @else if (selectedSupplierId != null && supplierData()) {
        <!-- KPIs -->
        <div class="stat-grid">
          <div class="card stat-card">
            <div class="stat-label">Crédito em Loja (total)</div>
            <div class="stat-value stat-success">{{ storeCreditsTotal() | currency: 'EUR' }}</div>
          </div>
          <div class="card stat-card">
            <div class="stat-label">Saldo para Resgate</div>
            <div class="stat-value stat-info">{{ cashBalance() | currency: 'EUR' }}</div>
          </div>
        </div>

        <!-- Actions -->
        <div class="card actions-card">
          <div class="card-title">Ações</div>
          <div class="actions-row">
            <button class="btn btn-primary" (click)="openIssueCredit()">
              Emitir Crédito em Loja
            </button>
            <button
              class="btn btn-outline"
              (click)="openCashRedemption()"
              [disabled]="cashBalance() <= 0"
            >
              Processar Resgate em Dinheiro
            </button>
          </div>
        </div>

        <!-- Store credits list -->
        <div class="card table-card">
          <div class="card-title-bar">
            <span class="card-title">Créditos em Loja</span>
          </div>
          @if (storeCredits().length === 0) {
            <div class="empty-state">Nenhum crédito em loja.</div>
          } @else {
            <div class="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Emitido em</th>
                    <th class="cell-right">Original</th>
                    <th class="cell-right">Saldo</th>
                    <th>Estado</th>
                    <th>Origem</th>
                    <th>Ações</th>
                  </tr>
                </thead>
                <tbody>
                  @for (c of storeCredits(); track c.externalId) {
                    <tr>
                      <td>{{ c.issuedOn | date: 'dd/MM/yyyy' }}</td>
                      <td class="cell-right">{{ c.originalAmount | currency: 'EUR' }}</td>
                      <td class="cell-right">{{ c.currentBalance | currency: 'EUR' }}</td>
                      <td><span class="badge" [ngClass]="getCreditStatusClass(c.status)">{{ getCreditStatusLabel(c.status) }}</span></td>
                      <td>{{ c.sourceSettlement ? 'Acerto' : 'Manual' }}</td>
                      <td>
                        <button class="btn btn-outline btn-sm" (click)="viewCredit(c.externalId)">
                          Ver
                        </button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }
        </div>

        <!-- Cash balance history -->
        @if (cashBalance() !== 0 || cashHistory().length > 0) {
          <div class="card table-card">
            <div class="card-title-bar">
              <span class="card-title">Histórico Resgate em Dinheiro</span>
            </div>
            @if (cashHistory().length === 0) {
              <div class="empty-state">Sem movimentos. Saldo disponível: {{ cashBalance() | currency: 'EUR' }}</div>
            } @else {
              <div class="table-wrapper">
                <table>
                  <thead>
                    <tr>
                      <th>Data</th>
                      <th class="cell-right">Valor</th>
                      <th>Tipo</th>
                      <th>Processado por</th>
                      <th>Origem</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (t of cashHistory(); track t.externalId) {
                      <tr>
                        <td>{{ t.transactionDate | date: 'dd/MM/yyyy HH:mm' }}</td>
                        <td class="cell-right" [class.negative]="t.amount < 0">{{ t.amount | currency: 'EUR' }}</td>
                        <td>{{ getTransactionTypeLabel(t.transactionType) }}</td>
                        <td>{{ t.processedBy }}</td>
                        <td>{{ t.settlementPeriod || '—' }}</td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            }
          </div>
        }
      }
    </div>

    <!-- Issue credit modal -->
    @if (showIssueModal()) {
      <div class="modal-backdrop" (click)="closeIssueModal()"></div>
      <div class="modal">
        <div class="modal-header">
          <h3>Emitir Crédito em Loja</h3>
          <button class="btn-close" (click)="closeIssueModal()">×</button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label>Valor (€) *</label>
            <input type="number" [(ngModel)]="issueAmount" step="0.01" min="0.01" class="form-control" />
          </div>
          <div class="form-group">
            <label>Data de validade (opcional)</label>
            <input type="date" [(ngModel)]="issueExpiresOn" class="form-control" />
          </div>
          <div class="form-group">
            <label>Notas</label>
            <textarea [(ngModel)]="issueNotes" class="form-control" rows="2"></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-outline" (click)="closeIssueModal()">Cancelar</button>
          <button class="btn btn-primary" (click)="submitIssueCredit()" [disabled]="!issueAmount || issueAmount <= 0 || issuing()">
            {{ issuing() ? 'A emitir...' : 'Emitir' }}
          </button>
        </div>
      </div>
    }

    <!-- Cash redemption modal -->
    @if (showRedemptionModal()) {
      <div class="modal-backdrop" (click)="closeRedemptionModal()"></div>
      <div class="modal">
        <div class="modal-header">
          <h3>Processar Resgate em Dinheiro</h3>
          <button class="btn-close" (click)="closeRedemptionModal()">×</button>
        </div>
        <div class="modal-body">
          <p class="balance-info">Saldo disponível: <strong>{{ cashBalance() | currency: 'EUR' }}</strong></p>
          <div class="form-group">
            <label>Valor a resgatar (€) *</label>
            <input type="number" [(ngModel)]="redemptionAmount" step="0.01" min="0.01" [max]="cashBalance()" class="form-control" />
          </div>
          <div class="form-group">
            <label>Notas</label>
            <textarea [(ngModel)]="redemptionNotes" class="form-control" rows="2"></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-outline" (click)="closeRedemptionModal()">Cancelar</button>
          <button class="btn btn-primary" (click)="submitCashRedemption()" [disabled]="!redemptionAmount || redemptionAmount <= 0 || redemptionAmount > cashBalance() || redeeming()">
            {{ redeeming() ? 'A processar...' : 'Processar Resgate' }}
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    .page { max-width: 900px; margin: 0 auto; }
    .page-header { margin-bottom: 24px; }
    .subtitle { color: #64748b; margin: 4px 0 0; font-size: 14px; }
    .form-card { margin-bottom: 24px; max-width: 400px; }
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; font-weight: 500; margin-bottom: 6px; font-size: 14px; }
    .form-control { width: 100%; padding: 10px 12px; border: 1px solid #e2e8f0; border-radius: 6px; font-size: 14px; }
    .loading { text-align: center; padding: 48px; color: #64748b; }
    .stat-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px; margin-bottom: 24px; }
    .stat-card { padding: 16px; text-align: center; }
    .stat-label { font-size: 12px; color: #64748b; margin-bottom: 4px; }
    .stat-value { font-size: 18px; font-weight: 700; }
    .stat-success { color: #059669; }
    .stat-info { color: #0284c7; }
    .actions-card { margin-bottom: 24px; }
    .actions-row { display: flex; gap: 12px; flex-wrap: wrap; }
    .cell-right { text-align: right; }
    .cell-right.negative { color: #dc2626; }
    .badge { padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 500; }
    .badge-active { background: #d1fae5; color: #065f46; }
    .badge-used { background: #e2e8f0; color: #475569; }
    .badge-cancelled { background: #fee2e2; color: #991b1b; }
    .badge-expired { background: #fef3c7; color: #92400e; }
    .empty-state { padding: 24px; text-align: center; color: #64748b; }
    .modal-backdrop { position: fixed; inset: 0; background: rgba(0,0,0,0.4); z-index: 100; }
    .modal { position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%); background: #fff; border-radius: 8px; box-shadow: 0 4px 20px rgba(0,0,0,0.15); z-index: 101; min-width: 400px; max-width: 90vw; }
    .modal-header { display: flex; justify-content: space-between; align-items: center; padding: 16px 20px; border-bottom: 1px solid #e2e8f0; }
    .modal-header h3 { margin: 0; font-size: 18px; }
    .btn-close { background: none; border: none; font-size: 24px; cursor: pointer; color: #64748b; line-height: 1; }
    .modal-body { padding: 20px; }
    .modal-footer { display: flex; justify-content: flex-end; gap: 12px; padding: 16px 20px; border-top: 1px solid #e2e8f0; }
    .balance-info { margin-bottom: 16px; }
  `],
})
export class StoreCreditsPageComponent implements OnInit {
  private readonly finance = inject(FinanceService);
  private readonly supplierService = inject(SupplierService);
  private readonly router = inject(Router);

  suppliers = signal<SupplierListItem[]>([]);
  selectedSupplierId: number | null = null;
  supplierData = signal<{ credits: SupplierStoreCreditsResponse; cash: SupplierCashBalanceResponse; history: any[] } | null>(null);
  loading = signal(true);
  issuing = signal(false);
  redeeming = signal(false);

  showIssueModal = signal(false);
  showRedemptionModal = signal(false);
  issueAmount: number | null = null;
  issueExpiresOn = '';
  issueNotes = '';
  redemptionAmount: number | null = null;
  redemptionNotes = '';

  storeCredits = computed(() => this.supplierData()?.credits?.credits ?? []);
  storeCreditsTotal = computed(() => this.supplierData()?.credits?.totalActiveBalance ?? 0);
  cashBalance = computed(() => this.supplierData()?.cash?.availableBalance ?? 0);
  cashHistory = computed(() => this.supplierData()?.history ?? []);

  ngOnInit(): void {
    this.supplierService.getAll().subscribe({
      next: (list) => this.suppliers.set(list),
    });
  }

  onSupplierChange(): void {
    if (this.selectedSupplierId == null) {
      this.supplierData.set(null);
      this.loading.set(false);
      return;
    }
    this.loadSupplierData();
  }

  private loadSupplierData(): void {
    if (this.selectedSupplierId == null) return;
    this.loading.set(true);
    const id = this.selectedSupplierId;
    this.finance.getSupplierStoreCredits(id).subscribe({
      next: (credits) => {
        this.finance.getSupplierCashBalance(id).subscribe({
          next: (cash) => {
            this.finance.getSupplierCashHistory(id).subscribe({
              next: (history) => {
                this.supplierData.set({ credits, cash, history: history.transactions });
                this.loading.set(false);
              },
              error: () => this.loading.set(false),
            });
          },
          error: () => this.loading.set(false),
        });
      },
      error: () => this.loading.set(false),
    });
  }

  openIssueCredit(): void {
    this.issueAmount = null;
    this.issueExpiresOn = '';
    this.issueNotes = '';
    this.showIssueModal.set(true);
  }

  closeIssueModal(): void {
    this.showIssueModal.set(false);
  }

  submitIssueCredit(): void {
    if (this.selectedSupplierId == null || !this.issueAmount || this.issueAmount <= 0) return;
    this.issuing.set(true);
    this.finance.issueStoreCredit({
      supplierId: this.selectedSupplierId,
      amount: this.issueAmount,
      expiresOn: this.issueExpiresOn || undefined,
      notes: this.issueNotes || undefined,
    }).subscribe({
      next: () => {
        this.issuing.set(false);
        this.closeIssueModal();
        this.loadSupplierData();
      },
      error: (err) => {
        this.issuing.set(false);
        alert(err.error?.error ?? 'Erro ao emitir crédito.');
      },
    });
  }

  openCashRedemption(): void {
    this.redemptionAmount = null;
    this.redemptionNotes = '';
    this.showRedemptionModal.set(true);
  }

  closeRedemptionModal(): void {
    this.showRedemptionModal.set(false);
  }

  submitCashRedemption(): void {
    if (this.selectedSupplierId == null || !this.redemptionAmount || this.redemptionAmount <= 0) return;
    if (this.redemptionAmount > this.cashBalance()) return;
    this.redeeming.set(true);
    this.finance.processCashRedemption({
      supplierId: this.selectedSupplierId,
      amount: this.redemptionAmount,
      notes: this.redemptionNotes || undefined,
    }).subscribe({
      next: () => {
        this.redeeming.set(false);
        this.closeRedemptionModal();
        this.loadSupplierData();
      },
      error: (err) => {
        this.redeeming.set(false);
        alert(err.error?.error ?? err.error?.availableBalance != null ? `Saldo insuficiente. Disponível: ${err.error.availableBalance}€` : 'Erro ao processar resgate.');
      },
    });
  }

  viewCredit(externalId: string): void {
    this.router.navigate(['/finance/credits', externalId]);
  }

  getCreditStatusLabel(status: number): string {
    const map: Record<number, string> = { 1: 'Ativo', 2: 'Esgotado', 3: 'Cancelado', 4: 'Expirado' };
    return map[status] ?? '—';
  }

  getCreditStatusClass(status: number): string {
    const map: Record<number, string> = { 1: 'badge-active', 2: 'badge-used', 3: 'badge-cancelled', 4: 'badge-expired' };
    return map[status] ?? '';
  }

  getTransactionTypeLabel(type: number): string {
    const map: Record<number, string> = { 1: 'Crédito acerto', 2: 'Resgate dinheiro' };
    return map[type] ?? '—';
  }
}
