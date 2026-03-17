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
    <div class="dashboard-layout">
      <!-- Header -->
      <div class="dashboard-header">
        <div class="header-left">
          <h1>Dashboard</h1>
          <p class="subtitle">Visão geral do negócio</p>
        </div>
        <div class="header-actions">
          <select
            [(ngModel)]="period"
            (ngModelChange)="load()"
            class="form-control period-select"
          >
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
        <div class="dashboard-content">
        <!-- Main content area -->
        <div class="dashboard-main">
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

          <!-- Chart Card -->
          <div class="card chart-card">
            <div class="card-header">
              <h2 class="card-title">Vendas (últimos {{ chartDays() }} dias)</h2>
            </div>
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
        </div>

        <!-- Sidebar -->
        <div class="dashboard-sidebar">
          <!-- Top Sales -->
          <div class="card sidebar-card">
            <div class="card-header">
              <h2 class="card-title">Top 5 Vendas</h2>
              <span class="card-badge">esta semana</span>
            </div>
            @if (data()!.topSellingItems.length === 0) {
              <div class="empty-state">
                <svg class="empty-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="9" cy="21" r="1"/><circle cx="20" cy="21" r="1"/><path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"/></svg>
                <span>Sem vendas esta semana</span>
              </div>
            } @else {
              <ul class="top-sales-list">
                @for (item of data()!.topSellingItems; track item.name + item.soldDate) {
                  <li class="top-sales-item">
                    <div class="item-info">
                      <span class="item-name">{{ item.name }}</span>
                      <span class="item-meta">{{ item.brand }} · {{ item.finalPrice | currency: 'EUR' }}</span>
                    </div>
                  </li>
                }
              </ul>
            }
          </div>

          <!-- Alerts -->
          @if (hasAlerts()) {
            <div class="card sidebar-card">
              <div class="card-header">
                <h2 class="card-title">Alertas</h2>
              </div>
              <div class="alerts-grid">
                @if (data()!.alerts.expiringConsignments > 0) {
                  <a class="alert-item alert-warning" routerLink="/consignments/returns">
                    <svg class="alert-icon-svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3Z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
                    {{ data()!.alerts.expiringConsignments }} consignação(ões) a expirar (7 dias)
                  </a>
                }
                @if (data()!.alerts.stagnantItems30 > 0) {
                  <a class="alert-item alert-yellow" routerLink="/inventory/items">
                    <svg class="alert-icon-svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/></svg>
                    {{ data()!.alerts.stagnantItems30 }} itens parados 30+ dias
                  </a>
                }
                @if (data()!.alerts.stagnantItems45 > 0) {
                  <a class="alert-item alert-orange" routerLink="/inventory/items">
                    <svg class="alert-icon-svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/></svg>
                    {{ data()!.alerts.stagnantItems45 }} itens parados 45+ dias
                  </a>
                }
                @if (data()!.alerts.stagnantItems60 > 0) {
                  <a class="alert-item alert-red" routerLink="/inventory/items">
                    <svg class="alert-icon-svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/></svg>
                    {{ data()!.alerts.stagnantItems60 }} itens parados 60+ dias
                  </a>
                }
                @if (data()!.alerts.openRegisters.length > 0) {
                  <div class="alert-item alert-info">
                    <svg class="alert-icon-svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><rect x="2" y="4" width="20" height="16" rx="2"/><path d="M12 4v16"/><path d="M2 12h20"/></svg>
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
          <div class="card sidebar-card quick-actions-card">
            <div class="card-header">
              <h2 class="card-title">Ações Rápidas</h2>
            </div>
            <div class="quick-actions">
              <a class="btn btn-primary btn-block" routerLink="/pos/sale">Nova Venda</a>
              <a class="btn btn-outline btn-block" routerLink="/consignments/receive">Nova Recepção</a>
              <a class="btn btn-outline btn-block" routerLink="/inventory/items">Buscar Item</a>
              <a class="btn btn-outline btn-block" routerLink="/finance/settlements">Acertos</a>
            </div>
          </div>
        </div>
        </div>
      }
    </div>
  `,
  styles: [`
    :host { display: block; }
    .dashboard-layout { display: flex; flex-direction: column; gap: 0; }

    .dashboard-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 24px;
      flex-wrap: wrap;
      gap: 16px;
    }
    .dashboard-header h1 {
      font-family: 'DM Serif Display', Georgia, serif;
      font-size: 24px;
      font-weight: 400;
      margin: 0;
      color: #1C1917;
    }
    .subtitle { color: #78716C; margin: 4px 0 0; font-size: 14px; }
    .header-actions { display: flex; gap: 12px; align-items: center; }
    .form-control {
      padding: 10px 12px;
      border: 1px solid #D6D3D1;
      border-radius: 10px;
      font-size: 14px;
      font-family: 'DM Sans', sans-serif;
      background: #fff;
      min-width: 140px;
      color: #1C1917;
    }
    .form-control:focus { outline: none; border-color: #5B7153; box-shadow: 0 0 0 3px rgba(91,113,83,0.12); }
    .loading { text-align: center; padding: 48px; color: #78716C; font-size: 14px; }

    .dashboard-content {
      display: flex;
      gap: 24px;
      flex-wrap: wrap;
    }
    .dashboard-main { flex: 1; min-width: 280px; }
    .dashboard-sidebar {
      width: 320px;
      flex-shrink: 0;
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    @media (max-width: 899px) {
      .dashboard-sidebar { width: 100%; }
    }

    .stat-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
      gap: 12px;
      margin-bottom: 20px;
    }
    .stat-card {
      padding: 16px;
      border-radius: 14px;
      background: #fff;
      border: 1px solid #E7E5E4;
      text-align: center;
      transition: all 0.15s;
      box-shadow: 0 1px 3px rgba(28,25,23,0.06);
    }
    .stat-card:hover { border-color: #D6D3D1; box-shadow: 0 4px 12px rgba(28,25,23,0.08); }
    .stat-card.stat-revenue { border-color: #5B7153; background: #F0F5EE; }
    .stat-card.stat-revenue:hover { box-shadow: 0 4px 12px rgba(91,113,83,0.15); }
    .stat-value { font-size: 20px; font-weight: 700; color: #1C1917; }
    .stat-label { font-size: 12px; color: #78716C; margin-top: 4px; }
    .stat-meta { font-size: 11px; color: #A8A29E; display: block; margin-top: 2px; }
    .growth { font-size: 12px; font-weight: 500; margin-top: 4px; display: block; }
    .growth.positive { color: #5B7153; }
    .growth.negative { color: #C45B5B; }

    .card {
      background: #fff;
      border: 1px solid #E7E5E4;
      border-radius: 14px;
      overflow: hidden;
      box-shadow: 0 1px 3px rgba(28,25,23,0.06);
    }
    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 20px;
      border-bottom: 1px solid #E7E5E4;
    }
    .card-title { font-size: 16px; font-weight: 600; margin: 0; color: #1C1917; }
    .card-badge { font-size: 11px; color: #5B7153; font-weight: 600; background: #E8EFE6; padding: 2px 10px; border-radius: 12px; }

    .chart-card { margin-bottom: 20px; }
    .chart-container { padding: 20px; min-height: 180px; }
    .chart-bars { display: flex; align-items: flex-end; gap: 4px; height: 140px; }
    .chart-bar-wrap { flex: 1; display: flex; justify-content: center; align-items: flex-end; }
    .chart-bar {
      width: 100%;
      max-width: 28px;
      min-height: 2px;
      background: #5B7153;
      border-radius: 6px 6px 0 0;
      transition: all 0.2s;
    }
    .chart-bar:hover { background: #4A5E43; }
    .chart-labels { display: flex; gap: 4px; margin-top: 12px; }
    .chart-label { flex: 1; font-size: 11px; color: #78716C; text-align: center; overflow: hidden; text-overflow: ellipsis; }
    .chart-empty { text-align: center; color: #A8A29E; padding: 48px; font-size: 14px; }

    .sidebar-card { min-height: 0; }
    .top-sales-list { list-style: none; margin: 0; padding: 0; }
    .top-sales-item {
      padding: 12px 20px;
      border-bottom: 1px solid #F5F5F4;
      display: flex;
      flex-direction: column;
      gap: 2px;
    }
    .top-sales-item:last-child { border-bottom: none; }
    .item-name { font-size: 13px; font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; color: #1C1917; }
    .item-meta { font-size: 11px; color: #78716C; }
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 8px;
      padding: 32px 20px;
      color: #A8A29E;
      font-size: 13px;
    }
    .empty-icon {
      width: 32px;
      height: 32px;
      color: #A8A29E;
    }

    .alerts-grid { display: flex; flex-direction: column; gap: 8px; padding: 16px 20px; }
    .alert-item {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px 16px;
      border-radius: 10px;
      text-decoration: none;
      color: inherit;
      font-size: 13px;
      transition: opacity 0.15s;
    }
    .alert-item:hover { opacity: 0.9; }
    .alert-icon-svg { width: 18px; height: 18px; flex-shrink: 0; }
    .alert-warning { background: #FDF3E3; color: #8B6914; }
    .alert-yellow { background: #FEF9E3; color: #7A5B0E; }
    .alert-orange { background: #FDE8D5; color: #9A4A0C; }
    .alert-red { background: #FCEAEA; color: #8B3A3A; }
    .alert-info { background: #E8EFE6; color: #3D5436; }

    .quick-actions-card .card-header { border-bottom: 1px solid #E7E5E4; }
    .quick-actions { display: flex; flex-direction: column; gap: 8px; padding: 16px 20px; }
    .btn-block { width: 100%; justify-content: center; }
    .btn {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 8px 16px;
      border: 1px solid #D6D3D1;
      border-radius: 10px;
      font-size: 13px;
      font-weight: 600;
      cursor: pointer;
      background: #fff;
      color: #44403C;
      text-decoration: none;
      transition: all 0.15s;
      font-family: 'DM Sans', sans-serif;
    }
    .btn:hover { background: #FAF9F7; border-color: #A8A29E; }
    .btn-primary { background: #5B7153; color: #fff; border-color: #5B7153; }
    .btn-primary:hover { background: #4A5E43; }
    .btn-outline { background: transparent; }
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
