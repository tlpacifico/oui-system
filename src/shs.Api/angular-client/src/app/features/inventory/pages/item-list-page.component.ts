import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ItemService } from '../services/item.service';
import { ItemListItem, ItemStatus } from '../../../core/models/item.model';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'oui-item-list-page',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Itens do Estoque</h1>
        <p class="page-subtitle">{{ totalCount() }} itens encontrados</p>
      </div>
      <div class="page-header-actions">
        <button class="btn btn-outline">Exportar</button>
        <button class="btn btn-outline">Imprimir Etiquetas</button>
        <button class="btn btn-primary" routerLink="/inventory/items/new">+ Novo Item</button>
      </div>
    </div>

    <!-- Filters -->
    <div class="card filters-card">
      <div class="filters-bar">
        <input
          type="text"
          placeholder="Pesquisar por nome ou ID..."
          [(ngModel)]="searchText"
          (ngModelChange)="onSearchChange()"
          class="filter-input filter-search"
        />
        <select [(ngModel)]="statusFilter" (ngModelChange)="onFilterChange()" class="filter-select">
          <option value="">Todos os Estados</option>
          <option value="ToSell">À Venda</option>
          <option value="Evaluated">Avaliado</option>
          <option value="AwaitingAcceptance">Aguardando</option>
          <option value="Sold">Vendido</option>
          <option value="Returned">Devolvido</option>
          <option value="Rejected">Rejeitado</option>
        </select>
        <button class="btn btn-outline btn-sm" (click)="clearFilters()">Limpar Filtros</button>
      </div>
    </div>

    @if (loading()) {
      <div class="state-message">A carregar...</div>
    } @else if (items().length === 0) {
      <div class="state-message">Nenhuma peça encontrada.</div>
    } @else {
      <div class="card table-card">
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Foto</th>
                <th>ID</th>
                <th>Nome</th>
                <th>Marca</th>
                <th>Tam</th>
                <th>Cor</th>
                <th>Preço</th>
                <th>Estado</th>
                @if (hasDaysInStock()) {
                  <th>Dias</th>
                }
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              @for (item of items(); track item.externalId) {
                <tr
                  [class.row-warning]="getDaysLevel(item) === 'warning'"
                  [class.row-danger]="getDaysLevel(item) === 'danger'"
                >
                  <td>
                    <div class="item-thumb">
                      @if (item.primaryPhotoUrl) {
                        <img [src]="getPhotoUrl(item.primaryPhotoUrl)" [alt]="item.name" />
                      } @else {
                        <span class="thumb-empty">📷</span>
                      }
                    </div>
                  </td>
                  <td class="cell-mono">{{ item.identificationNumber }}</td>
                  <td><b>{{ item.name }}</b></td>
                  <td>{{ item.brand }}</td>
                  <td>{{ item.size }}</td>
                  <td>{{ item.color }}</td>
                  <td><b>€{{ item.evaluatedPrice.toFixed(2) }}</b></td>
                  <td>
                    <span class="badge" [ngClass]="'badge-' + getStatusBadgeClass(item.status)">
                      {{ getStatusLabel(item.status) }}
                    </span>
                  </td>
                  @if (hasDaysInStock()) {
                    <td [class]="'cell-days ' + getDaysLevel(item)">
                      {{ item.daysInStock ?? '-' }}
                    </td>
                  }
                  <td class="cell-actions">
                    <button class="btn btn-outline btn-sm" [routerLink]="['/inventory/items', item.externalId]">Ver</button>
                    <button class="btn btn-outline btn-sm" [routerLink]="['/inventory/items', item.externalId, 'edit']">Editar</button>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>

        <div class="pagination">
          <span class="pagination-info">
            Mostrando {{ paginationStart() }}-{{ paginationEnd() }} de {{ totalCount() }} itens
          </span>
          <div class="pagination-btns">
            <button (click)="goToPage(currentPage() - 1)" [disabled]="currentPage() === 1">‹</button>
            @for (p of visiblePages(); track $index) {
              @if (p === -1) {
                <span class="pagination-ellipsis">...</span>
              } @else {
                <button (click)="goToPage(p)" [class.active]="currentPage() === p">{{ p }}</button>
              }
            }
            <button (click)="goToPage(currentPage() + 1)" [disabled]="currentPage() === totalPages()">›</button>
          </div>
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

    .filter-input,
    .filter-select {
      padding: 8px 12px;
      border: 1px solid #e2e8f0;
      border-radius: 8px;
      font-size: 13px;
      background: white;
      outline: none;
      color: #1e293b;
    }

    .filter-input:focus,
    .filter-select:focus {
      border-color: #6366f1;
    }

    .filter-search {
      width: 240px;
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

    .row-warning td {
      background: #fef3c7;
    }

    .row-warning:hover td {
      background: #fde68a;
    }

    .row-danger td {
      background: #fee2e2;
    }

    .row-danger:hover td {
      background: #fecaca;
    }

    .cell-mono {
      font-family: monospace;
      font-size: 12px;
    }

    .cell-days {
      font-weight: 700;
    }

    .cell-days.warning {
      color: #f59e0b;
    }

    .cell-days.danger {
      color: #ef4444;
    }

    .cell-actions {
      display: flex;
      gap: 4px;
      white-space: nowrap;
    }

    .item-thumb {
      width: 40px;
      height: 40px;
      background: #f1f5f9;
      border-radius: 6px;
      display: flex;
      align-items: center;
      justify-content: center;
      overflow: hidden;
    }

    .item-thumb img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .thumb-empty {
      font-size: 16px;
      opacity: 0.5;
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
    .badge-danger  { background: #fee2e2; color: #991b1b; }
    .badge-info    { background: #dbeafe; color: #1e40af; }
    .badge-gray    { background: #f1f5f9; color: #475569; }

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

    .pagination-btns button:hover:not(:disabled):not(.active) {
      background: #f1f5f9;
    }

    .pagination-btns button.active {
      background: #6366f1;
      color: white;
      border-color: #6366f1;
    }

    .pagination-btns button:disabled {
      opacity: 0.4;
      cursor: not-allowed;
    }

    .pagination-ellipsis {
      width: 32px;
      text-align: center;
      color: #94a3b8;
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

    /* ── Responsive ── */
    @media (max-width: 768px) {
      .page-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 12px;
      }

      .filters-bar {
        flex-direction: column;
      }

      .filter-search {
        width: 100%;
      }
    }
  `]
})
export class ItemListPageComponent implements OnInit {
  private readonly itemService = inject(ItemService);
  private readonly baseUrl = environment.apiUrl.replace('/api', '');

  items = signal<ItemListItem[]>([]);
  loading = signal(false);
  currentPage = signal(1);
  totalPages = signal(1);
  totalCount = signal(0);
  searchText = '';
  statusFilter = '';
  private readonly pageSize = 20;

  paginationStart = computed(() => {
    if (this.totalCount() === 0) return 0;
    return (this.currentPage() - 1) * this.pageSize + 1;
  });

  paginationEnd = computed(() => {
    return Math.min(this.currentPage() * this.pageSize, this.totalCount());
  });

  visiblePages = computed(() => {
    const total = this.totalPages();
    const current = this.currentPage();
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

  hasDaysInStock = computed(() => {
    return this.items().some(i => i.daysInStock != null);
  });

  ngOnInit(): void {
    this.loadItems();
  }

  loadItems(): void {
    this.loading.set(true);
    this.itemService.getItems({
      search: this.searchText || undefined,
      status: this.statusFilter || undefined,
      page: this.currentPage(),
      pageSize: this.pageSize
    }).subscribe({
      next: (result) => {
        this.items.set(result.data);
        this.totalPages.set(result.totalPages);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  onSearchChange(): void {
    this.currentPage.set(1);
    this.loadItems();
  }

  onFilterChange(): void {
    this.currentPage.set(1);
    this.loadItems();
  }

  clearFilters(): void {
    this.searchText = '';
    this.statusFilter = '';
    this.currentPage.set(1);
    this.loadItems();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
      this.loadItems();
    }
  }

  getStatusLabel(status: ItemStatus): string {
    const labels: Record<ItemStatus, string> = {
      [ItemStatus.Received]: 'Recebido',
      [ItemStatus.Evaluated]: 'Avaliado',
      [ItemStatus.AwaitingAcceptance]: 'Aguardando',
      [ItemStatus.ToSell]: 'À Venda',
      [ItemStatus.Sold]: 'Vendido',
      [ItemStatus.Returned]: 'Devolvido',
      [ItemStatus.Paid]: 'Pago',
      [ItemStatus.Rejected]: 'Rejeitado'
    };
    return labels[status] || status;
  }

  getStatusBadgeClass(status: ItemStatus): string {
    const classes: Record<ItemStatus, string> = {
      [ItemStatus.Received]: 'gray',
      [ItemStatus.Evaluated]: 'info',
      [ItemStatus.AwaitingAcceptance]: 'warning',
      [ItemStatus.ToSell]: 'success',
      [ItemStatus.Sold]: 'info',
      [ItemStatus.Returned]: 'gray',
      [ItemStatus.Paid]: 'success',
      [ItemStatus.Rejected]: 'danger'
    };
    return classes[status] || 'gray';
  }

  getDaysLevel(item: ItemListItem): string {
    if (item.daysInStock == null) return '';
    if (item.daysInStock >= 60) return 'danger';
    if (item.daysInStock >= 30) return 'warning';
    return '';
  }

  getPhotoUrl(path?: string): string {
    if (!path) return '';
    return `${this.baseUrl}${path}`;
  }
}
