import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DashboardService, DashboardData } from './dashboard.service';

@Component({
  selector: 'oui-dashboard-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page">
      <div class="page-header">
        <div>
          <h1>Dashboard</h1>
          <p class="subtitle">Visão geral do negócio</p>
        </div>
        <div class="header-actions">
          <select [(ngModel)]="period" (ngModelChange)="load()" class="period-select">
            <option value="today">Hoje</option>
            <option value="week">7 dias</option>
            <option value="month">30 dias</option>
          </select>
          <a class="btn btn-primary" routerLink="/pos/sale">Nova Venda</a>
        </div>
      </div>

      @if (loading()) {
        <div class="loading">A carregar...</div>
      } @else if (data()) {
        <!-- KPI Cards -->
        <div class="stat-grid">
          <div class="stat-card">
            <div class="stat-value">{{ data()!.salesToday.count }}</div>
            <div class="stat-label">Vendas Hoje</div>
          </div>
          <div class="stat-card stat-revenue">
            <div class="stat-value">{{ data()!.salesToday.revenue | currency: 'EUR' }}</div>
            <div class="stat-label">Faturação Hoje</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ data()!.salesMonth.revenue | currency: 'EUR' }}</div>
            <div class="stat-label">Faturação Mês</div>
            @if (data()!.salesMonth.growthPercent !== 0) {
              <span class="growth" [class.positive]="data()!.salesMonth.growthPercent > 0" [class.negative]="data()!.salesMonth.growthPercent < 0">
                {{ data()!.salesMonth.growthPercent > 0 ? '+' : '' }}{{ data()!.salesMonth.growthPercent | number: '1.1-1' }}%
              </span>
            }
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ data()!.inventory.totalItems }}</div>
            <div class="stat-label">Itens em Stock</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ data()!.inventory.totalValue | currency: 'EUR' }}</div>
            <div class="stat-label">Valor Stock</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ data()!.pendingSettlements.totalAmount | currency: 'EUR' }}</div>
            <div class="stat-label">Acertos Pendentes</div>
            <span class="stat-meta">{{ data()!.pendingSettlements.suppliersCount }} fornecedores</span>
          </div>
        </div>

        <!-- Chart + Top Sales -->
        <div class="chart-row">
          <div class="card chart-card">
            <div class="card-title">Vendas (últimos {{ chartDays() }} dias)</div>
            <div class="chart-container">
              @if (maxChartRevenue() > 0) {
                <div class="chart-bars">
                  @for (point of data()!.salesChart; track point.date) {
                    <div class="chart-bar-wrap">
                      <div
                        class="chart-bar"
                        [style.height.%]="(point.revenue / maxChartRevenue()) * 100"
                        [title]="point.date + ': ' + (point.revenue | currency: 'EUR')"
                      ></div>
                    </div>
                  }
                </div>
                <div class="chart-labels">
                  @for (point of data()!.salesChart; track point.date) {
                    <span class="chart-label">{{ formatChartDate(point.date) }}</span>
                  }
                </div>
              } @else {
                <div class="chart-empty">Sem vendas no período</div>
              }
            </div>
          </div>
          <div class="card top-sales-card">
            <div class="card-title">Top 5 Vendas (esta semana)</div>
            @if (data()!.topSellingItems.length === 0) {
              <div class="empty-state">Sem vendas esta semana</div>
            } @else {
              <ul class="top-sales-list">
                @for (item of data()!.topSellingItems; track item.name + item.soldDate) {
                  <li>
                    <span class="item-name">{{ item.name }}</span>
                    <span class="item-meta">{{ item.brand }} · {{ item.finalPrice | currency: 'EUR' }}</span>
                  </li>
                }
              </ul>
            }
          </div>
        </div>

        <!-- Alerts -->
        @if (hasAlerts()) {
          <div class="card alerts-card">
            <div class="card-title">Alertas</div>
            <div class="alerts-grid">
              @if (data()!.alerts.expiringConsignments > 0) {
                <a class="alert-item alert-warning" routerLink="/consignments/returns">
                  <span class="alert-icon">⚠️</span>
                  {{ data()!.alerts.expiringConsignments }} consignação(ões) a expirar (7 dias)
                </a>
              }
              @if (data()!.alerts.stagnantItems30 > 0) {
                <a class="alert-item alert-yellow" routerLink="/inventory/items">
                  <span class="alert-icon">📦</span>
                  {{ data()!.alerts.stagnantItems30 }} itens parados 30+ dias
                </a>
              }
              @if (data()!.alerts.stagnantItems45 > 0) {
                <a class="alert-item alert-orange" routerLink="/inventory/items">
                  <span class="alert-icon">📦</span>
                  {{ data()!.alerts.stagnantItems45 }} itens parados 45+ dias
                </a>
              }
              @if (data()!.alerts.stagnantItems60 > 0) {
                <a class="alert-item alert-red" routerLink="/inventory/items">
                  <span class="alert-icon">📦</span>
                  {{ data()!.alerts.stagnantItems60 }} itens parados 60+ dias
                </a>
              }
              @if (data()!.alerts.openRegisters.length > 0) {
                <div class="alert-item alert-info">
                  <span class="alert-icon">💰</span>
                  {{ data()!.alerts.openRegisters.length }} caixa(s) aberto(s):
                  @for (r of data()!.alerts.openRegisters; track r.operatorName) {
                    <span>{{ r.operatorName }}{{ $last ? '' : ', ' }}</span>
                  }
                </div>
              }
            </div>
          </div>
        }

        <!-- Quick Actions -->
        <div class="quick-actions">
          <a class="btn btn-outline" routerLink="/pos/sale">Nova Venda</a>
          <a class="btn btn-outline" routerLink="/consignments/receive">Nova Recepção</a>
          <a class="btn btn-outline" routerLink="/inventory/items">Buscar Item</a>
          <a class="btn btn-outline" routerLink="/finance/settlements">Acertos</a>
        </div>
      }
    </div>
  `,
  styles: [`
    .page { max-width: 1100px; margin: 0 auto; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 24px; }
    .page-header h1 { font-size: 22px; font-weight: 700; margin: 0; }
    .subtitle { color: #64748b; margin: 4px 0 0; font-size: 14px; }
    .header-actions { display: flex; gap: 12px; align-items: center; }
    .period-select { padding: 8px 12px; border: 1px solid #e2e8f0; border-radius: 6px; font-size: 14px; }
    .loading { text-align: center; padding: 48px; color: #64748b; }
    .stat-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(160px, 1fr)); gap: 16px; margin-bottom: 24px; }
    .stat-card { padding: 16px; border-radius: 8px; background: #fff; border: 1px solid #e2e8f0; text-align: center; }
    .stat-card.stat-revenue { border-color: #0f172a; background: #f8fafc; }
    .stat-value { font-size: 20px; font-weight: 700; }
    .stat-label { font-size: 12px; color: #64748b; margin-top: 4px; }
    .stat-meta { font-size: 11px; color: #94a3b8; display: block; margin-top: 2px; }
    .growth { font-size: 12px; font-weight: 500; margin-top: 4px; display: block; }
    .growth.positive { color: #059669; }
    .growth.negative { color: #dc2626; }
    .chart-row { display: grid; grid-template-columns: 1fr 280px; gap: 24px; margin-bottom: 24px; }
    @media (max-width: 768px) { .chart-row { grid-template-columns: 1fr; } }
    .chart-card { min-height: 200px; }
    .chart-container { padding: 16px 0; min-height: 160px; }
    .chart-bars { display: flex; align-items: flex-end; gap: 4px; height: 120px; }
    .chart-bar-wrap { flex: 1; display: flex; justify-content: center; align-items: flex-end; }
    .chart-bar { width: 100%; max-width: 24px; min-height: 2px; background: #0f172a; border-radius: 4px 4px 0 0; transition: height 0.2s; }
    .chart-bar:hover { background: #334155; }
    .chart-labels { display: flex; gap: 4px; margin-top: 8px; }
    .chart-label { flex: 1; font-size: 10px; color: #64748b; text-align: center; overflow: hidden; text-overflow: ellipsis; }
    .chart-empty { text-align: center; color: #94a3b8; padding: 40px; }
    .top-sales-list { list-style: none; margin: 0; padding: 0; }
    .top-sales-list li { padding: 8px 0; border-bottom: 1px solid #f1f5f9; display: flex; flex-direction: column; gap: 2px; }
    .top-sales-list li:last-child { border-bottom: none; }
    .item-name { font-weight: 500; font-size: 14px; }
    .item-meta { font-size: 12px; color: #64748b; }
    .empty-state { padding: 24px; text-align: center; color: #94a3b8; }
    .alerts-grid { display: flex; flex-direction: column; gap: 8px; }
    .alert-item { display: flex; align-items: center; gap: 8px; padding: 12px 16px; border-radius: 6px; text-decoration: none; color: inherit; }
    .alert-warning { background: #fef3c7; color: #92400e; }
    .alert-yellow { background: #fef9c3; color: #854d0e; }
    .alert-orange { background: #ffedd5; color: #c2410c; }
    .alert-red { background: #fee2e2; color: #991b1b; }
    .alert-info { background: #e0f2fe; color: #0369a1; }
    .quick-actions { display: flex; gap: 12px; flex-wrap: wrap; }
  `],
})
export class DashboardPageComponent implements OnInit {
  private readonly dashboard = inject(DashboardService);

  data = signal<DashboardData | null>(null);
  loading = signal(true);
  period: 'today' | 'week' | 'month' = 'today';

  chartDays = computed(() => (this.period === 'month' ? 30 : 7));
  maxChartRevenue = computed(() => {
    const d = this.data();
    if (!d?.salesChart?.length) return 0;
    return Math.max(...d.salesChart.map((p) => p.revenue), 1);
  });
  hasAlerts = computed(() => {
    const d = this.data();
    if (!d?.alerts) return false;
    const a = d.alerts;
    return (
      (a.expiringConsignments ?? 0) > 0 ||
      (a.stagnantItems30 ?? 0) > 0 ||
      (a.stagnantItems45 ?? 0) > 0 ||
      (a.stagnantItems60 ?? 0) > 0 ||
      (a.openRegisters?.length ?? 0) > 0
    );
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.dashboard.getDashboard(this.period).subscribe({
      next: (d) => {
        this.data.set(d);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  formatChartDate(dateStr: string): string {
    const d = new Date(dateStr);
    return d.toLocaleDateString('pt-PT', { day: '2-digit', month: '2-digit' });
  }
}
