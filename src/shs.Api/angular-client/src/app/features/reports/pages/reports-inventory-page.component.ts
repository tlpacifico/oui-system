import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ReportsService, InventoryReport } from '../reports.service';

@Component({
  selector: 'oui-reports-inventory-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="page">
      <div class="page-header">
        <div>
          <h1>Relatório de Inventário</h1>
          <p class="subtitle">Distribuição por idade e taxa de venda</p>
        </div>
        <a class="btn btn-outline" routerLink="/reports">← Voltar</a>
      </div>

      @if (loading()) {
        <div class="loading">A carregar...</div>
      } @else if (data()) {
        <div class="stat-grid">
          <div class="stat-card">
            <div class="stat-value">{{ data()!.totalItems }}</div>
            <div class="stat-label">Itens em Stock</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ data()!.totalValue | currency: 'EUR' }}</div>
            <div class="stat-label">Valor Total</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ data()!.sellThroughRate | number: '1.1-1' }}%</div>
            <div class="stat-label">Taxa de Venda</div>
          </div>
        </div>

        <div class="card">
          <div class="card-title">Distribuição por Idade (dias em stock)</div>
          <div class="aging-grid">
            <div class="aging-item">
              <span class="aging-label">0-15 dias</span>
              <span class="aging-value">{{ data()!.agingDistribution.days0_15 }}</span>
            </div>
            <div class="aging-item">
              <span class="aging-label">15-30 dias</span>
              <span class="aging-value">{{ data()!.agingDistribution.days15_30 }}</span>
            </div>
            <div class="aging-item aging-warn">
              <span class="aging-label">30-45 dias</span>
              <span class="aging-value">{{ data()!.agingDistribution.days30_45 }}</span>
            </div>
            <div class="aging-item aging-warn">
              <span class="aging-label">45-60 dias</span>
              <span class="aging-value">{{ data()!.agingDistribution.days45_60 }}</span>
            </div>
            <div class="aging-item aging-danger">
              <span class="aging-label">60+ dias</span>
              <span class="aging-value">{{ data()!.agingDistribution.days60Plus }}</span>
            </div>
          </div>
        </div>

        <div class="card table-card">
          <div class="card-title">Taxa de Venda por Marca</div>
          @if (data()!.sellThroughByBrand.length === 0) {
            <div class="empty">Sem dados</div>
          } @else {
            <table>
              <thead>
                <tr><th>Marca</th><th class="cell-center">Em Stock</th><th class="cell-center">Vendidos</th><th class="cell-right">Taxa</th></tr>
              </thead>
              <tbody>
                @for (b of data()!.sellThroughByBrand; track b.brandName) {
                  <tr>
                    <td>{{ b.brandName }}</td>
                    <td class="cell-center">{{ b.inStock }}</td>
                    <td class="cell-center">{{ b.sold }}</td>
                    <td class="cell-right">{{ b.sellThroughRate | number: '1.1-1' }}%</td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </div>

        <div class="card table-card">
          <div class="card-title">Itens Parados (30+ dias)</div>
          @if (data()!.stagnantItemsList.length === 0) {
            <div class="empty">Nenhum item parado</div>
          } @else {
            <table>
              <thead>
                <tr><th>Marca</th><th>Categoria</th><th class="cell-right">Preço</th><th class="cell-center">Dias</th><th>Ações</th></tr>
              </thead>
              <tbody>
                @for (i of data()!.stagnantItemsList; track i.id) {
                  <tr>
                    <td>{{ i.brandName }}</td>
                    <td>{{ i.categoryName || '—' }}</td>
                    <td class="cell-right">{{ i.evaluatedPrice | currency: 'EUR' }}</td>
                    <td class="cell-center">{{ i.daysInStock }}</td>
                    <td><a class="btn btn-outline btn-sm" [routerLink]="['/inventory/items', i.externalId]">Ver</a></td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .page { max-width: 900px; margin: 0 auto; }
    .page-header { display: flex; justify-content: space-between; margin-bottom: 24px; }
    .subtitle { color: #64748b; margin: 4px 0 0; }
    .loading { text-align: center; padding: 48px; color: #64748b; }
    .stat-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 16px; margin-bottom: 24px; }
    .stat-card { padding: 16px; border-radius: 8px; background: #fff; border: 1px solid #e2e8f0; text-align: center; }
    .stat-value { font-size: 18px; font-weight: 700; }
    .stat-label { font-size: 12px; color: #64748b; margin-top: 4px; }
    .aging-grid { display: grid; grid-template-columns: repeat(5, 1fr); gap: 16px; }
    .aging-item { padding: 16px; text-align: center; border-radius: 6px; background: #f8fafc; }
    .aging-warn { background: #fef3c7; }
    .aging-danger { background: #fee2e2; }
    .aging-label { display: block; font-size: 12px; color: #64748b; }
    .aging-value { font-size: 20px; font-weight: 700; }
    .cell-right { text-align: right; }
    .cell-center { text-align: center; }
    .empty { padding: 24px; text-align: center; color: #94a3b8; }
  `],
})
export class ReportsInventoryPageComponent implements OnInit {
  private readonly reports = inject(ReportsService);
  data = signal<InventoryReport | null>(null);
  loading = signal(true);

  ngOnInit(): void {
    this.reports.getInventoryReport().subscribe({
      next: (d) => { this.data.set(d); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }
}
