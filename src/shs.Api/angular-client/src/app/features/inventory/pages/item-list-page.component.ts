import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ItemService } from '../services/item.service';
import { EcommerceService } from '../services/ecommerce.service';
import { BrandService } from '../services/brand.service';
import { CategoryService } from '../services/category.service';
import { SupplierService } from '../services/supplier.service';
import { ColorService } from '../services/color.service';
import { ItemListItem, ItemStatus } from '../../../core/models/item.model';
import { BrandListItem } from '../../../core/models/brand.model';
import { CategoryListItem } from '../../../core/models/category.model';
import { SupplierListItem } from '../../../core/models/supplier.model';
import { ColorListItem } from '../../../core/models/color.model';
import { HasPermissionDirective } from '../../../core/auth/directives/has-permission.directive';
import { SearchableSelectComponent, SearchableOption } from '../../../shared/components/searchable-select/searchable-select.component';
import { environment } from '../../../../environments/environment';

interface AdvancedFilters {
  brandExternalId: string;
  categoryExternalId: string;
  supplierExternalId: string;
  colorExternalId: string;
  size: string;
  condition: string;
  acquisitionType: string;
  minPrice: number | null;
  maxPrice: number | null;
  createdFrom: string;
  createdTo: string;
}

interface FilterChip {
  key: keyof AdvancedFilters | 'price' | 'created';
  text: string;
}

function emptyFilters(): AdvancedFilters {
  return {
    brandExternalId: '',
    categoryExternalId: '',
    supplierExternalId: '',
    colorExternalId: '',
    size: '',
    condition: '',
    acquisitionType: '',
    minPrice: null,
    maxPrice: null,
    createdFrom: '',
    createdTo: '',
  };
}

