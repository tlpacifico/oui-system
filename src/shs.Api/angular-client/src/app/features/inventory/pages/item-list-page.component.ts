import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ItemService } from '../services/item.service';
import { EcommerceService } from '../services/ecommerce.service';
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
                    @if (item.ecommerceProductExternalId) {
                      <a class="btn btn-ecommerce-view btn-sm" [href]="getStorefrontUrl(item.ecommerceProductSlug)" target="_blank">Ver Loja</a>
                      <button class="btn btn-ecommerce-remove btn-sm" (click)="unpublishFromEcommerce(item)" [disabled]="publishing()">Remover</button>
                    } @else if (item.status === 'ToSell') {
                      <button class="btn btn-ecommerce btn-sm" (click)="publishToEcommerce(item)" [disabled]="publishing()">Publicar</button>
                    }
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

    .filter-search { width: 240px; }

    td { border-bottom: 1px solid #E7E5E4; }

    tr:hover td { background: #F5F5F4; }

    .row-warning td { background: #FDF3E3; }
    .row-warning:hover td { background: #fde68a; }
    .row-danger td { background: #FCEAEA; }
    .row-danger:hover td { background: rgba(196, 91, 91, 0.3); }

    .cell-days { font-weight: 700; }
    .cell-days.warning { color: #f59e0b; }
    .cell-days.danger { color: #C45B5B; }

    .btn-ecommerce {
      background: #5B7153;
      color: white;
      border-color: #5B7153;
    }

    .btn-ecommerce:hover:not(:disabled) { background: #4A5E43; }

    .btn-ecommerce-view {
      background: #2563eb;
      color: white;
      border-color: #2563eb;
      text-decoration: none;
    }

    .btn-ecommerce-view:hover { background: #1d4ed8; }

    .btn-ecommerce-remove {
      background: white;
      color: #A84848;
      border-color: #A84848;
    }

    .btn-ecommerce-remove:hover:not(:disabled) { background: rgba(196, 91, 91, 0.06); }

    .item-thumb {
      width: 40px;
      height: 40px;
      background: #F5F5F4;
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
  `]
})
export class ItemListPageComponent implements OnInit {
  private readonly itemService = inject(ItemService);
  private readonly ecommerceService = inject(EcommerceService);
  private readonly baseUrl = environment.apiUrl.replace('/api', '');

  items = signal<ItemListItem[]>([]);
  loading = signal(false);
  publishing = signal(false);
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

  publishToEcommerce(item: ItemListItem): void {
    this.publishing.set(true);
    this.ecommerceService.publishItem(item.externalId).subscribe({
      next: () => {
        this.publishing.set(false);
        this.loadItems();
      },
      error: (err) => {
        this.publishing.set(false);
        const msg = err.error?.error || 'Erro ao publicar no e-commerce.';
        alert(msg);
      }
    });
  }

  unpublishFromEcommerce(item: ItemListItem): void {
    if (!item.ecommerceProductExternalId) return;
    if (!confirm(`Remover "${item.name}" do e-commerce?`)) return;

    this.publishing.set(true);
    this.ecommerceService.unpublishProduct(item.ecommerceProductExternalId).subscribe({
      next: () => {
        this.publishing.set(false);
        this.loadItems();
      },
      error: (err) => {
        this.publishing.set(false);
        const msg = err.error?.error || 'Erro ao remover do e-commerce.';
        alert(msg);
      }
    });
  }

  getStorefrontUrl(slug?: string): string {
    if (!slug) return '#';
    return `http://localhost:3000/produtos/${slug}`;
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
