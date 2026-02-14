import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ReportsService, SalesReport } from '../reports.service';

@Component({
  selector: 'oui-reports-sales-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page">
      <div class="page-header">
        <div>
          <h1>Relatório de Vendas</h1>
          <p class="subtitle">Análise de vendas por período</p>
        </div>
        <a class="btn btn-outline" routerLink="/reports">← Voltar</a>
      </div>

      <div class="card filters-card">
        <div class="form-row">
          <div class="form-group">
            <label>Data início</label>
            <input type="date" [(ngModel)]="startDate" (ngModelChange)="load()" class="form-control" />
          </div>
          <div class="form-group">
            <label>Data fim</label>
            <input type="date" [(ngModel)]="endDate" (ngModelChange)="load()" class="form-control" />
          </div>
          <div class="form-group">
            <button class="btn btn-primary" (click)="load()">Atualizar</button>
          </div>
        </div>
      </div>

      @if (loading()) {
        <div class="loading">A carregar...</div>
      } @else if (data()) {
        <div class="stat-grid">
          <div class="stat-card">
            <div class="stat-value">{{ data()!.revenue | currency: 'EUR' }}</div>
            <div class="stat-label">Receita Total</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ data()!.salesCount }}</div>
            <div class="stat-label">Vendas</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ data()!.avgTicket | currency: 'EUR' }}</div>
            <div class="stat-label">Ticket Médio</div>
          </div>
          <div class="stat-card">
            <div class="stat-value" [class.positive]="data()!.previousPeriodComparison.percentChange > 0" [class.negative]="data()!.previousPeriodComparison.percentChange < 0">
              {{ data()!.previousPeriodComparison.percentChange > 0 ? '+' : '' }}{{ data()!.previousPeriodComparison.percentChange | number: '1.1-1' }}%
            </div>
            <div class="stat-label">vs Período Anterior</div>
          </div>
        </div>

        <div class="chart-row">
          <div class="card chart-card">
            <div class="card-title">Vendas por Dia</div>
            @if (maxChartRevenue() > 0) {
              <div class="chart-bars">
                @for (p of data()!.dailySalesChart; track p.date) {
                  <div class="chart-bar-wrap">
                    <div class="chart-bar" [style.height.%]="(p.revenue / maxChartRevenue()) * 100"></div>
                  </div>
                }
              </div>
              <div class="chart-labels">
                @for (p of data()!.dailySalesChart; track p.date) {
                  <span class="chart-label">{{ formatDate(p.date) }}</span>
                }
              </div>
            } @else {
              <div class="chart-empty">Sem dados no período</div>
            }
          </div>
          <div class="card">
            <div class="card-title">Por Método de Pagamento</div>
            <ul class="breakdown-list">
              @for (key of paymentKeys(); track key) {
                <li>
                  <span>{{ getPaymentLabel(key) }}</span>
                  <span>{{ data()!.paymentBreakdown[key].total | currency: 'EUR' }} ({{ data()!.paymentBreakdown[key].count }})</span>
                </li>
              }
            </ul>
          </div>
        </div>

        <div class="card table-card">
          <div class="card-title">Top Marcas</div>
          @if (data()!.topBrands.length === 0) {
            <div class="empty">Sem dados</div>
          } @else {
            <table>
              <thead>
                <tr><th>Marca</th><th class="cell-right">Receita</th><th class="cell-center">Vendas</th></tr>
              </thead>
              <tbody>
                @for (b of data()!.topBrands; track b.brandName) {
                  <tr>
                    <td>{{ b.brandName }}</td>
                    <td class="cell-right">{{ b.revenue | currency: 'EUR' }}</td>
                    <td class="cell-center">{{ b.count }}</td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </div>

        @if (data()!.topCategories.length > 0) {
          <div class="card table-card">
            <div class="card-title">Top Categorias</div>
            <table>
              <thead>
                <tr><th>Categoria</th><th class="cell-right">Receita</th><th class="cell-center">Vendas</th></tr>
              </thead>
              <tbody>
                @for (c of data()!.topCategories; track c.categoryName) {
                  <tr>
                    <td>{{ c.categoryName }}</td>
                    <td class="cell-right">{{ c.revenue | currency: 'EUR' }}</td>
                    <td class="cell-center">{{ c.count }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .page { max-width: 1000px; margin: 0 auto; }
    .page-header { display: flex; justify-content: space-between; margin-bottom: 24px; }
    .subtitle { color: #64748b; margin: 4px 0 0; }
    .filters-card { margin-bottom: 24px; }
    .form-row { display: flex; gap: 16px; flex-wrap: wrap; align-items: flex-end; }
    .form-group label { display: block; font-size: 12px; margin-bottom: 4px; }
    .form-control { padding: 8px 12px; border: 1px solid #e2e8f0; border-radius: 6px; }
    .loading { text-align: center; padding: 48px; color: #64748b; }
    .stat-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin-bottom: 24px; }
    .stat-card { padding: 16px; border-radius: 8px; background: #fff; border: 1px solid #e2e8f0; text-align: center; }
    .stat-value { font-size: 18px; font-weight: 700; }
    .stat-value.positive { color: #059669; }
    .stat-value.negative { color: #dc2626; }
    .stat-label { font-size: 12px; color: #64748b; margin-top: 4px; }
    .chart-row { display: grid; grid-template-columns: 1fr 240px; gap: 24px; margin-bottom: 24px; }
    .chart-bars { display: flex; align-items: flex-end; gap: 4px; height: 120px; }
    .chart-bar-wrap { flex: 1; display: flex; justify-content: center; align-items: flex-end; }
    .chart-bar { width: 100%; max-width: 20px; min-height: 2px; background: #0f172a; border-radius: 4px 4px 0 0; }
    .chart-labels { display: flex; gap: 4px; margin-top: 8px; }
    .chart-label { flex: 1; font-size: 10px; color: #64748b; text-align: center; }
    .chart-empty { padding: 40px; text-align: center; color: #94a3b8; }
    .breakdown-list { list-style: none; margin: 0; padding: 0; }
    .breakdown-list li { display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #f1f5f9; }
    .cell-right { text-align: right; }
    .cell-center { text-align: center; }
    .empty { padding: 24px; text-align: center; color: #94a3b8; }
  `],
})
export class ReportsSalesPageComponent implements OnInit {
  private readonly reports = inject(ReportsService);

  data = signal<SalesReport | null>(null);
  loading = signal(true);
  startDate = '';
  endDate = '';

  paymentKeys = computed(() => {
    const d = this.data();
    return d?.paymentBreakdown ? Object.keys(d.paymentBreakdown) : [];
  });
  maxChartRevenue = computed(() => {
    const d = this.data();
    if (!d?.dailySalesChart?.length) return 0;
    return Math.max(...d.dailySalesChart.map((p) => p.revenue), 1);
  });

  ngOnInit(): void {
    const end = new Date();
    const start = new Date(end);
    start.setMonth(start.getMonth() - 1);
    this.startDate = start.toISOString().slice(0, 10);
    this.endDate = end.toISOString().slice(0, 10);
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.reports.getSalesReport(this.startDate, this.endDate).subscribe({
      next: (d) => { this.data.set(d); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  formatDate(s: string): string {
    return new Date(s).toLocaleDateString('pt-PT', { day: '2-digit', month: '2-digit' });
  }

  getPaymentLabel(key: string): string {
    const map: Record<string, string> = {
      Cash: 'Dinheiro', CreditCard: 'Cartão Crédito', DebitCard: 'Cartão Débito',
      PIX: 'PIX', StoreCredit: 'Crédito Loja',
    };
    return map[key] ?? key;
  }
}
