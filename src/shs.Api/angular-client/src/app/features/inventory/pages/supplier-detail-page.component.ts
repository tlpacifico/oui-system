import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { SupplierService } from '../services/supplier.service';
import { Supplier, SupplierItemListItem, SupplierReception } from '../../../core/models/supplier.model';
import { PagedResult } from '../../../core/models/item.model';

type TabId = 'info' | 'items' | 'receptions';

@Component({
  selector: 'oui-supplier-detail-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    @if (loading()) {
      <div class="state-message">A carregar...</div>
    } @else if (supplier()) {
      <!-- Top bar -->
      <div class="detail-topbar">
        <button class="btn btn-outline" (click)="goBack()">← Voltar</button>
        <div class="detail-topbar-right">
          <button class="btn btn-outline" (click)="goEdit()">Editar</button>
        </div>
      </div>

      <!-- Supplier header -->
      <div class="supplier-header">
        <div class="supplier-header-left">
          <span class="initial-badge-lg">{{ supplier()!.initial }}</span>
          <div class="supplier-header-info">
            <h1 class="supplier-name">{{ supplier()!.name }}</h1>
            <div class="supplier-meta">
              <span>{{ supplier()!.email }}</span>
              <span class="meta-sep">·</span>
              <span>{{ formatPhone(supplier()!.phoneNumber) }}</span>
              @if (supplier()!.taxNumber) {
                <span class="meta-sep">·</span>
                <span>NIF: {{ supplier()!.taxNumber }}</span>
              }
            </div>
          </div>
        </div>
      </div>

      <!-- KPI Stats -->
      <div class="stat-grid">
        <div class="card stat-card">
          <div class="stat-label">Total Itens</div>
          <div class="stat-value">{{ supplier()!.itemCount }}</div>
        </div>
        <div class="card stat-card">
          <div class="stat-label">Recepções</div>
          <div class="stat-value">{{ receptions().length }}</div>
        </div>
        <div class="card stat-card">
          <div class="stat-label">Itens à Venda</div>
          <div class="stat-value stat-success">{{ itemsToSellCount() }}</div>
        </div>
        <div class="card stat-card">
          <div class="stat-label">Itens Vendidos</div>
          <div class="stat-value stat-info">{{ itemsSoldCount() }}</div>
        </div>
      </div>

      <!-- Tabs -->
      <div class="tabs-bar">
        <button
          class="tab-btn"
          [class.active]="activeTab() === 'info'"
          (click)="setTab('info')"
        >Informações</button>
        <button
          class="tab-btn"
          [class.active]="activeTab() === 'items'"
          (click)="setTab('items')"
        >Itens ({{ supplier()!.itemCount }})</button>
        <button
          class="tab-btn"
          [class.active]="activeTab() === 'receptions'"
          (click)="setTab('receptions')"
        >Recepções ({{ receptions().length }})</button>
      </div>

      <!-- Tab: Info -->
      @if (activeTab() === 'info') {
        <div class="card">
          <div class="info-grid">
            <div class="info-row">
              <label>Nome</label>
              <span>{{ supplier()!.name }}</span>
            </div>
            <div class="info-row">
              <label>Inicial</label>
              <span class="mono">{{ supplier()!.initial }}</span>
            </div>
            <div class="info-row">
              <label>Email</label>
              <span>{{ supplier()!.email }}</span>
            </div>
            <div class="info-row">
              <label>Telefone</label>
              <span>{{ formatPhone(supplier()!.phoneNumber) }}</span>
            </div>
            <div class="info-row">
              <label>NIF</label>
              <span>{{ supplier()!.taxNumber || '—' }}</span>
            </div>
            <div class="info-row">
              <label>Criado em</label>
              <span>{{ supplier()!.createdOn | date: 'dd/MM/yyyy HH:mm' }}</span>
            </div>
            @if (supplier()!.updatedOn) {
              <div class="info-row">
                <label>Atualizado em</label>
                <span>{{ supplier()!.updatedOn | date: 'dd/MM/yyyy HH:mm' }}</span>
              </div>
            }
            @if (supplier()!.notes) {
              <div class="info-row full-width">
                <label>Notas</label>
                <span>{{ supplier()!.notes }}</span>
              </div>
            }
          </div>
        </div>
      }

      <!-- Tab: Items -->
      @if (activeTab() === 'items') {
        @if (loadingItems()) {
          <div class="state-message">A carregar itens...</div>
        } @else if (items().data.length === 0) {
          <div class="state-message">Nenhum item associado a este fornecedor.</div>
        } @else {
          <div class="card table-card">
            <div class="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Nome</th>
                    <th>Marca</th>
                    <th>Tam</th>
                    <th>Preço</th>
                    <th>Estado</th>
                    <th>Dias</th>
                    <th>Criado em</th>
                    <th>Ações</th>
                  </tr>
                </thead>
                <tbody>
                  @for (item of items().data; track item.externalId) {
                    <tr>
                      <td class="cell-mono">{{ item.identificationNumber }}</td>
                      <td><b>{{ item.name }}</b></td>
                      <td>{{ item.brand }}</td>
                      <td>{{ item.size }}</td>
                      <td><b>€{{ item.evaluatedPrice.toFixed(2) }}</b></td>
                      <td>
                        <span class="badge" [ngClass]="'badge-' + getStatusBadgeClass(item.status)">
                          {{ getStatusLabel(item.status) }}
                        </span>
                      </td>
                      <td [class]="getDaysClass(item.daysInStock)">{{ item.daysInStock }}</td>
                      <td>{{ item.createdOn | date: 'dd/MM/yyyy' }}</td>
                      <td>
                        <a class="btn btn-outline btn-sm" [routerLink]="['/inventory/items', item.externalId]">Ver</a>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
            @if (items().totalPages > 1) {
              <div class="pagination">
                <span class="pagination-info">
                  Página {{ items().page }} de {{ items().totalPages }} ({{ items().totalCount }} itens)
                </span>
                <div class="pagination-btns">
                  <button (click)="goItemsPage(items().page - 1)" [disabled]="items().page <= 1">‹</button>
                  <button (click)="goItemsPage(items().page + 1)" [disabled]="items().page >= items().totalPages">›</button>
                </div>
              </div>
            }
          </div>
        }
      }

      <!-- Tab: Receptions -->
      @if (activeTab() === 'receptions') {
        @if (loadingReceptions()) {
          <div class="state-message">A carregar recepções...</div>
        } @else if (receptions().length === 0) {
          <div class="state-message">Nenhuma recepção registada para este fornecedor.</div>
        } @else {
          <div class="card table-card">
            <div class="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Data</th>
                    <th>Itens Recebidos</th>
                    <th>Avaliados</th>
                    <th>Aceites</th>
                    <th>Rejeitados</th>
                    <th>Estado</th>
                    <th>Notas</th>
                  </tr>
                </thead>
                <tbody>
                  @for (rec of receptions(); track rec.externalId) {
                    <tr>
                      <td>{{ rec.receptionDate | date: 'dd/MM/yyyy' }}</td>
                      <td><b>{{ rec.itemCount }}</b></td>
                      <td>{{ rec.evaluatedCount }}</td>
                      <td>
                        @if (rec.acceptedCount > 0) {
                          <span class="badge badge-success">{{ rec.acceptedCount }}</span>
                        } @else {
                          <span>0</span>
                        }
                      </td>
                      <td>
                        @if (rec.rejectedCount > 0) {
                          <span class="badge badge-danger">{{ rec.rejectedCount }}</span>
                        } @else {
                          <span>0</span>
                        }
                      </td>
                      <td>
                        <span class="badge" [ngClass]="'badge-' + getReceptionBadgeClass(rec.status)">
                          {{ getReceptionLabel(rec.status) }}
                        </span>
                      </td>
                      <td class="cell-notes">{{ rec.notes || '—' }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          </div>
        }
      }
    } @else {
      <div class="state-message">Fornecedor não encontrado.</div>
    }
  `,
  styles: [`
    :host { display: block; }

    /* ── Topbar ── */
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

    /* ── Supplier Header ── */
    .supplier-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
    }

    .supplier-header-left {
      display: flex;
      align-items: center;
      gap: 16px;
    }

    .initial-badge-lg {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 56px;
      height: 56px;
      border-radius: 12px;
      background: #6366f1;
      color: white;
      font-size: 20px;
      font-weight: 800;
      letter-spacing: 1px;
      flex-shrink: 0;
    }

    .supplier-name {
      font-size: 24px;
      font-weight: 700;
      margin: 0 0 4px;
      color: #1e293b;
    }

    .supplier-meta {
      font-size: 13px;
      color: #64748b;
      display: flex;
      align-items: center;
      gap: 6px;
      flex-wrap: wrap;
    }

    .meta-sep { color: #cbd5e1; }

    /* ── Stat Grid ── */
    .stat-grid {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
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

    .stat-success { color: #16a34a; }
    .stat-info { color: #6366f1; }

    /* ── Tabs ── */
    .tabs-bar {
      display: flex;
      gap: 4px;
      margin-bottom: 20px;
      border-bottom: 2px solid #e2e8f0;
      padding-bottom: 0;
    }

    .tab-btn {
      padding: 10px 20px;
      font-size: 13px;
      font-weight: 600;
      cursor: pointer;
      background: none;
      border: none;
      color: #64748b;
      border-bottom: 2px solid transparent;
      margin-bottom: -2px;
      transition: all 0.15s;
    }

    .tab-btn:hover {
      color: #1e293b;
    }

    .tab-btn.active {
      color: #6366f1;
      border-bottom-color: #6366f1;
    }

    /* ── Cards ── */
    .card {
      background: #ffffff;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      padding: 24px;
    }

    .table-card {
      padding: 0;
    }

    /* ── Info Grid ── */
    .info-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 20px;
    }

    .info-row {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .info-row.full-width {
      grid-column: 1 / -1;
    }

    .info-row label {
      font-size: 12px;
      color: #64748b;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .info-row span {
      font-size: 14px;
      color: #1e293b;
      font-weight: 500;
    }

    .mono {
      font-family: monospace;
      font-size: 15px !important;
      font-weight: 700 !important;
      color: #6366f1 !important;
    }

    /* ── Table ── */
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

    .cell-mono {
      font-family: monospace;
      font-size: 12px;
    }

    .cell-notes {
      color: #64748b;
      max-width: 200px;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .days-warning { color: #f59e0b; font-weight: 700; }
    .days-danger  { color: #ef4444; font-weight: 700; }

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
    .badge-danger  { background: #fee2e2; color: #991b1b; }
    .badge-info    { background: #dbeafe; color: #1e40af; }
    .badge-gray    { background: #f1f5f9; color: #475569; }

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
      text-decoration: none;
      color: #1e293b;
    }

    .btn-outline {
      background: white;
      border-color: #e2e8f0;
    }

    .btn-outline:hover { background: #f8fafc; }

    .btn-sm {
      padding: 5px 10px;
      font-size: 12px;
    }

    /* ── Pagination ── */
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
    }

    .pagination-btns button {
      width: 32px;
      height: 32px;
      border: 1px solid #e2e8f0;
      background: white;
      border-radius: 6px;
      cursor: pointer;
      font-size: 14px;
      color: #1e293b;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .pagination-btns button:hover:not(:disabled) { background: #f1f5f9; }
    .pagination-btns button:disabled { opacity: 0.4; cursor: not-allowed; }

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

    /* ── Responsive ── */
    @media (max-width: 1024px) {
      .stat-grid { grid-template-columns: repeat(2, 1fr); }
    }

    @media (max-width: 768px) {
      .supplier-header-left { flex-direction: column; align-items: flex-start; }
      .info-grid { grid-template-columns: 1fr; }
      .stat-grid { grid-template-columns: 1fr 1fr; }
    }
  `]
})
export class SupplierDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly supplierService = inject(SupplierService);

  supplier = signal<Supplier | null>(null);
  loading = signal(true);
  activeTab = signal<TabId>('info');

  // Items tab
  items = signal<PagedResult<SupplierItemListItem>>({ data: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 });
  loadingItems = signal(false);
  private itemsLoaded = false;

  // Receptions tab
  receptions = signal<SupplierReception[]>([]);
  loadingReceptions = signal(false);
  private receptionsLoaded = false;

  // Computed stats
  itemsToSellCount = computed(() => {
    return this.items().data.filter(i => i.status === 'ToSell').length;
  });

  itemsSoldCount = computed(() => {
    return this.items().data.filter(i => i.status === 'Sold').length;
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadSupplier(id);
    }
  }

  private loadSupplier(externalId: string): void {
    this.loading.set(true);
    this.supplierService.getById(externalId).subscribe({
      next: (supplier) => {
        this.supplier.set(supplier);
        this.loading.set(false);
        // Pre-load items and receptions
        this.loadItems(externalId);
        this.loadReceptions(externalId);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  private loadItems(externalId: string, page = 1): void {
    this.loadingItems.set(true);
    this.supplierService.getItems(externalId, page).subscribe({
      next: (result) => {
        this.items.set(result);
        this.loadingItems.set(false);
        this.itemsLoaded = true;
      },
      error: () => {
        this.loadingItems.set(false);
      }
    });
  }

  private loadReceptions(externalId: string): void {
    this.loadingReceptions.set(true);
    this.supplierService.getReceptions(externalId).subscribe({
      next: (receptions) => {
        this.receptions.set(receptions);
        this.loadingReceptions.set(false);
        this.receptionsLoaded = true;
      },
      error: () => {
        this.loadingReceptions.set(false);
      }
    });
  }

  setTab(tab: TabId): void {
    this.activeTab.set(tab);
  }

  goItemsPage(page: number): void {
    const sup = this.supplier();
    if (sup && page >= 1 && page <= this.items().totalPages) {
      this.loadItems(sup.externalId, page);
    }
  }

  goBack(): void {
    this.router.navigate(['/inventory/suppliers']);
  }

  goEdit(): void {
    // Navigate back to list - edit is done via modal in the list page
    this.router.navigate(['/inventory/suppliers']);
  }

  formatPhone(phone: string): string {
    if (phone && phone.startsWith('+351') && phone.length === 13) {
      const num = phone.slice(4);
      return `+351 ${num.slice(0, 3)} ${num.slice(3, 6)} ${num.slice(6)}`;
    }
    return phone;
  }

  getStatusLabel(status: string): string {
    const labels: Record<string, string> = {
      'Received': 'Recebido',
      'Evaluated': 'Avaliado',
      'AwaitingAcceptance': 'Aguardando',
      'ToSell': 'À Venda',
      'Sold': 'Vendido',
      'Returned': 'Devolvido',
      'Paid': 'Pago',
      'Rejected': 'Rejeitado'
    };
    return labels[status] || status;
  }

  getStatusBadgeClass(status: string): string {
    const classes: Record<string, string> = {
      'Received': 'gray',
      'Evaluated': 'info',
      'AwaitingAcceptance': 'warning',
      'ToSell': 'success',
      'Sold': 'info',
      'Returned': 'gray',
      'Paid': 'success',
      'Rejected': 'danger'
    };
    return classes[status] || 'gray';
  }

  getReceptionLabel(status: string): string {
    const labels: Record<string, string> = {
      'PendingEvaluation': 'Pendente',
      'Evaluated': 'Avaliada',
      'ConsignmentCreated': 'Concluída'
    };
    return labels[status] || status;
  }

  getReceptionBadgeClass(status: string): string {
    const classes: Record<string, string> = {
      'PendingEvaluation': 'warning',
      'Evaluated': 'info',
      'ConsignmentCreated': 'success'
    };
    return classes[status] || 'gray';
  }

  getDaysClass(days: number): string {
    if (days >= 60) return 'days-danger';
    if (days >= 30) return 'days-warning';
    return '';
  }
}
