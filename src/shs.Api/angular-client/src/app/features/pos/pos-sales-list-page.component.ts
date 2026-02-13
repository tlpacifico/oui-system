import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PosService, TodaySalesResponse, SaleListItem } from './pos.service';

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
                    <tr>
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

    .card { background: #fff; border: 1px solid #e2e8f0; border-radius: 12px; padding: 24px; margin-bottom: 20px; }
    .card-title { font-size: 15px; font-weight: 700; color: #1e293b; margin-bottom: 16px; }
    .card-title-bar { padding: 16px 16px 8px; }
    .table-card { padding: 0; overflow: hidden; }

    .method-card { }
    .method-grid { display: flex; flex-wrap: wrap; gap: 12px; }
    .method-item { display: flex; align-items: center; gap: 12px; padding: 12px 16px; background: #f8fafc; border-radius: 8px; flex: 1; min-width: 200px; }
    .method-icon { font-size: 24px; }
    .method-info { display: flex; flex-direction: column; flex: 1; }
    .method-name { font-size: 13px; font-weight: 600; }
    .method-count { font-size: 11px; color: #64748b; }
    .method-total { font-size: 16px; font-weight: 700; color: #1e293b; }

    .table-wrapper { overflow-x: auto; }
    table { width: 100%; border-collapse: collapse; font-size: 13px; }
    th { text-align: left; padding: 10px 16px; font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; color: #64748b; border-bottom: 2px solid #e2e8f0; white-space: nowrap; }
    td { padding: 12px 16px; border-bottom: 1px solid #f1f5f9; }
    .cell-mono { font-family: monospace; font-size: 12px; color: #6366f1; font-weight: 600; }
    .cell-center { text-align: center; }
    .cell-right { text-align: right; }
    .cell-bold { font-weight: 700; }
    tbody tr { transition: background 0.15s; }
    tbody tr:hover { background: #f8fafc; }

    .badge { display: inline-block; padding: 2px 8px; border-radius: 12px; font-size: 11px; font-weight: 600; }
    .badge-active { background: #f0fdf4; color: #16a34a; }
    .badge-voided { background: #fef2f2; color: #dc2626; }

    .empty { text-align: center; padding: 48px; }
    .empty-icon { font-size: 40px; }
    .empty h3 { font-size: 16px; margin: 16px 0 8px; }
    .empty p { font-size: 13px; color: #64748b; margin: 0 0 20px; }

    .btn { display: inline-flex; align-items: center; gap: 6px; padding: 8px 16px; border: 1px solid #d1d5db; border-radius: 8px; font-size: 13px; font-weight: 600; cursor: pointer; background: #fff; color: #374151; text-decoration: none; transition: all 0.15s; }
    .btn:hover { background: #f8fafc; }
    .btn-primary { background: #6366f1; color: #fff; border-color: #6366f1; }
    .btn-primary:hover { background: #4f46e5; }
    .btn-outline { background: transparent; }

    @media (max-width: 768px) { .stat-grid { grid-template-columns: repeat(2, 1fr); } }
  `]
})
export class PosSalesListPageComponent implements OnInit {
  private readonly posService = inject(PosService);

  data = signal<TodaySalesResponse | null>(null);
  loading = signal(true);
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
}