@Component({
  selector: 'oui-item-list-page',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, HasPermissionDirective, SearchableSelectComponent],
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
        <select [(ngModel)]="statusFilter" (ngModelChange)="onTopFilterChange()" class="filter-select">
          <option value="">Todos os Estados</option>
          <option value="ToSell">À Venda</option>
          <option value="Evaluated">Avaliado</option>
          <option value="AwaitingAcceptance">Aguardando</option>
          <option value="Sold">Vendido</option>
          <option value="Returned">Devolvido</option>
          <option value="Rejected">Rejeitado</option>
        </select>
        <button class="btn btn-outline btn-sm filter-toggle" (click)="filtersOpen.set(!filtersOpen())">
          ⚙ Filtros avançados
          @if (advancedCount() > 0) {
            <span class="filter-count">{{ advancedCount() }}</span>
          }
          <span class="chevron">{{ filtersOpen() ? '▴' : '▾' }}</span>
        </button>
        <button class="btn btn-ghost btn-sm" (click)="clearAll()">Limpar tudo</button>
      </div>

      @if (filtersOpen()) {
        <div class="advanced-panel">
          <div class="advanced-grid">
            <div class="adv-field">
              <label>Marca</label>
              <oui-searchable-select [options]="brandOptions()" [(value)]="filters.brandExternalId" placeholder="Pesquisar marca..." />
            </div>
            <div class="adv-field">
              <label>Cor</label>
              <oui-searchable-select [options]="colorOptions()" [(value)]="filters.colorExternalId" placeholder="Pesquisar cor..." />
            </div>
            <div class="adv-field">
              <label>Tamanho</label>
              <select class="filter-select adv-select" [(ngModel)]="filters.size">
                <option value="">Todos</option>
                @for (s of sizes; track s) {
                  <option [value]="s">{{ s }}</option>
                }
              </select>
            </div>
            <div class="adv-field">
              <label>Categoria</label>
              <oui-searchable-select [options]="categoryOptions()" [(value)]="filters.categoryExternalId" placeholder="Pesquisar categoria..." />
            </div>
            <div class="adv-field">
              <label>Condição</label>
              <select class="filter-select adv-select" [(ngModel)]="filters.condition">
                <option value="">Todas</option>
                @for (c of conditions; track c.value) {
                  <option [value]="c.value">{{ c.label }}</option>
                }
              </select>
            </div>
            <div class="adv-field">
              <label>Tipo de Aquisição</label>
              <select class="filter-select adv-select" [(ngModel)]="filters.acquisitionType" (ngModelChange)="onAcqTypeChange()">
                <option value="">Todos</option>
                <option value="Consignment">Consignação</option>
                <option value="OwnPurchase">Compra Própria</option>
              </select>
            </div>
            @if (filters.acquisitionType !== 'OwnPurchase') {
              <div class="adv-field">
                <label>Fornecedor</label>
                <oui-searchable-select [options]="supplierOptions()" [(value)]="filters.supplierExternalId" placeholder="Pesquisar fornecedor..." />
              </div>
            }
            <div class="adv-field">
              <label>Preço (€)</label>
              <div class="range-row">
                <input type="number" class="filter-input range-input" placeholder="mín" min="0" step="0.01" [(ngModel)]="filters.minPrice" />
                <span class="range-sep">–</span>
                <input type="number" class="filter-input range-input" placeholder="máx" min="0" step="0.01" [(ngModel)]="filters.maxPrice" />
              </div>
            </div>
            <div class="adv-field">
              <label>Entrada (data)</label>
              <div class="range-row">
                <input type="date" class="filter-input range-input" [(ngModel)]="filters.createdFrom" />
                <span class="range-sep">–</span>
                <input type="date" class="filter-input range-input" [(ngModel)]="filters.createdTo" />
              </div>
            </div>
          </div>
          <div class="advanced-actions">
            <button class="btn btn-outline btn-sm" (click)="clearAdvanced()">Limpar</button>
            <button class="btn btn-primary btn-sm" (click)="applyFilters()">Aplicar</button>
          </div>
        </div>
      }

      @if (activeChips().length > 0) {
        <div class="chips-row">
          @for (chip of activeChips(); track chip.key) {
            <span class="chip">
              {{ chip.text }}
              <button class="chip-remove" (click)="removeChip(chip.key)" aria-label="Remover filtro">✕</button>
            </span>
          }
        </div>
      }
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
                <th class="sortable" (click)="sort('name')">Nome <span class="sort-icon">{{ sortIcon('name') }}</span></th>
                <th>Marca</th>
                <th>Tam</th>
                <th>Cor</th>
                <th class="sortable" (click)="sort('price')">Preço <span class="sort-icon">{{ sortIcon('price') }}</span></th>
                <th>Estado</th>
                @if (hasDaysInStock()) {
                  <th class="sortable" (click)="sort('days')">Dias <span class="sort-icon">{{ sortIcon('days') }}</span></th>
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
                    @if (item.status !== 'Sold') {
                      <button
                        *hasPermission="'inventory.items.delete'"
                        class="btn btn-danger btn-sm"
                        (click)="deleteItem(item)"
                        [disabled]="deleting()"
                      >Eliminar</button>
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

    /* ── Filters bar ── */
    .filters-bar {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 12px;
    }

    .filter-toggle {
      display: inline-flex;
      align-items: center;
      gap: 6px;
    }

    .filter-count {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-width: 18px;
      height: 18px;
      padding: 0 5px;
      border-radius: 9px;
      background: #6366f1;
      color: white;
      font-size: 11px;
      font-weight: 700;
    }

    .chevron { font-size: 11px; }

    .btn-ghost {
      background: transparent;
      color: #64748b;
      border-color: transparent;
    }

    .btn-ghost:hover { background: #f1f5f9; color: #1e293b; }

    /* ── Advanced panel ── */
    .advanced-panel {
      margin-top: 16px;
      padding-top: 16px;
      border-top: 1px solid #e2e8f0;
    }

    .advanced-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
      gap: 14px 16px;
    }

    .adv-field { display: flex; flex-direction: column; gap: 6px; }

    .adv-field label {
      font-size: 12px;
      font-weight: 600;
      color: #64748b;
    }

    .adv-select { width: 100%; height: 40px; }

    .range-row { display: flex; align-items: center; gap: 8px; }
    .range-input { width: 100%; }
    .range-sep { color: #94a3b8; }

    .advanced-actions {
      display: flex;
      justify-content: flex-end;
      gap: 10px;
      margin-top: 16px;
    }

    /* ── Active filter chips ── */
    .chips-row {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
      margin-top: 14px;
    }

    .chip {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 4px 6px 4px 12px;
      border-radius: 16px;
      background: #eef2ff;
      color: #4338ca;
      font-size: 12px;
      font-weight: 600;
      border: 1px solid #c7d2fe;
    }

    .chip-remove {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 16px;
      height: 16px;
      border: none;
      border-radius: 50%;
      background: transparent;
      color: #4338ca;
      cursor: pointer;
      font-size: 11px;
      line-height: 1;
    }

    .chip-remove:hover { background: #c7d2fe; }

    /* ── Sortable headers ── */
    th.sortable { cursor: pointer; user-select: none; white-space: nowrap; }
    th.sortable:hover { color: #6366f1; }
    .sort-icon { font-size: 11px; opacity: 0.7; }

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

    .btn-danger {
      background: white;
      color: #dc2626;
      border-color: #fecaca;
    }

    .btn-danger:hover:not(:disabled) {
      background: #fef2f2;
      border-color: #f87171;
    }

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
  private readonly brandService = inject(BrandService);
  private readonly categoryService = inject(CategoryService);
  private readonly supplierService = inject(SupplierService);
  private readonly colorService = inject(ColorService);
  private readonly baseUrl = environment.apiUrl.replace('/api', '');

  items = signal<ItemListItem[]>([]);
  loading = signal(false);
  publishing = signal(false);
  deleting = signal(false);
  currentPage = signal(1);
  totalPages = signal(1);
  totalCount = signal(0);

  // Top-bar filters (apply immediately)
  searchText = '';
  statusFilter = '';

  // Sorting
  sortBy = signal('');
  sortDir = signal<'asc' | 'desc'>('desc');

  // Advanced filters: draft (form-bound) + applied (used for query/chips)
  filtersOpen = signal(false);
  filters: AdvancedFilters = emptyFilters();
  applied = signal<AdvancedFilters>(emptyFilters());

  // Reference data
  brands = signal<BrandListItem[]>([]);
  categories = signal<CategoryListItem[]>([]);
  suppliers = signal<SupplierListItem[]>([]);
  colors = signal<ColorListItem[]>([]);

  readonly sizes = ['XXS', 'XS', 'S', 'M', 'L', 'XL', 'XXL', 'XXXL', '34', '36', '38', '40', '42', '44', '46', 'Único'];
  readonly conditions = [
    { value: 'Excellent', label: 'Excelente' },
    { value: 'VeryGood', label: 'Muito Bom' },
    { value: 'Good', label: 'Bom' },
    { value: 'Fair', label: 'Razoável' },
    { value: 'Poor', label: 'Mau' },
  ];

  private readonly pageSize = 20;

  brandOptions = computed<SearchableOption[]>(() =>
    this.brands().map(b => ({ value: b.externalId, label: b.name }))
  );
  categoryOptions = computed<SearchableOption[]>(() =>
    this.categories().map(c => ({ value: c.externalId, label: c.name }))
  );
  supplierOptions = computed<SearchableOption[]>(() =>
    this.suppliers().map(s => ({ value: s.externalId, label: s.name, sublabel: s.email, badge: s.initial }))
  );
  colorOptions = computed<SearchableOption[]>(() =>
    this.colors().map(c => ({ value: c.externalId, label: c.name }))
  );

  advancedCount = computed(() => {
    const a = this.applied();
    let n = 0;
    if (a.brandExternalId) n++;
    if (a.categoryExternalId) n++;
    if (a.supplierExternalId) n++;
    if (a.colorExternalId) n++;
    if (a.size) n++;
    if (a.condition) n++;
    if (a.acquisitionType) n++;
    if (a.minPrice != null || a.maxPrice != null) n++;
    if (a.createdFrom || a.createdTo) n++;
    return n;
  });

  activeChips = computed<FilterChip[]>(() => {
    const a = this.applied();
    const chips: FilterChip[] = [];

    if (a.brandExternalId) {
      chips.push({ key: 'brandExternalId', text: `Marca: ${this.labelOf(this.brandOptions(), a.brandExternalId)}` });
    }
    if (a.colorExternalId) {
      chips.push({ key: 'colorExternalId', text: `Cor: ${this.labelOf(this.colorOptions(), a.colorExternalId)}` });
    }
    if (a.size) {
      chips.push({ key: 'size', text: `Tamanho: ${a.size}` });
    }
    if (a.categoryExternalId) {
      chips.push({ key: 'categoryExternalId', text: `Categoria: ${this.labelOf(this.categoryOptions(), a.categoryExternalId)}` });
    }
    if (a.condition) {
      chips.push({ key: 'condition', text: `Condição: ${this.conditions.find(c => c.value === a.condition)?.label ?? a.condition}` });
    }
    if (a.acquisitionType) {
      chips.push({ key: 'acquisitionType', text: `Tipo: ${a.acquisitionType === 'Consignment' ? 'Consignação' : 'Compra Própria'}` });
    }
    if (a.supplierExternalId) {
      chips.push({ key: 'supplierExternalId', text: `Fornecedor: ${this.labelOf(this.supplierOptions(), a.supplierExternalId)}` });
    }
    if (a.minPrice != null || a.maxPrice != null) {
      const min = a.minPrice != null ? `€${a.minPrice}` : '';
      const max = a.maxPrice != null ? `€${a.maxPrice}` : '';
      const text = a.minPrice != null && a.maxPrice != null ? `${min}–${max}`
        : a.minPrice != null ? `≥ ${min}` : `≤ ${max}`;
      chips.push({ key: 'price', text: `Preço: ${text}` });
    }
    if (a.createdFrom || a.createdTo) {
      const text = a.createdFrom && a.createdTo ? `${a.createdFrom} – ${a.createdTo}`
        : a.createdFrom ? `desde ${a.createdFrom}` : `até ${a.createdTo}`;
      chips.push({ key: 'created', text: `Entrada: ${text}` });
    }

    return chips;
  });

  private labelOf(options: SearchableOption[], value: string): string {
    return options.find(o => o.value === value)?.label ?? value;
  }

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
    this.loadReferenceData();
    this.loadItems();
  }

  private loadReferenceData(): void {
    forkJoin({
      brands: this.brandService.getAll(),
      categories: this.categoryService.getAll(),
      suppliers: this.supplierService.getAll(),
      colors: this.colorService.getAll(),
    }).subscribe({
      next: ({ brands, categories, suppliers, colors }) => {
        this.brands.set(brands);
        this.categories.set(categories);
        this.suppliers.set(suppliers);
        this.colors.set(colors);
      },
      error: () => { /* reference dropdowns simply stay empty */ }
    });
  }

  loadItems(): void {
    this.loading.set(true);
    const a = this.applied();
    this.itemService.getItems({
      search: this.searchText || undefined,
      status: this.statusFilter || undefined,
      brandExternalId: a.brandExternalId || undefined,
      categoryExternalId: a.categoryExternalId || undefined,
      supplierExternalId: a.supplierExternalId || undefined,
      colorExternalId: a.colorExternalId || undefined,
      size: a.size || undefined,
      condition: a.condition || undefined,
      acquisitionType: a.acquisitionType || undefined,
      minPrice: a.minPrice ?? undefined,
      maxPrice: a.maxPrice ?? undefined,
      createdFrom: a.createdFrom || undefined,
      createdTo: a.createdTo || undefined,
      sortBy: this.sortBy() || undefined,
      sortDir: this.sortBy() ? this.sortDir() : undefined,
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

  onTopFilterChange(): void {
    this.currentPage.set(1);
    this.loadItems();
  }

  onAcqTypeChange(): void {
    // Supplier only applies to consignment items
    if (this.filters.acquisitionType === 'OwnPurchase') {
      this.filters.supplierExternalId = '';
    }
  }

  applyFilters(): void {
    this.applied.set({ ...this.filters });
    this.currentPage.set(1);
    this.loadItems();
  }

  clearAdvanced(): void {
    this.filters = emptyFilters();
    this.applied.set(emptyFilters());
    this.currentPage.set(1);
    this.loadItems();
  }

  clearAll(): void {
    this.searchText = '';
    this.statusFilter = '';
    this.sortBy.set('');
    this.sortDir.set('desc');
    this.clearAdvanced();
  }

  removeChip(key: FilterChip['key']): void {
    const next = { ...this.applied() };
    if (key === 'price') {
      next.minPrice = null;
      next.maxPrice = null;
      this.filters.minPrice = null;
      this.filters.maxPrice = null;
    } else if (key === 'created') {
      next.createdFrom = '';
      next.createdTo = '';
      this.filters.createdFrom = '';
      this.filters.createdTo = '';
    } else if (key === 'minPrice' || key === 'maxPrice') {
      next[key] = null;
      this.filters[key] = null;
    } else {
      next[key] = '';
      this.filters[key] = '';
    }
    this.applied.set(next);
    this.currentPage.set(1);
    this.loadItems();
  }

  sort(column: string): void {
    if (this.sortBy() === column) {
      this.sortDir.set(this.sortDir() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortBy.set(column);
      this.sortDir.set(column === 'name' ? 'asc' : 'desc');
    }
    this.currentPage.set(1);
    this.loadItems();
  }

  sortIcon(column: string): string {
    if (this.sortBy() !== column) return '↕';
    return this.sortDir() === 'asc' ? '▲' : '▼';
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

  deleteItem(item: ItemListItem): void {
    if (item.status === 'Sold') return;
    if (!confirm(`Eliminar "${item.name}"? Esta ação não pode ser anulada.`)) return;

    this.deleting.set(true);
    this.itemService.deleteItem(item.externalId).subscribe({
      next: () => {
        this.deleting.set(false);
        this.loadItems();
      },
      error: (err) => {
        this.deleting.set(false);
        const msg = err.error?.error || 'Erro ao eliminar item.';
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
