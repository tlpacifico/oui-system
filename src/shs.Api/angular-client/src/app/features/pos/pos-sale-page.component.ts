import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { PosService, RegisterResponse, ProcessSaleResponse } from './pos.service';
import { ItemService, ItemFilters } from '../inventory/services/item.service';
import { ItemListItem } from '../../core/models/item.model';
import { environment } from '../../../environments/environment';

interface CartItem {
  externalId: string;
  identificationNumber: string;
  name: string;
  brand: string;
  size: string;
  color: string;
  price: number;
  discount: number;
  photoUrl?: string;
}

@Component({
  selector: 'oui-pos-sale-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="pos-layout">
      <!-- LEFT: Search & Results -->
      <div class="pos-left">
        <div class="search-bar">
          <input
            #searchInput
            class="search-input"
            placeholder="Pesquisar item por nome, ID, marca..."
            [(ngModel)]="searchTerm"
            (ngModelChange)="onSearch()"
            (keydown.enter)="onSearch()"
            autofocus
          />
          <span class="search-hint">{{ searchResults().length }} resultados</span>
        </div>

        @if (searchLoading()) {
          <div class="search-loading">A pesquisar...</div>
        } @else if (searchResults().length === 0 && searchTerm) {
          <div class="search-empty">Nenhum item encontrado para "{{ searchTerm }}"</div>
        } @else {
          <div class="results-grid">
            @for (item of searchResults(); track item.externalId) {
              <div class="item-card" [class.in-cart]="isInCart(item.externalId)" (click)="addToCart(item)">
                <div class="item-card-top">
                  @if (item.primaryPhotoUrl) {
                    <img [src]="getPhotoUrl(item.primaryPhotoUrl)" [alt]="item.name" class="item-thumb" />
                  } @else {
                    <div class="item-thumb-placeholder">👕</div>
                  }
                  <div class="item-info">
                    <div class="item-name">{{ item.name }}</div>
                    <div class="item-meta">{{ item.brand }} · {{ item.size }} · {{ item.color }}</div>
                    <div class="item-id">{{ item.identificationNumber }}</div>
                  </div>
                </div>
                <div class="item-card-bottom">
                  <span class="item-price">{{ item.evaluatedPrice | currency: 'EUR' }}</span>
                  @if (isInCart(item.externalId)) {
                    <span class="in-cart-badge">No carrinho</span>
                  } @else {
                    <span class="add-label">+ Adicionar</span>
                  }
                </div>
              </div>
            }
          </div>
        }
      </div>

      <!-- RIGHT: Cart -->
      <div class="pos-right">
        <div class="cart-header">
          <h2>Carrinho</h2>
          <span class="cart-count">{{ cart().length }} {{ cart().length === 1 ? 'item' : 'itens' }}</span>
        </div>

        @if (cart().length === 0) {
          <div class="cart-empty">
            <span class="cart-empty-icon">🛒</span>
            <span>Pesquise e adicione itens</span>
          </div>
        } @else {
          <div class="cart-items">
            @for (item of cart(); track item.externalId; let idx = $index) {
              <div class="cart-item">
                <div class="cart-item-info">
                  <div class="cart-item-name">{{ item.name }}</div>
                  <div class="cart-item-meta">{{ item.brand }} · {{ item.size }}</div>
                </div>
                <div class="cart-item-price">{{ (item.price - item.discount) | currency: 'EUR' }}</div>
                <button class="cart-item-remove" (click)="removeFromCart(idx)" title="Remover">&times;</button>
              </div>
            }
          </div>

          <!-- Discount -->
          <div class="discount-section">
            <label>Desconto Global (%)</label>
            <div class="discount-row">
              <input
                type="number"
                class="form-control discount-input"
                [(ngModel)]="discountPercentage"
                min="0"
                max="100"
                step="1"
              />
              <span class="discount-amount">-{{ discountAmount() | currency: 'EUR' }}</span>
            </div>
            @if (discountPercentage > 10) {
              <input
                class="form-control"
                placeholder="Motivo do desconto (obrigatório)"
                [(ngModel)]="discountReason"
                style="margin-top:8px"
              />
            }
          </div>

          <!-- Totals -->
          <div class="totals">
            <div class="total-row">
              <span>Subtotal</span>
              <span>{{ subtotal() | currency: 'EUR' }}</span>
            </div>
            @if (discountPercentage > 0) {
              <div class="total-row discount-line">
                <span>Desconto ({{ discountPercentage }}%)</span>
                <span>-{{ discountAmount() | currency: 'EUR' }}</span>
              </div>
            }
            <div class="total-row total-final">
              <span>TOTAL</span>
              <span>{{ totalAmount() | currency: 'EUR' }}</span>
            </div>
          </div>

          <!-- Pay button -->
          <button class="btn btn-primary btn-pay" (click)="showPaymentDialog.set(true)">
            Finalizar Venda (F4)
          </button>
        }

        <!-- Bottom bar -->
        <div class="cart-footer">
          <a class="btn btn-outline btn-sm" routerLink="/pos">← Caixa</a>
          <span class="shortcut-hint">F4 Pagar · ESC Limpar</span>
        </div>
      </div>

      <!-- Payment Dialog -->
      @if (showPaymentDialog()) {
        <div class="overlay" (click)="showPaymentDialog.set(false)">
          <div class="dialog pay-dialog" (click)="$event.stopPropagation()">
            <h2>Pagamento</h2>
            <div class="pay-total">{{ totalAmount() | currency: 'EUR' }}</div>

            <div class="form-group">
              <label>Método de Pagamento</label>
              <select class="form-control" [(ngModel)]="paymentMethod">
                <option value="Cash">Dinheiro</option>
                <option value="CreditCard">Cartão Crédito</option>
                <option value="DebitCard">Cartão Débito</option>
                <option value="PIX">PIX</option>
                <option value="StoreCredit">Crédito Loja</option>
              </select>
            </div>

            <div class="form-group">
              <label>Valor (€)</label>
              <input
                type="number"
                class="form-control form-control-lg"
                [(ngModel)]="paymentAmount"
                min="0"
                step="0.01"
              />
            </div>

            @if (paymentMethod === 'Cash' && paymentAmount > totalAmount()) {
              <div class="change-display">
                <span>Troco:</span>
                <span class="change-value">{{ (paymentAmount - totalAmount()) | currency: 'EUR' }}</span>
              </div>
            }

            <div class="form-group">
              <label>Notas (opcional)</label>
              <input class="form-control" [(ngModel)]="saleNotes" placeholder="Observações..." />
            </div>

            @if (payError()) {
              <div class="alert alert-error">{{ payError() }}</div>
            }

            <div class="dialog-actions">
              <button class="btn btn-outline" (click)="showPaymentDialog.set(false)">Cancelar</button>
              <button
                class="btn btn-primary"
                (click)="confirmPayment()"
                [disabled]="paying()"
              >
                {{ paying() ? 'A processar...' : 'Confirmar Pagamento' }}
              </button>
            </div>
          </div>
        </div>
      }

      <!-- Sale success dialog -->
      @if (saleResult()) {
        <div class="overlay">
          <div class="dialog success-dialog">
            <div class="success-icon">&#10003;</div>
            <h2>Venda Concluída</h2>
            <div class="sale-number">{{ saleResult()!.saleNumber }}</div>
            <div class="sale-summary">
              <span>{{ saleResult()!.itemCount }} itens</span>
              <span class="sale-total">{{ saleResult()!.totalAmount | currency: 'EUR' }}</span>
            </div>
            @if (saleResult()!.change > 0) {
              <div class="change-display large">
                <span>Troco:</span>
                <span class="change-value">{{ saleResult()!.change | currency: 'EUR' }}</span>
              </div>
            }
            <div class="dialog-actions">
              <button class="btn btn-primary" (click)="newSale()">Nova Venda</button>
              <a class="btn btn-outline" [routerLink]="['/pos/sales']">Ver Vendas</a>
            </div>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    :host { display: block; height: calc(100vh - 64px); overflow: hidden; }
    .pos-layout { display: flex; height: 100%; gap: 0; }

    .pos-left { flex: 1; display: flex; flex-direction: column; overflow: hidden; padding: 16px; }
    .search-bar { display: flex; align-items: center; gap: 12px; margin-bottom: 12px; }
    .search-input { flex: 1; padding: 12px 16px; border: 2px solid #e2e8f0; border-radius: 10px; font-size: 15px; }
    .search-input:focus { outline: none; border-color: #6366f1; }
    .search-hint { font-size: 12px; color: #94a3b8; white-space: nowrap; }
    .search-loading, .search-empty { text-align: center; padding: 40px; color: #64748b; font-size: 14px; }

    .results-grid { flex: 1; overflow-y: auto; display: grid; grid-template-columns: repeat(auto-fill, minmax(260px, 1fr)); gap: 12px; align-content: start; }
    .item-card { background: #fff; border: 1px solid #e2e8f0; border-radius: 10px; padding: 12px; cursor: pointer; transition: all 0.15s; }
    .item-card:hover { border-color: #6366f1; box-shadow: 0 2px 8px rgba(99,102,241,0.1); }
    .item-card.in-cart { border-color: #22c55e; background: #f0fdf4; opacity: 0.7; }
    .item-card-top { display: flex; gap: 10px; margin-bottom: 8px; }
    .item-thumb { width: 48px; height: 48px; border-radius: 6px; object-fit: cover; }
    .item-thumb-placeholder { width: 48px; height: 48px; border-radius: 6px; background: #f1f5f9; display: flex; align-items: center; justify-content: center; font-size: 20px; }
    .item-info { flex: 1; min-width: 0; }
    .item-name { font-size: 13px; font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .item-meta { font-size: 11px; color: #64748b; }
    .item-id { font-size: 10px; color: #94a3b8; font-family: monospace; }
    .item-card-bottom { display: flex; justify-content: space-between; align-items: center; }
    .item-price { font-size: 16px; font-weight: 700; color: #1e293b; }
    .add-label { font-size: 11px; color: #6366f1; font-weight: 600; }
    .in-cart-badge { font-size: 10px; color: #16a34a; font-weight: 600; background: #dcfce7; padding: 2px 8px; border-radius: 12px; }

    .pos-right { width: 380px; background: #fff; border-left: 1px solid #e2e8f0; display: flex; flex-direction: column; }
    .cart-header { display: flex; justify-content: space-between; align-items: center; padding: 16px 20px; border-bottom: 1px solid #e2e8f0; }
    .cart-header h2 { font-size: 16px; margin: 0; }
    .cart-count { font-size: 12px; color: #6366f1; font-weight: 600; background: #eef2ff; padding: 2px 10px; border-radius: 12px; }

    .cart-empty { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 8px; color: #94a3b8; font-size: 13px; }
    .cart-empty-icon { font-size: 40px; }

    .cart-items { flex: 1; overflow-y: auto; padding: 8px 0; }
    .cart-item { display: flex; align-items: center; gap: 8px; padding: 10px 20px; border-bottom: 1px solid #f1f5f9; }
    .cart-item-info { flex: 1; min-width: 0; }
    .cart-item-name { font-size: 13px; font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .cart-item-meta { font-size: 11px; color: #64748b; }
    .cart-item-price { font-size: 14px; font-weight: 700; white-space: nowrap; }
    .cart-item-remove { background: none; border: none; font-size: 18px; color: #94a3b8; cursor: pointer; padding: 4px 8px; border-radius: 4px; }
    .cart-item-remove:hover { color: #ef4444; background: #fef2f2; }

    .discount-section { padding: 12px 20px; border-top: 1px solid #e2e8f0; }
    .discount-section label { font-size: 11px; font-weight: 600; color: #64748b; text-transform: uppercase; letter-spacing: 0.5px; }
    .discount-row { display: flex; align-items: center; gap: 8px; margin-top: 6px; }
    .discount-input { width: 80px; padding: 6px 8px; text-align: center; }
    .discount-amount { font-size: 13px; color: #ef4444; font-weight: 600; }

    .totals { padding: 12px 20px; border-top: 1px solid #e2e8f0; }
    .total-row { display: flex; justify-content: space-between; font-size: 13px; padding: 4px 0; }
    .discount-line { color: #ef4444; }
    .total-final { font-size: 20px; font-weight: 800; padding-top: 8px; border-top: 2px solid #1e293b; margin-top: 4px; }

    .btn-pay { margin: 12px 20px; padding: 14px; font-size: 15px; width: calc(100% - 40px); justify-content: center; background: #22c55e; border-color: #22c55e; color: #fff; }
    .btn-pay:hover { background: #16a34a; }

    .cart-footer { padding: 12px 20px; border-top: 1px solid #e2e8f0; display: flex; justify-content: space-between; align-items: center; }
    .shortcut-hint { font-size: 10px; color: #94a3b8; }

    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; font-size: 13px; font-weight: 600; color: #374151; margin-bottom: 6px; }
    .form-control { width: 100%; padding: 10px 12px; border: 1px solid #d1d5db; border-radius: 8px; font-size: 14px; font-family: inherit; box-sizing: border-box; }
    .form-control-lg { font-size: 20px; text-align: center; padding: 14px; }
    .form-control:focus { outline: none; border-color: #6366f1; box-shadow: 0 0 0 3px rgba(99,102,241,0.1); }

    .overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; }
    .dialog { background: #fff; border-radius: 16px; padding: 32px; max-width: 440px; width: 90%; }
    .dialog h2 { font-size: 20px; margin: 0 0 8px; }
    .dialog-actions { display: flex; gap: 12px; margin-top: 20px; }
    .dialog-actions .btn { flex: 1; justify-content: center; padding: 12px; }

    .pay-total { font-size: 36px; font-weight: 800; text-align: center; margin: 16px 0 24px; color: #1e293b; }
    .change-display { display: flex; justify-content: space-between; align-items: center; background: #f0fdf4; border: 1px solid #bbf7d0; border-radius: 8px; padding: 12px 16px; margin-bottom: 16px; }
    .change-display.large { margin: 16px 0; }
    .change-value { font-size: 20px; font-weight: 700; color: #16a34a; }

    .alert { padding: 12px 16px; border-radius: 8px; font-size: 13px; margin-bottom: 16px; }
    .alert-error { background: #fef2f2; color: #dc2626; border: 1px solid #fecaca; }

    .success-dialog { text-align: center; }
    .success-icon { width: 56px; height: 56px; border-radius: 50%; background: #f0fdf4; color: #22c55e; font-size: 28px; display: flex; align-items: center; justify-content: center; margin: 0 auto 16px; border: 2px solid #bbf7d0; }
    .sale-number { font-size: 13px; color: #64748b; font-family: monospace; margin-bottom: 16px; }
    .sale-summary { display: flex; justify-content: center; gap: 16px; align-items: center; margin-bottom: 8px; }
    .sale-summary span { font-size: 14px; color: #64748b; }
    .sale-total { font-size: 24px; font-weight: 800; color: #1e293b; }

    .btn { display: inline-flex; align-items: center; gap: 6px; padding: 8px 16px; border: 1px solid #d1d5db; border-radius: 8px; font-size: 13px; font-weight: 600; cursor: pointer; background: #fff; color: #374151; text-decoration: none; transition: all 0.15s; }
    .btn:hover { background: #f8fafc; }
    .btn:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-primary { background: #6366f1; color: #fff; border-color: #6366f1; }
    .btn-primary:hover { background: #4f46e5; }
    .btn-outline { background: transparent; }
    .btn-sm { padding: 6px 12px; font-size: 12px; }
  `],
  host: {
    '(document:keydown.f4)': 'onF4($event)',
    '(document:keydown.escape)': 'onEscape($event)',
  }
})
export class PosSalePageComponent implements OnInit {
  private readonly posService = inject(PosService);
  private readonly itemService = inject(ItemService);
  private readonly router = inject(Router);
  private readonly baseUrl = environment.apiUrl.replace('/api', '');

  register = signal<RegisterResponse | null>(null);
  searchTerm = '';
  searchResults = signal<ItemListItem[]>([]);
  searchLoading = signal(false);
  cart = signal<CartItem[]>([]);
  discountPercentage = 0;
  discountReason = '';

  showPaymentDialog = signal(false);
  paymentMethod = 'Cash';
  paymentAmount = 0;
  saleNotes = '';
  paying = signal(false);
  payError = signal<string | null>(null);
  saleResult = signal<ProcessSaleResponse | null>(null);

  private searchTimeout: any;

  subtotal = computed(() => this.cart().reduce((s, i) => s + i.price, 0));
  discountAmount = computed(() => this.subtotal() * this.discountPercentage / 100);
  totalAmount = computed(() => Math.max(0, this.subtotal() - this.discountAmount()));

  ngOnInit(): void {
    this.posService.getCurrentRegister().subscribe({
      next: (res) => {
        if (!res.open) {
          this.router.navigate(['/pos']);
          return;
        }
        this.register.set(res.register!);
      },
      error: () => this.router.navigate(['/pos']),
    });

    // Load initial items (ToSell)
    this.loadItems();
  }

  onSearch(): void {
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => this.loadItems(), 250);
  }

  private loadItems(): void {
    this.searchLoading.set(true);
    const filters: ItemFilters = {
      status: 'ToSell',
      search: this.searchTerm || undefined,
      pageSize: 50,
    };
    this.itemService.getItems(filters).subscribe({
      next: (result) => {
        this.searchResults.set(result.data);
        this.searchLoading.set(false);
      },
      error: () => this.searchLoading.set(false),
    });
  }

  isInCart(externalId: string): boolean {
    return this.cart().some(i => i.externalId === externalId);
  }

  addToCart(item: ItemListItem): void {
    if (this.isInCart(item.externalId)) return;
    const updated = [...this.cart(), {
      externalId: item.externalId,
      identificationNumber: item.identificationNumber,
      name: item.name,
      brand: item.brand,
      size: item.size,
      color: item.color,
      price: item.evaluatedPrice,
      discount: 0,
      photoUrl: item.primaryPhotoUrl,
    }];
    this.cart.set(updated);
  }

  removeFromCart(index: number): void {
    const updated = [...this.cart()];
    updated.splice(index, 1);
    this.cart.set(updated);
  }

  confirmPayment(): void {
    if (!this.register()) return;
    if (this.paymentAmount < this.totalAmount()) {
      this.payError.set('O valor do pagamento é insuficiente.');
      return;
    }
    if (this.discountPercentage > 10 && !this.discountReason.trim()) {
      this.payError.set('É necessário indicar o motivo do desconto superior a 10%.');
      return;
    }

    this.paying.set(true);
    this.payError.set(null);

    this.posService.processSale({
      cashRegisterId: this.register()!.externalId,
      items: this.cart().map(i => ({ itemExternalId: i.externalId, discountAmount: i.discount })),
      payments: [{ method: this.paymentMethod, amount: this.paymentAmount }],
      discountPercentage: this.discountPercentage > 0 ? this.discountPercentage : undefined,
      discountReason: this.discountReason.trim() || undefined,
      notes: this.saleNotes.trim() || undefined,
    }).subscribe({
      next: (result) => {
        this.paying.set(false);
        this.showPaymentDialog.set(false);
        this.saleResult.set(result);
      },
      error: (err) => {
        this.paying.set(false);
        this.payError.set(err.error?.error || 'Erro ao processar venda.');
      },
    });
  }

  newSale(): void {
    this.saleResult.set(null);
    this.cart.set([]);
    this.discountPercentage = 0;
    this.discountReason = '';
    this.paymentAmount = 0;
    this.saleNotes = '';
    this.searchTerm = '';
    this.loadItems();
  }

  getPhotoUrl(path?: string): string {
    if (!path) return '';
    return `${this.baseUrl}${path}`;
  }

  onF4(event: Event): void {
    event.preventDefault();
    if (this.cart().length > 0 && !this.showPaymentDialog() && !this.saleResult()) {
      this.paymentAmount = this.totalAmount();
      this.showPaymentDialog.set(true);
    }
  }

  onEscape(event: Event): void {
    if (this.showPaymentDialog()) {
      this.showPaymentDialog.set(false);
    } else if (this.saleResult()) {
      this.newSale();
    }
  }
}
