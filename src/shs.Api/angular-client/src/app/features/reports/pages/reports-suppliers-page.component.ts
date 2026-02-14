import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ReportsService, SuppliersReport } from '../reports.service';

@Component({
  selector: 'oui-reports-suppliers-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page">
      <div class="page-header">
        <div>
          <h1>Relatório de Fornecedores</h1>
          <p class="subtitle">Ranking e métricas por fornecedor</p>
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
          <button class="btn btn-primary" (click)="load()">Atualizar</button>
        </div>
      </div>

      @if (loading()) {
        <div class="loading">A carregar...</div>
      } @else if (data()) {
        @if (data()!.ranking.length === 0) {
          <div class="empty-state">Sem dados no período</div>
        } @else {
          <div class="card table-card">
            <table>
              <thead>
                <tr>
                  <th>#</th>
                  <th>Fornecedor</th>
                  <th class="cell-right">Receita</th>
                  <th class="cell-center">Vendidos</th>
                  <th class="cell-center">Devolvidos</th>
                  <th class="cell-right">Taxa Devol.</th>
                  <th class="cell-right">Dias Médio</th>
                  <th class="cell-right">Pendente</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                @for (s of data()!.ranking; track s.id; let i = $index) {
                  <tr>
                    <td>{{ i + 1 }}</td>
                    <td><span class="initial-badge">{{ s.initial }}</span> {{ s.name }}</td>
                    <td class="cell-right">{{ s.revenue | currency: 'EUR' }}</td>
                    <td class="cell-center">{{ s.soldCount }}</td>
                    <td class="cell-center">{{ s.returnedCount }}</td>
                    <td class="cell-right">{{ s.returnRate | number: '1.1-1' }}%</td>
                    <td class="cell-right">{{ s.avgDaysToSell | number: '1.0-0' }}</td>
                    <td class="cell-right">{{ s.pendingAmount | currency: 'EUR' }}</td>
                    <td><a class="btn btn-outline btn-sm" [routerLink]="['/inventory/suppliers', s.externalId]">Ver</a></td>
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
    .page { max-width: 1100px; margin: 0 auto; }
    .page-header { display: flex; justify-content: space-between; margin-bottom: 24px; }
    .subtitle { color: #64748b; margin: 4px 0 0; }
    .filters-card { margin-bottom: 24px; }
    .form-row { display: flex; gap: 16px; align-items: flex-end; flex-wrap: wrap; }
    .form-group label { display: block; font-size: 12px; margin-bottom: 4px; }
    .form-control { padding: 8px 12px; border: 1px solid #e2e8f0; border-radius: 6px; }
    .loading { text-align: center; padding: 48px; color: #64748b; }
    .empty-state { text-align: center; padding: 48px; color: #94a3b8; }
    .initial-badge { display: inline-block; width: 28px; height: 28px; line-height: 28px; text-align: center; background: #e2e8f0; border-radius: 6px; font-weight: 600; font-size: 12px; margin-right: 8px; }
    .cell-right { text-align: right; }
    .cell-center { text-align: center; }
  `],
})
export class ReportsSuppliersPageComponent implements OnInit {
  private readonly reports = inject(ReportsService);
  data = signal<SuppliersReport | null>(null);
  loading = signal(true);
  startDate = '';
  endDate = '';

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
    this.reports.getSuppliersReport(this.startDate, this.endDate).subscribe({
      next: (d) => { this.data.set(d); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }
}
