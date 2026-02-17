import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { FinanceService, SupplierCashBalanceResponse, SupplierStoreCreditsResponse } from '../finance.service';
import { SupplierService } from '../../inventory/services/supplier.service';
import { SupplierListItem } from '../../../core/models/supplier.model';

@Component({
  selector: 'oui-store-credits-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Créditos em Loja</h1>
        <p class="page-subtitle">Créditos e saldo para resgate em dinheiro dos fornecedores</p>
      </div>
      @if (selectedSupplierId != null && supplierData()) {
        <div class="page-header-actions">
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
      }
    </div>

    <!-- Filters -->
    <div class="card filters-card">
      <div class="filters-bar">
        <label class="filter-label">Fornecedor</label>
        <select
          [(ngModel)]="selectedSupplierId"
          (ngModelChange)="onSupplierChange()"
          class="filter-select filter-supplier"
        >
          <option [ngValue]="null">Selecione o fornecedor</option>
          @for (s of suppliers(); track s.externalId) {
            <option [ngValue]="s.id">{{ s.initial }} – {{ s.name }}</option>
          }
        </select>
        @if (selectedSupplierId != null) {
          <button class="btn btn-outline btn-sm" (click)="clearSupplier()">Limpar</button>
        }
      </div>
    </div>

    @if (loading()) {
      <div class="state-message">A carregar...</div>
    } @else if (selectedSupplierId == null) {
      <div class="state-message">Selecione um fornecedor para ver os créditos em loja e o saldo para resgate.</div>
    } @else if (supplierData()) {
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

      <!-- Store credits list -->
      <div class="card table-card">
        <div class="table-wrapper">
          @if (storeCredits().length === 0) {
            <div class="empty-state">Nenhum crédito em loja.</div>
          } @else {
            <table>
              <thead>
                <tr>
                  <th>Emitido em</th>
                  <th class="cell-right">Original</th>
                  <th class="cell-right">Saldo</th>
                  <th>Estado</th>
                  <th>Origem</th>
                  <th class="cell-actions">Ações</th>
                </tr>
              </thead>
              <tbody>
                @for (c of storeCredits(); track c.externalId) {
                  <tr>
                    <td>{{ c.issuedOn | date: 'dd/MM/yyyy' }}</td>
                    <td class="cell-right">{{ c.originalAmount | currency: 'EUR' }}</td>
                    <td class="cell-right">{{ c.currentBalance | currency: 'EUR' }}</td>
                    <td>
                      <span class="badge" [ngClass]="'badge-' + getCreditStatusBadgeClass(c.status)">
                        {{ getCreditStatusLabel(c.status) }}
                      </span>
                    </td>
                    <td>{{ c.sourceSettlement ? 'Acerto' : 'Manual' }}</td>
                    <td class="cell-actions">
                      <button class="btn btn-outline btn-sm" (click)="viewCredit(c.externalId)">Ver</button>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </div>
      </div>

      <!-- Cash balance history -->
      @if (cashBalance() !== 0 || cashHistory().length > 0) {
        <div class="card table-card">
          <div class="table-wrapper">
            @if (cashHistory().length === 0) {
              <div class="empty-state">Sem movimentos. Saldo disponível: {{ cashBalance() | currency: 'EUR' }}</div>
            } @else {
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
                      <td class="cell-right" [class.cell-negative]="t.amount < 0">{{ t.amount | currency: 'EUR' }}</td>
                      <td>{{ getTransactionTypeLabel(t.transactionType) }}</td>
                      <td>{{ t.processedBy }}</td>
                      <td>{{ t.settlementPeriod || '—' }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            }
          </div>
        </div>
      }
    }

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
    :host { display: block; }

    /* ── Page header ── */
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }

    .page-title {
      font-size: 22px;
      font-weight: 700;
      margin: 0 0 4px;
      color: #1e293b;
    }

    .page-subtitle {
      font-size: 14px;
      color: #64748b;
      margin: 0;
    }

    .page-header-actions {
      display: flex;
      gap: 8px;
    }

    /* ── Cards ── */
    .card {
      background: #ffffff;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      padding: 20px;
    }

    .filters-card {
      margin-bottom: 20px;
      padding: 16px;
    }

    .table-card {
      margin-bottom: 20px;
      padding: 0;
    }

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

    .btn-primary {
      background: #6366f1;
      color: white;
    }

    .btn-primary:hover {
      background: #4f46e5;
    }

    .btn-outline {
      background: white;
      color: #1e293b;
      border-color: #e2e8f0;
    }

    .btn-outline:hover {
      background: #f8fafc;
    }

    .btn-outline:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .btn-sm {
      padding: 5px 10px;
      font-size: 12px;
    }

    /* ── Filters ── */
    .filters-bar {
      display: flex;
      gap: 12px;
      flex-wrap: wrap;
      align-items: center;
    }

    .filter-label {
      font-size: 13px;
      font-weight: 500;
      color: #1e293b;
    }

    .filter-select {
      padding: 8px 12px;
      border: 1px solid #e2e8f0;
      border-radius: 8px;
      font-size: 13px;
      background: white;
      outline: none;
      color: #1e293b;
    }

    .filter-select:focus {
      border-color: #6366f1;
    }

    .filter-supplier {
      min-width: 280px;
    }

    /* ── Stats ── */
    .stat-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
      gap: 16px;
      margin-bottom: 20px;
    }

    .stat-card {
      padding: 20px;
      text-align: center;
    }

    .stat-label {
      font-size: 12px;
      color: #64748b;
      margin-bottom: 8px;
    }

    .stat-value {
      font-size: 20px;
      font-weight: 700;
    }

    .stat-success { color: #059669; }
    .stat-info { color: #0284c7; }

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

    .cell-actions {
      display: flex;
      gap: 4px;
      white-space: nowrap;
    }

    /* ── Badges ── */
    .badge {
      display: inline-block;
      padding: 3px 10px;
      border-radius: 20px;
      font-size: 11px;
      font-weight: 600;
      white-space: nowrap;
    }

    .badge-success { background: #dcfce7; color: #166534; }
    .badge-warning { background: #fef3c7; color: #92400e; }
    .badge-danger { background: #fee2e2; color: #991b1b; }
    .badge-info { background: #dbeafe; color: #1e40af; }
    .badge-gray { background: #f1f5f9; color: #475569; }

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
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; font-weight: 500; margin-bottom: 6px; font-size: 14px; }
    .form-control { width: 100%; padding: 10px 12px; border: 1px solid #e2e8f0; border-radius: 8px; font-size: 14px; }
    .modal-backdrop { position: fixed; inset: 0; background: rgba(0,0,0,0.4); z-index: 100; }
    .modal { position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%); background: #fff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0,0,0,0.15); z-index: 101; min-width: 400px; max-width: 90vw; }
    .modal-header { display: flex; justify-content: space-between; align-items: center; padding: 16px 20px; border-bottom: 1px solid #e2e8f0; }
    .modal-header h3 { margin: 0; font-size: 18px; }
    .btn-close { background: none; border: none; font-size: 24px; cursor: pointer; color: #64748b; line-height: 1; }
    .modal-body { padding: 20px; }
    .modal-footer { display: flex; justify-content: flex-end; gap: 12px; padding: 16px 20px; border-top: 1px solid #e2e8f0; }
    .balance-info { margin-bottom: 16px; }

    /* ── Responsive ── */
    @media (max-width: 768px) {
      .page-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 12px;
      }

      .filters-bar {
        flex-direction: column;
        align-items: stretch;
      }

      .filter-supplier {
        min-width: 100%;
      }
    }
  `],
})
export class StoreCreditsPageComponent implements OnInit {
  private readonly finance = inject(FinanceService);
  private readonly supplierService = inject(SupplierService);
  private readonly router = inject(Router);

  suppliers = signal<SupplierListItem[]>([]);
  selectedSupplierId: number | null = null;
  supplierData = signal<{ credits: SupplierStoreCreditsResponse; cash: SupplierCashBalanceResponse; history: any[] } | null>(null);
  loading = signal(false);
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
      error: () => {},
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

  clearSupplier(): void {
    this.selectedSupplierId = null;
    this.supplierData.set(null);
    this.loading.set(false);
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

  getCreditStatusBadgeClass(status: number): string {
    const map: Record<number, string> = { 1: 'success', 2: 'gray', 3: 'danger', 4: 'warning' };
    return map[status] ?? 'gray';
  }

  getTransactionTypeLabel(type: number): string {
    const map: Record<number, string> = { 1: 'Crédito acerto', 2: 'Resgate dinheiro' };
    return map[type] ?? '—';
  }
}
