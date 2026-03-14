import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EcommerceService } from '../../inventory/services/ecommerce.service';
import { EcommerceOrder, EcommerceOrderDetail, EcommerceOrderStatus } from '../../../core/models/ecommerce.model';
import { HasPermissionDirective } from '../../../core/auth/directives/has-permission.directive';

@Component({
  selector: 'oui-ecommerce-orders-page',
  standalone: true,
  imports: [CommonModule, FormsModule, HasPermissionDirective],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Encomendas E-commerce</h1>
        <p class="page-subtitle">{{ totalCount() }} encomendas no total</p>
      </div>
    </div>

    <!-- Filters -->
    <div class="card filters-card">
      <div class="filters-bar">
        <select
          class="filter-input filter-select"
          [ngModel]="statusFilter()"
          (ngModelChange)="statusFilter.set($event); onSearch()"
        >
          <option value="">Todos os estados</option>
          <option value="Pending">Pendente</option>
          <option value="Confirmed">Confirmada</option>
          <option value="Completed">Concluída</option>
          <option value="Cancelled">Cancelada</option>
        </select>
        @if (statusFilter()) {
          <button class="btn btn-outline btn-sm" (click)="clearFilters()">Limpar</button>
        }
      </div>
    </div>

    @if (loading()) {
      <div class="state-message">A carregar...</div>
    } @else if (orders().length === 0) {
      <div class="state-message">
        @if (statusFilter()) {
          Nenhuma encomenda encontrada com o estado selecionado.
        } @else {
          Nenhuma encomenda registada.
        }
      </div>
    } @else {
      <div class="card table-card">
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Nº Encomenda</th>
                <th>Cliente</th>
                <th>Email</th>
                <th>Telefone</th>
                <th>Estado</th>
                <th>Total</th>
                <th>Itens</th>
                <th>Reservada em</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              @for (order of orders(); track order.externalId) {
                <tr class="clickable-row" (click)="openDetail(order)">
                  <td><b>{{ order.orderNumber }}</b></td>
                  <td>{{ order.customerName }}</td>
                  <td>{{ order.customerEmail }}</td>
                  <td>{{ order.customerPhone || '—' }}</td>
                  <td>
                    <span class="badge" [ngClass]="getStatusBadgeClass(order.status)">
                      {{ getStatusLabel(order.status) }}
                    </span>
                  </td>
                  <td>{{ order.totalAmount | currency: 'EUR' : 'symbol' : '1.2-2' }}</td>
                  <td>
                    <span class="badge badge-gray">{{ order.itemCount }}</span>
                  </td>
                  <td>{{ order.reservedAt | date: 'dd/MM/yyyy HH:mm' }}</td>
                  <td class="cell-actions" (click)="$event.stopPropagation()">
                    @if (order.status === 'Pending') {
                      <button
                        class="btn btn-outline btn-sm"
                        style="color: #16a34a; border-color: #16a34a;"
                        (click)="confirmOrderAction(order)"
                        *hasPermission="'ecommerce.orders.manage'"
                      >
                        Confirmar
                      </button>
                    }
                    @if (order.status === 'Pending' || order.status === 'Confirmed') {
                      <button
                        class="btn btn-outline btn-sm btn-danger-outline"
                        (click)="openCancelModal(order)"
                        *hasPermission="'ecommerce.orders.manage'"
                      >
                        Cancelar
                      </button>
                    }
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>

      <!-- Pagination -->
      @if (totalPages() > 1) {
        <div class="pagination">
          <button class="btn btn-outline btn-sm" [disabled]="currentPage() <= 1" (click)="goToPage(currentPage() - 1)">
            ← Anterior
          </button>
          <span class="pagination-info">Página {{ currentPage() }} de {{ totalPages() }}</span>
          <button class="btn btn-outline btn-sm" [disabled]="currentPage() >= totalPages()" (click)="goToPage(currentPage() + 1)">
            Seguinte →
          </button>
        </div>
      }
    }

    <!-- Order Detail Modal -->
    @if (showDetailModal()) {
      <div class="modal-overlay" (click)="closeDetailModal()"></div>
      <div class="modal">
        <div class="modal-header">
          <h2>Encomenda {{ orderDetail()?.orderNumber }}</h2>
          <button class="modal-close" (click)="closeDetailModal()">&times;</button>
        </div>
        @if (loadingDetail()) {
          <div class="modal-body">
            <div class="state-message">A carregar...</div>
          </div>
        } @else if (orderDetail()) {
          <div class="modal-body">
            <div class="detail-grid">
              <div class="detail-row">
                <span class="detail-label">Cliente</span>
                <span>{{ orderDetail()!.customerName }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Email</span>
                <span>{{ orderDetail()!.customerEmail }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Telefone</span>
                <span>{{ orderDetail()!.customerPhone || '—' }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Estado</span>
                <span class="badge" [ngClass]="getStatusBadgeClass(orderDetail()!.status)">
                  {{ getStatusLabel(orderDetail()!.status) }}
                </span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Total</span>
                <span><b>{{ orderDetail()!.totalAmount | currency: 'EUR' : 'symbol' : '1.2-2' }}</b></span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Reservada em</span>
                <span>{{ orderDetail()!.reservedAt | date: 'dd/MM/yyyy HH:mm' }}</span>
              </div>
              @if (orderDetail()!.expiresAt) {
                <div class="detail-row">
                  <span class="detail-label">Expira em</span>
                  <span>{{ orderDetail()!.expiresAt | date: 'dd/MM/yyyy HH:mm' }}</span>
                </div>
              }
              @if (orderDetail()!.confirmedAt) {
                <div class="detail-row">
                  <span class="detail-label">Confirmada em</span>
                  <span>{{ orderDetail()!.confirmedAt | date: 'dd/MM/yyyy HH:mm' }}</span>
                </div>
              }
              @if (orderDetail()!.completedAt) {
                <div class="detail-row">
                  <span class="detail-label">Concluída em</span>
                  <span>{{ orderDetail()!.completedAt | date: 'dd/MM/yyyy HH:mm' }}</span>
                </div>
              }
              @if (orderDetail()!.cancelledAt) {
                <div class="detail-row">
                  <span class="detail-label">Cancelada em</span>
                  <span>{{ orderDetail()!.cancelledAt | date: 'dd/MM/yyyy HH:mm' }}</span>
                </div>
              }
              @if (orderDetail()!.cancellationReason) {
                <div class="detail-row">
                  <span class="detail-label">Motivo cancelamento</span>
                  <span>{{ orderDetail()!.cancellationReason }}</span>
                </div>
              }
              @if (orderDetail()!.notes) {
                <div class="detail-row">
                  <span class="detail-label">Notas</span>
                  <span>{{ orderDetail()!.notes }}</span>
                </div>
              }
            </div>

            <h3 style="margin-top: 1.5rem; margin-bottom: 0.75rem; font-size: 1rem;">Itens ({{ orderDetail()!.items.length }})</h3>
            <div class="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Produto</th>
                    <th>Preço</th>
                  </tr>
                </thead>
                <tbody>
                  @for (item of orderDetail()!.items; track item.externalId) {
                    <tr>
                      <td>{{ item.productTitle }}</td>
                      <td>{{ item.price | currency: 'EUR' : 'symbol' : '1.2-2' }}</td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          </div>
        }
        <div class="modal-footer">
          <button class="btn btn-outline" (click)="closeDetailModal()">Fechar</button>
        </div>
      </div>
    }

    <!-- Cancel Modal -->
    @if (showCancelModal()) {
      <div class="modal-overlay" (click)="closeCancelModal()"></div>
      <div class="modal modal-sm">
        <div class="modal-header">
          <h2>Cancelar Encomenda</h2>
          <button class="modal-close" (click)="closeCancelModal()">&times;</button>
        </div>
        <div class="modal-body">
          <p>Tem a certeza que deseja cancelar a encomenda <strong>{{ orderToCancel()?.orderNumber }}</strong>?</p>
          <div class="form-group" style="margin-top: 1rem;">
            <label class="form-label">Motivo (opcional)</label>
            <textarea
              class="form-input"
              [ngModel]="cancelReason()"
              (ngModelChange)="cancelReason.set($event)"
              rows="3"
              placeholder="Motivo do cancelamento..."
            ></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-outline" (click)="closeCancelModal()">Voltar</button>
          <button class="btn btn-danger" (click)="cancelOrder()" [disabled]="cancelling()">
            {{ cancelling() ? 'A cancelar...' : 'Cancelar Encomenda' }}
          </button>
        </div>
      </div>
    }

    <!-- Confirm Order Modal -->
    @if (showConfirmModal()) {
      <div class="modal-overlay" (click)="closeConfirmModal()"></div>
      <div class="modal modal-sm">
        <div class="modal-header">
          <h2>Confirmar Encomenda</h2>
          <button class="modal-close" (click)="closeConfirmModal()">&times;</button>
        </div>
        <div class="modal-body">
          <p>Confirmar a encomenda <strong>{{ orderToConfirm()?.orderNumber }}</strong> de <strong>{{ orderToConfirm()?.customerName }}</strong>?</p>
        </div>
        <div class="modal-footer">
          <button class="btn btn-outline" (click)="closeConfirmModal()">Cancelar</button>
          <button class="btn btn-primary" (click)="confirmOrder()" [disabled]="confirming()">
            {{ confirming() ? 'A confirmar...' : 'Confirmar' }}
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    :host { display: block; }

    .detail-grid {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .detail-row {
      display: flex;
      gap: 1rem;
      align-items: baseline;
    }

    .detail-label {
      min-width: 160px;
      font-weight: 500;
      color: #64748b;
      font-size: 0.875rem;
    }

    .pagination {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 1rem;
      padding: 1rem 0;
    }

    .pagination-info {
      font-size: 0.875rem;
      color: #64748b;
    }
  `]
})
export class EcommerceOrdersPageComponent implements OnInit {
  private readonly ecommerceService = inject(EcommerceService);

  readonly orders = signal<EcommerceOrder[]>([]);
  readonly loading = signal(false);
  readonly statusFilter = signal('');
  readonly currentPage = signal(1);
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);

  // Detail modal
  readonly showDetailModal = signal(false);
  readonly loadingDetail = signal(false);
  readonly orderDetail = signal<EcommerceOrderDetail | null>(null);

  // Cancel modal
  readonly showCancelModal = signal(false);
  readonly orderToCancel = signal<EcommerceOrder | null>(null);
  readonly cancelReason = signal('');
  readonly cancelling = signal(false);

  // Confirm modal
  readonly showConfirmModal = signal(false);
  readonly orderToConfirm = signal<EcommerceOrder | null>(null);
  readonly confirming = signal(false);

  ngOnInit() {
    this.loadOrders();
  }

  loadOrders() {
    this.loading.set(true);
    const params: any = {
      page: this.currentPage(),
      pageSize: 20
    };
    if (this.statusFilter()) params.status = this.statusFilter();

    this.ecommerceService.getOrders(params).subscribe({
      next: (res) => {
        this.orders.set(res.items);
        this.totalCount.set(res.totalCount);
        this.totalPages.set(res.totalPages);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  onSearch() {
    this.currentPage.set(1);
    this.loadOrders();
  }

  clearFilters() {
    this.statusFilter.set('');
    this.currentPage.set(1);
    this.loadOrders();
  }

  goToPage(page: number) {
    this.currentPage.set(page);
    this.loadOrders();
  }

  getStatusLabel(status: string): string {
    const labels: Record<string, string> = {
      Pending: 'Pendente',
      Confirmed: 'Confirmada',
      Completed: 'Concluída',
      Cancelled: 'Cancelada'
    };
    return labels[status] || status;
  }

  getStatusBadgeClass(status: string): string {
    const classes: Record<string, string> = {
      Pending: 'badge-yellow',
      Confirmed: 'badge-green',
      Completed: 'badge-blue',
      Cancelled: 'badge-red'
    };
    return classes[status] || 'badge-gray';
  }

  // Detail modal
  openDetail(order: EcommerceOrder) {
    this.showDetailModal.set(true);
    this.loadingDetail.set(true);
    this.orderDetail.set(null);

    this.ecommerceService.getOrderById(order.externalId).subscribe({
      next: (detail) => {
        this.orderDetail.set(detail);
        this.loadingDetail.set(false);
      },
      error: () => {
        this.loadingDetail.set(false);
      }
    });
  }

  closeDetailModal() {
    this.showDetailModal.set(false);
    this.orderDetail.set(null);
  }

  // Confirm order
  confirmOrderAction(order: EcommerceOrder) {
    this.orderToConfirm.set(order);
    this.showConfirmModal.set(true);
  }

  closeConfirmModal() {
    this.showConfirmModal.set(false);
    this.orderToConfirm.set(null);
  }

  confirmOrder() {
    const order = this.orderToConfirm();
    if (!order) return;

    this.confirming.set(true);
    this.ecommerceService.confirmOrder(order.externalId).subscribe({
      next: () => {
        this.confirming.set(false);
        this.closeConfirmModal();
        this.loadOrders();
      },
      error: (err: { error?: { error?: string } }) => {
        this.confirming.set(false);
        alert(err.error?.error || 'Erro ao confirmar encomenda.');
      }
    });
  }

  // Cancel order
  openCancelModal(order: EcommerceOrder) {
    this.orderToCancel.set(order);
    this.cancelReason.set('');
    this.showCancelModal.set(true);
  }

  closeCancelModal() {
    this.showCancelModal.set(false);
    this.orderToCancel.set(null);
  }

  cancelOrder() {
    const order = this.orderToCancel();
    if (!order) return;

    this.cancelling.set(true);
    this.ecommerceService.cancelOrder(order.externalId, this.cancelReason() || undefined).subscribe({
      next: () => {
        this.cancelling.set(false);
        this.closeCancelModal();
        this.loadOrders();
      },
      error: (err: { error?: { error?: string } }) => {
        this.cancelling.set(false);
        alert(err.error?.error || 'Erro ao cancelar encomenda.');
      }
    });
  }
}
