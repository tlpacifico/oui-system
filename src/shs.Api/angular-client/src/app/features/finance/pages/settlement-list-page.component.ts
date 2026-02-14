import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FinanceService, PendingSettlementGroup, SettlementListItem, SettlementStatus } from '../finance.service';

type TabId = 'pending' | 'processed' | 'all';

@Component({
  selector: 'oui-settlement-list-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="page">
      <div class="page-header">
        <div>
          <h1 class="page-title">Acertos</h1>
          <p class="page-subtitle">{{ getSubtitleText() }}</p>
        </div>
        <div class="page-header-actions">
          <a class="btn btn-primary" routerLink="/finance/settlements/new">
            + Novo Acerto
          </a>
        </div>
      </div>

      <!-- Tabs -->
      <div class="tabs-bar">
        <button
          class="tab-btn"
          [class.active]="activeTab() === 'pending'"
          (click)="setTab('pending')"
        >
          Pendentes ({{ pendingCount() }})
        </button>
        <button
          class="tab-btn"
          [class.active]="activeTab() === 'processed'"
          (click)="setTab('processed')"
        >
          Processados
        </button>
        <button
          class="tab-btn"
          [class.active]="activeTab() === 'all'"
          (click)="setTab('all')"
        >
          Todos
        </button>
      </div>

      @if (loading()) {
        <div class="state-message">A carregar...</div>
      } @else if (activeTab() === 'pending') {
        <!-- Pending items by supplier -->
        @if (pendingGroups().length === 0) {
          <div class="card empty-state">
            <span class="empty-icon">✓</span>
            <h3>Sem acertos pendentes</h3>
            <p>Não existem itens vendidos aguardando acerto.</p>
          </div>
        } @else {
          <div class="card table-card">
            <div class="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Fornecedor</th>
                    <th class="cell-center">Itens</th>
                    <th class="cell-right">Total Vendas</th>
                    <th class="cell-right">Crédito Loja</th>
                    <th class="cell-right">Resgate Cash</th>
                    <th class="cell-right">A Pagar</th>
                    <th>Ações</th>
                  </tr>
                </thead>
                <tbody>
                  @for (g of pendingGroups(); track g.supplierId) {
                    <tr>
                      <td>
                        <span class="initial-badge">{{ g.supplierInitial }}</span>
                        <strong>{{ g.supplierName }}</strong>
                      </td>
                      <td class="cell-center">{{ g.itemCount }}</td>
                      <td class="cell-right">{{ g.totalSalesAmount | currency: 'EUR' }}</td>
                      <td class="cell-right cell-muted">—</td>
                      <td class="cell-right cell-muted">—</td>
                      <td class="cell-right cell-muted">—</td>
                      <td>
                        <a
                          class="btn btn-primary btn-sm"
                          [routerLink]="['/finance/settlements/new']"
                          [queryParams]="{ supplierId: g.supplierId }"
                        >
                          Processar
                        </a>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          </div>
        }
      } @else {
        <!-- Settlements list (processed / all) -->
        @if (settlements().length === 0) {
          <div class="card empty-state">
            <span class="empty-icon">📋</span>
            <h3>Nenhum acerto encontrado</h3>
            <p>Os acertos processados aparecerão aqui.</p>
          </div>
        } @else {
          <div class="card table-card">
            <div class="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Fornecedor</th>
                    <th>Período</th>
                    <th class="cell-center">Itens</th>
                    <th class="cell-right">Total Vendas</th>
                    <th class="cell-right">Crédito Loja</th>
                    <th class="cell-right">Resgate Cash</th>
                    <th class="cell-right">A Pagar</th>
                    <th>Estado</th>
                    <th>Ações</th>
                  </tr>
                </thead>
                <tbody>
                  @for (s of settlements(); track s.externalId) {
                    <tr>
                      <td>
                        <span class="initial-badge">{{ s.supplierInitial }}</span>
                        <a class="link-name" [routerLink]="['/finance/settlements', s.externalId]">
                          {{ s.supplierName }}
                        </a>
                      </td>
                      <td>{{ formatPeriod(s.periodStart, s.periodEnd) }}</td>
                      <td class="cell-center">{{ s.itemCount }}</td>
                      <td class="cell-right">{{ s.totalSalesAmount | currency: 'EUR' }}</td>
                      <td class="cell-right">{{ s.storeCreditAmount | currency: 'EUR' }}</td>
                      <td class="cell-right">{{ s.cashRedemptionAmount | currency: 'EUR' }}</td>
                      <td class="cell-right cell-bold">{{ s.netAmountToSupplier | currency: 'EUR' }}</td>
                      <td>
                        <span class="badge" [ngClass]="getStatusClass(s.status)">
                          {{ getStatusLabel(s.status) }}
                        </span>
                      </td>
                      <td>
                        <a class="btn btn-outline btn-sm" [routerLink]="['/finance/settlements', s.externalId]">
                          Ver
                        </a>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
            @if (totalCount() > pageSize()) {
              <div class="pagination">
                <span class="pagination-info">
                  Mostrando {{ paginationStart() }}-{{ paginationEnd() }} de {{ totalCount() }} acertos
                </span>
                <div class="pagination-btns">
                  <button (click)="goToPage(page() - 1)" [disabled]="page() <= 1">‹</button>
                  @for (p of visiblePages(); track $index) {
                    @if (p === -1) {
                      <span class="pagination-ellipsis">...</span>
                    } @else {
                      <button (click)="goToPage(p)" [class.active]="page() === p">{{ p }}</button>
                    }
                  }
                  <button (click)="goToPage(page() + 1)" [disabled]="page() >= totalPages()">›</button>
                </div>
              </div>
            }
          </div>
        }
      }
    </div>
  `,
  styles: [`
    :host { display: block; }

    .page { max-width: 1200px; margin: 0 auto; }

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

    .page-header-actions { display: flex; gap: 8px; }

    .tabs-bar {
      display: flex;
      gap: 4px;
      margin-bottom: 20px;
      padding: 4px;
      background: #f8fafc;
      border-radius: 10px;
      width: fit-content;
    }

    .tab-btn {
      padding: 8px 16px;
      border: none;
      background: transparent;
      border-radius: 8px;
      cursor: pointer;
      font-size: 13px;
      font-weight: 600;
      color: #64748b;
      transition: all 0.15s;
    }

    .tab-btn:hover { background: #e2e8f0; color: #1e293b; }
    .tab-btn.active { background: #6366f1; color: white; }

    .card {
      background: #ffffff;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
    }

    .table-card { padding: 0; }

    .table-wrapper { overflow-x: auto; }

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

    tr:hover td { background: #f1f5f9; }

    .initial-badge {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 28px;
      height: 28px;
      border-radius: 6px;
      background: #eef2ff;
      color: #6366f1;
      font-weight: 600;
      font-size: 12px;
      margin-right: 8px;
    }

    .link-name { color: #6366f1; text-decoration: none; font-weight: 600; }
    .link-name:hover { text-decoration: underline; }

    .cell-center { text-align: center; }
    .cell-right { text-align: right; }
    .cell-bold { font-weight: 600; }
    .cell-muted { color: #94a3b8; }
    .cell-mono { font-family: monospace; font-size: 12px; }

    .badge {
      display: inline-block;
      padding: 3px 10px;
      border-radius: 20px;
      font-size: 11px;
      font-weight: 600;
      white-space: nowrap;
    }

    .badge-pending { background: #fef3c7; color: #92400e; }
    .badge-paid { background: #dcfce7; color: #166534; }
    .badge-cancelled { background: #fee2e2; color: #991b1b; }

    .empty-state {
      text-align: center;
      padding: 48px 24px;
    }

    .empty-icon { font-size: 48px; display: block; margin-bottom: 16px; opacity: 0.5; }
    .empty-state h3 { margin: 0 0 8px; font-size: 18px; color: #1e293b; }
    .empty-state p { color: #64748b; margin: 0; }

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
      text-decoration: none;
    }

    .btn-primary { background: #6366f1; color: white; }
    .btn-primary:hover { background: #4f46e5; }

    .btn-outline {
      background: white;
      color: #1e293b;
      border-color: #e2e8f0;
    }

    .btn-outline:hover { background: #f8fafc; }

    .btn-sm { padding: 5px 10px; font-size: 12px; }

    .pagination {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 16px 20px;
      font-size: 13px;
      color: #64748b;
    }

    .pagination-btns {
      display: flex;
      gap: 4px;
      align-items: center;
    }

    .pagination-btns button {
      width: 32px;
      height: 32px;
      border: 1px solid #e2e8f0;
      background: white;
      border-radius: 6px;
      cursor: pointer;
      font-size: 12px;
      color: #1e293b;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.15s;
    }

    .pagination-btns button:hover:not(:disabled):not(.active) { background: #f1f5f9; }
    .pagination-btns button.active { background: #6366f1; color: white; border-color: #6366f1; }
    .pagination-btns button:disabled { opacity: 0.4; cursor: not-allowed; }

    .pagination-ellipsis {
      width: 32px;
      text-align: center;
      color: #94a3b8;
    }

    .state-message {
      text-align: center;
      padding: 4rem 2rem;
      color: #64748b;
      font-size: 15px;
      background: white;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
    }

    @media (max-width: 768px) {
      .page-header { flex-direction: column; align-items: flex-start; gap: 12px; }
    }
  `],
})
export class SettlementListPageComponent implements OnInit {
  private readonly finance = inject(FinanceService);

  activeTab = signal<TabId>('pending');
  loading = signal(true);
  pendingGroups = signal<PendingSettlementGroup[]>([]);
  settlements = signal<SettlementListItem[]>([]);
  totalCount = signal(0);
  page = signal(1);
  pageSize = signal(20);

  pendingCount = computed(() => this.pendingGroups().length);
  totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize()) || 1);

  paginationStart = computed(() => {
    if (this.totalCount() === 0) return 0;
    return (this.page() - 1) * this.pageSize() + 1;
  });

  paginationEnd = computed(() => Math.min(this.page() * this.pageSize(), this.totalCount()));

  visiblePages = computed(() => {
    const total = this.totalPages();
    const current = this.page();
    const pages: number[] = [];
    if (total <= 7) {
      for (let i = 1; i <= total; i++) pages.push(i);
    } else {
      pages.push(1);
      if (current > 3) pages.push(-1);
      for (let i = Math.max(2, current - 1); i <= Math.min(total - 1, current + 1); i++) {
        pages.push(i);
      }
      if (current < total - 2) pages.push(-1);
      pages.push(total);
    }
    return pages;
  });

  getSubtitleText(): string {
    if (this.activeTab() === 'pending') {
      return `${this.pendingCount()} fornecedores com itens pendentes`;
    }
    if (this.activeTab() === 'processed') {
      return `${this.totalCount()} acertos processados`;
    }
    return `${this.totalCount()} acertos no total`;
  }

  ngOnInit(): void {
    this.loadPending();
  }

  setTab(tab: TabId): void {
    this.activeTab.set(tab);
    this.page.set(1);
    if (tab === 'pending') {
      this.loadPending();
    } else {
      this.loadSettlements(tab === 'processed' ? 2 : undefined);
    }
  }

  private loadPending(): void {
    this.loading.set(true);
    this.finance.getPendingSettlementItems().subscribe({
      next: (groups) => {
        this.pendingGroups.set(groups);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  private loadSettlements(status?: SettlementStatus): void {
    this.loading.set(true);
    this.finance.getSettlements(undefined, status, this.page(), this.pageSize()).subscribe({
      next: (res) => {
        this.settlements.set(res.data);
        this.totalCount.set(res.total);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  goToPage(p: number): void {
    if (p >= 1 && p <= this.totalPages()) {
      this.page.set(p);
      this.loadSettlements(this.activeTab() === 'processed' ? 2 : undefined);
    }
  }

  formatPeriod(start: string, end: string): string {
    const s = new Date(start);
    const e = new Date(end);
    return `${s.toLocaleDateString('pt-PT')} – ${e.toLocaleDateString('pt-PT')}`;
  }

  getStatusLabel(status: SettlementStatus): string {
    const map: Record<number, string> = { 1: 'Pendente', 2: 'Pago', 3: 'Cancelado' };
    return map[status] ?? '—';
  }

  getStatusClass(status: SettlementStatus): string {
    const map: Record<number, string> = { 1: 'badge-pending', 2: 'badge-paid', 3: 'badge-cancelled' };
    return map[status] ?? '';
  }
}
