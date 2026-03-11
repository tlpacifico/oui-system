import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PosService, TodaySalesResponse, SaleListItem, SaleDetail } from './pos.service';

@Component({
  selector: 'oui-pos-sales-list-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="page">
      <div class="page-header">
        <div>
          <h1>Vendas de Hoje</h1>
          <p class="subtitle">Resumo e lista de vendas do dia</p>
        </div>
        <div class="header-actions">
          <a class="btn btn-outline" routerLink="/pos">Caixa</a>
          <a class="btn btn-primary" routerLink="/pos/sale">Nova Venda</a>
        </div>
      </div>

      @if (loading()) {
        <div class="loading">A carregar...</div>
      } @else if (data()) {
        <!-- KPIs -->
        <div class="stat-grid">
          <div class="stat-card">
            <div class="stat-value">{{ data()!.salesCount }}</div>
            <div class="stat-label">Vendas</div>
          </div>
          <div class="stat-card stat-revenue">
            <div class="stat-value">{{ data()!.totalRevenue | currency: 'EUR' }}</div>
            <div class="stat-label">Faturação Total</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ data()!.averageTicket | currency: 'EUR' }}</div>
            <div class="stat-label">Ticket Médio</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ data()!.totalItems }}</div>
            <div class="stat-label">Itens Vendidos</div>
          </div>
        </div>

        <!-- Payment breakdown -->
        @if (objectKeys(data()!.byPaymentMethod).length > 0) {
          <div class="card method-card">
            <div class="card-title">Por Método de Pagamento</div>
            <div class="method-grid">
              @for (key of objectKeys(data()!.byPaymentMethod); track key) {
                <div class="method-item">
                  <span class="method-icon">{{ getMethodIcon(key) }}</span>
                  <div class="method-info">
                    <span class="method-name">{{ getPaymentLabel(key) }}</span>
                    <span class="method-count">{{ data()!.byPaymentMethod[key].count }} vendas</span>
                  </div>
                  <span class="method-total">{{ data()!.byPaymentMethod[key].total | currency: 'EUR' }}</span>
                </div>
              }
            </div>
          </div>
        }

        <!-- Sales table -->
        @if (data()!.recentSales.length > 0) {
          <div class="card table-card">
            <div class="card-title-bar">
              <span class="card-title">Vendas Recentes</span>
            </div>
            <div class="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>N.º Venda</th>
                    <th>Hora</th>
                    <th class="cell-center">Itens</th>
                    <th class="cell-right">Total</th>
                    <th>Pagamento</th>
                    <th>Estado</th>
                  </tr>
                </thead>
                <tbody>
                  @for (sale of data()!.recentSales; track sale.externalId) {
                    <tr class="row-clickable" (click)="openSaleDetail(sale)">
                      <td class="cell-mono">{{ sale.saleNumber }}</td>
                      <td>{{ sale.saleDate | date: 'HH:mm' }}</td>
                      <td class="cell-center">{{ sale.itemCount }}</td>
                      <td class="cell-right cell-bold">{{ sale.totalAmount | currency: 'EUR' }}</td>
                      <td>{{ sale.paymentMethods }}</td>
                      <td>
                        <span class="badge" [ngClass]="getStatusClass(sale.status)">
                          {{ getStatusLabel(sale.status) }}
                        </span>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          </div>
        } @else {
          <div class="card empty">
            <span class="empty-icon">📋</span>
            <h3>Sem vendas hoje</h3>
            <p>Ainda não foram registadas vendas hoje.</p>
            <a class="btn btn-primary" routerLink="/pos/sale">Iniciar Venda</a>
          </div>
        }
      }

      <!-- Sale Detail Modal -->
      @if (selectedSale()) {
        <div class="overlay" (click)="closeSaleDetail()">
          <div class="modal" (click)="$event.stopPropagation()">
            <div class="modal-header">
              <h2>Venda {{ selectedSale()!.saleNumber }}</h2>
              <button class="btn-close" (click)="closeSaleDetail()">&times;</button>
            </div>

            @if (loadingDetail()) {
              <div class="loading">A carregar detalhes...</div>
            } @else {
              <div class="modal-body">
                <!-- Sale info -->
                <div class="detail-grid">
                  <div class="detail-item">
                    <span class="detail-label">Data</span>
                    <span class="detail-value">{{ selectedSale()!.saleDate | date: 'dd/MM/yyyy HH:mm' }}</span>
                  </div>
                  <div class="detail-item">
                    <span class="detail-label">Operador</span>
                    <span class="detail-value">{{ selectedSale()!.cashierName }}</span>
                  </div>
                  <div class="detail-item">
                    <span class="detail-label">Caixa</span>
                    <span class="detail-value">#{{ selectedSale()!.registerNumber }}</span>
                  </div>
                  <div class="detail-item">
                    <span class="detail-label">Estado</span>
                    <span class="badge" [ngClass]="getStatusClass(selectedSale()!.status)">
                      {{ getStatusLabel(selectedSale()!.status) }}
                    </span>
                  </div>
                </div>

                <!-- Items table -->
                <div class="section-title">Itens ({{ selectedSale()!.items.length }})</div>
                <div class="table-wrapper">
                  <table class="detail-table">
                    <thead>
                      <tr>
                        <th>Ref.</th>
                        <th>Artigo</th>
                        <th>Marca</th>
                        <th>Tam.</th>
                        <th>Cor</th>
                        <th>Fornecedor</th>
                        <th class="cell-right">Preço</th>
                        <th class="cell-right">Desc.</th>
                        <th class="cell-right">Final</th>
                      </tr>
                    </thead>
                    <tbody>
                      @for (item of selectedSale()!.items; track item.itemExternalId) {
                        <tr>
                          <td class="cell-mono">{{ item.identificationNumber }}</td>
                          <td>{{ item.name }}</td>
                          <td>{{ item.brand }}</td>
                          <td>{{ item.size }}</td>
                          <td>{{ item.color }}</td>
                          <td>{{ item.supplierName || '—' }}</td>
                          <td class="cell-right">{{ item.unitPrice | currency: 'EUR' }}</td>
                          <td class="cell-right">{{ item.discountAmount > 0 ? (item.discountAmount | currency: 'EUR') : '—' }}</td>
                          <td class="cell-right cell-bold">{{ item.finalPrice | currency: 'EUR' }}</td>
                        </tr>
                      }
                    </tbody>
                  </table>
                </div>

                <!-- Payments -->
                <div class="section-title">Pagamentos</div>
                <div class="payments-list">
                  @for (p of selectedSale()!.payments; track $index) {
                    <div class="payment-row">
                      <span>{{ getPaymentLabel(p.method) }}</span>
                      <span class="cell-bold">{{ p.amount | currency: 'EUR' }}</span>
                    </div>
                  }
                </div>

                <!-- Totals -->
                <div class="totals">
                  <div class="total-row">
                    <span>Subtotal</span>
                    <span>{{ selectedSale()!.subtotal | currency: 'EUR' }}</span>
                  </div>
                  @if (selectedSale()!.discountAmount > 0) {
                    <div class="total-row discount-row">
                      <span>Desconto ({{ selectedSale()!.discountPercentage }}%)</span>
                      <span>-{{ selectedSale()!.discountAmount | currency: 'EUR' }}</span>
                    </div>
                  }
                  <div class="total-row total-final">
                    <span>Total</span>
                    <span>{{ selectedSale()!.totalAmount | currency: 'EUR' }}</span>
                  </div>
                </div>

                @if (selectedSale()!.notes) {
                  <div class="notes-section">
                    <span class="detail-label">Notas:</span> {{ selectedSale()!.notes }}
                  </div>
                }
              </div>
            }
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .page { max-width: 1000px; margin: 0 auto; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 24px; }
    .page-header h1 { font-size: 22px; font-weight: 700; margin: 0; }
    .subtitle { font-size: 13px; color: #64748b; margin: 4px 0 0; }
    .header-actions { display: flex; gap: 8px; }
    .loading { text-align: center; padding: 48px; color: #64748b; }

    .stat-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin-bottom: 24px; }
    .stat-card { background: #fff; border: 1px solid #e2e8f0; border-radius: 12px; padding: 24px; text-align: center; }
    .stat-value { font-size: 28px; font-weight: 700; color: #1e293b; }
    .stat-label { font-size: 12px; color: #64748b; margin-top: 4px; }
    .stat-revenue { border-color: #6366f1; background: #eef2ff; }

    .card { padding: 24px; margin-bottom: 20px; }
    .card-title { font-size: 15px; font-weight: 700; color: #1e293b; margin-bottom: 16px; }
    .card-title-bar { padding: 16px 16px 8px; }
    .table-card { padding: 0; overflow: hidden; }

    .method-grid { display: flex; flex-wrap: wrap; gap: 12px; }
    .method-item { display: flex; align-items: center; gap: 12px; padding: 12px 16px; background: #f8fafc; border-radius: 8px; flex: 1; min-width: 200px; }
    .method-icon { font-size: 24px; }
    .method-info { display: flex; flex-direction: column; flex: 1; }
    .method-name { font-size: 13px; font-weight: 600; }
    .method-count { font-size: 11px; color: #64748b; }
    .method-total { font-size: 16px; font-weight: 700; color: #1e293b; }

    th { border-bottom: 2px solid #e2e8f0; white-space: nowrap; padding: 10px 16px; }
    td { padding: 12px 16px; }
    .cell-mono { color: #6366f1; font-weight: 600; }
    tbody tr { transition: background 0.15s; }
    tbody tr:hover { background: #f8fafc; }
    .row-clickable { cursor: pointer; }

    .badge-active { background: #f0fdf4; color: #16a34a; }
    .badge-voided { background: #fef2f2; color: #dc2626; }

    .empty { text-align: center; padding: 48px; }
    .empty-icon { font-size: 40px; }
    .empty h3 { font-size: 16px; margin: 16px 0 8px; }
    .empty p { font-size: 13px; color: #64748b; margin: 0 0 20px; }

    /* Modal */
    .overlay { position: fixed; inset: 0; background: rgba(0,0,0,.45); display: flex; align-items: center; justify-content: center; z-index: 1000; }
    .modal { position: static; transform: none; border-radius: 16px; width: 90%; max-width: 860px; max-height: 90vh; display: flex; flex-direction: column; box-shadow: 0 20px 60px rgba(0,0,0,.25); }
    .modal-header { padding: 20px 24px; }
    .modal-header h2 { font-size: 18px; }
    .btn-close { background: none; border: none; font-size: 24px; cursor: pointer; color: #64748b; padding: 0 4px; line-height: 1; }
    .btn-close:hover { color: #1e293b; }
    .modal-body { padding: 24px; overflow-y: auto; }

    .detail-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin-bottom: 24px; }
    .detail-item { display: flex; flex-direction: column; gap: 4px; }
    .detail-label { font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; color: #64748b; }
    .detail-value { font-size: 14px; font-weight: 600; color: #1e293b; }

    .section-title { font-size: 14px; font-weight: 700; color: #1e293b; margin: 20px 0 12px; padding-bottom: 8px; border-bottom: 1px solid #e2e8f0; }
    .detail-table th { font-size: 10px; padding: 8px 10px; }
    .detail-table td { padding: 8px 10px; font-size: 12px; }

    .payments-list { display: flex; flex-direction: column; gap: 8px; margin-bottom: 20px; }
    .payment-row { display: flex; justify-content: space-between; padding: 8px 12px; background: #f8fafc; border-radius: 8px; font-size: 13px; }

    .totals { border-top: 2px solid #e2e8f0; padding-top: 12px; margin-top: 8px; }
    .total-row { display: flex; justify-content: space-between; padding: 4px 0; font-size: 13px; color: #64748b; }
    .discount-row { color: #dc2626; }
    .total-final { font-size: 16px; font-weight: 700; color: #1e293b; padding-top: 8px; border-top: 1px solid #e2e8f0; margin-top: 4px; }

    .notes-section { margin-top: 16px; padding: 12px; background: #f8fafc; border-radius: 8px; font-size: 13px; color: #475569; }

    @media (max-width: 768px) {
      .stat-grid { grid-template-columns: repeat(2, 1fr); }
      .detail-grid { grid-template-columns: repeat(2, 1fr); }
    }
  `]
})
export class PosSalesListPageComponent implements OnInit {
  private readonly posService = inject(PosService);

  data = signal<TodaySalesResponse | null>(null);
  loading = signal(true);
  selectedSale = signal<SaleDetail | null>(null);
  loadingDetail = signal(false);
  objectKeys = Object.keys;

  ngOnInit(): void {
    this.posService.getTodaySales().subscribe({
      next: (d) => {
        this.data.set(d);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  getPaymentLabel(method: string): string {
    const labels: Record<string, string> = {
      Cash: 'Dinheiro', CreditCard: 'Cartao Credito', DebitCard: 'Cartao Debito',
      PIX: 'PIX', StoreCredit: 'Credito Loja',
    };
    return labels[method] || method;
  }

  getMethodIcon(method: string): string {
    const icons: Record<string, string> = {
      Cash: '💵', CreditCard: '💳', DebitCard: '💳', PIX: '📱', StoreCredit: '🎫',
    };
    return icons[method] || '💰';
  }

  getStatusLabel(status: string): string {
    const labels: Record<string, string> = {
      Active: 'Ativa', Voided: 'Anulada', PartialReturn: 'Dev. Parcial', FullReturn: 'Devolvida',
    };
    return labels[status] || status;
  }

  getStatusClass(status: string): string {
    return status === 'Active' ? 'badge-active' : 'badge-voided';
  }

  openSaleDetail(sale: SaleListItem): void {
    this.loadingDetail.set(true);
    this.selectedSale.set({ saleNumber: sale.saleNumber } as SaleDetail);
    this.posService.getSaleById(sale.externalId).subscribe({
      next: (detail) => {
        this.selectedSale.set(detail);
        this.loadingDetail.set(false);
      },
      error: () => this.loadingDetail.set(false),
    });
  }

  closeSaleDetail(): void {
    this.selectedSale.set(null);
  }
}
