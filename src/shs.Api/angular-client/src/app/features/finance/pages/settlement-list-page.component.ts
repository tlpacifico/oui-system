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
          <h1>Acertos</h1>
          <p class="subtitle">Gestão de acertos com fornecedores consignantes</p>
        </div>
        <div class="header-actions">
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
        <div class="loading">A carregar...</div>
      } @else if (activeTab() === 'pending') {
        <!-- Pending items by supplier -->
        @if (pendingGroups().length === 0) {
          <div class="card empty">
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
          <div class="card empty">
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
                <button
                  class="btn btn-outline btn-sm"
                  [disabled]="page() <= 1"
                  (click)="prevPage()"
                >
                  Anterior
                </button>
                <span class="page-info">
                  Página {{ page() }} de {{ totalPages() }}
                </span>
                <button
                  class="btn btn-outline btn-sm"
                  [disabled]="page() >= totalPages()"
                  (click)="nextPage()"
                >
                  Seguinte
                </button>
              </div>
            }
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .page { max-width: 1200px; margin: 0 auto; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 24px; }
    .page-header h1 { font-size: 22px; font-weight: 700; margin: 0; }
    .subtitle { font-size: 13px; color: #64748b; margin: 4px 0 0; }
    .header-actions { display: flex; gap: 8px; }
    .loading { text-align: center; padding: 48px; color: #64748b; }
    .tabs-bar { display: flex; gap: 4px; margin-bottom: 20px; }
    .tab-btn {
      padding: 8px 16px;
      border: 1px solid #e2e8f0;
      background: #fff;
      border-radius: 6px;
      cursor: pointer;
      font-size: 14px;
    }
    .tab-btn:hover { background: #f8fafc; }
    .tab-btn.active { background: #0f172a; color: #fff; border-color: #0f172a; }
    .initial-badge {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 28px;
      height: 28px;
      border-radius: 6px;
      background: #e2e8f0;
      font-weight: 600;
      font-size: 12px;
      margin-right: 8px;
    }
    .link-name { color: #0f172a; text-decoration: none; font-weight: 500; }
    .link-name:hover { text-decoration: underline; }
    .cell-center { text-align: center; }
    .cell-right { text-align: right; }
    .cell-bold { font-weight: 600; }
    .cell-muted { color: #94a3b8; }
    .badge { padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 500; }
    .badge-pending { background: #fef3c7; color: #92400e; }
    .badge-paid { background: #d1fae5; color: #065f46; }
    .badge-cancelled { background: #fee2e2; color: #991b1b; }
    .empty { text-align: center; padding: 48px; }
    .empty-icon { font-size: 48px; display: block; margin-bottom: 16px; opacity: 0.5; }
    .empty h3 { margin: 0 0 8px; font-size: 18px; }
    .empty p { color: #64748b; margin: 0 0 20px; }
    .pagination { display: flex; align-items: center; gap: 16px; padding: 16px; justify-content: center; }
    .page-info { font-size: 14px; color: #64748b; }
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

  prevPage(): void {
    if (this.page() > 1) {
      this.page.update((p) => p - 1);
      this.loadSettlements(this.activeTab() === 'processed' ? 2 : undefined);
    }
  }

  nextPage(): void {
    if (this.page() < this.totalPages()) {
      this.page.update((p) => p + 1);
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
