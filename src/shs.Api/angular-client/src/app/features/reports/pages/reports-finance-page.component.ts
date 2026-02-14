import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ReportsService, FinanceReport } from '../reports.service';

@Component({
  selector: 'oui-reports-finance-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page">
      <div class="page-header">
        <div>
          <h1>Relatório Financeiro</h1>
          <p class="subtitle">Receita, comissões e acertos</p>
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
        <div class="stat-grid">
          <div class="stat-card">
            <div class="stat-value">{{ data()!.grossRevenue | currency: 'EUR' }}</div>
            <div class="stat-label">Receita Bruta</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ data()!.commissionRevenue | currency: 'EUR' }}</div>
            <div class="stat-label">Comissões (Loja)</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ data()!.pendingSettlements | currency: 'EUR' }}</div>
            <div class="stat-label">Acertos Pendentes</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ data()!.paidSettlements | currency: 'EUR' }}</div>
            <div class="stat-label">Acertos Pagos</div>
          </div>
          <div class="stat-card stat-highlight">
            <div class="stat-value">{{ data()!.projectedCashflow | currency: 'EUR' }}</div>
            <div class="stat-label">Fluxo de Caixa Projetado</div>
          </div>
        </div>
        <div class="card">
          <p class="info-text">
            Fluxo de caixa = Receita bruta - Comissões - Acertos pendentes
          </p>
        </div>
      }
    </div>
  `,
  styles: [`
    .page { max-width: 900px; margin: 0 auto; }
    .page-header { display: flex; justify-content: space-between; margin-bottom: 24px; }
    .subtitle { color: #64748b; margin: 4px 0 0; }
    .filters-card { margin-bottom: 24px; }
    .form-row { display: flex; gap: 16px; align-items: flex-end; flex-wrap: wrap; }
    .form-group label { display: block; font-size: 12px; margin-bottom: 4px; }
    .form-control { padding: 8px 12px; border: 1px solid #e2e8f0; border-radius: 6px; }
    .loading { text-align: center; padding: 48px; color: #64748b; }
    .stat-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap: 16px; margin-bottom: 24px; }
    .stat-card { padding: 16px; border-radius: 8px; background: #fff; border: 1px solid #e2e8f0; text-align: center; }
    .stat-card.stat-highlight { border-color: #0f172a; background: #f8fafc; }
    .stat-value { font-size: 18px; font-weight: 700; }
    .stat-label { font-size: 12px; color: #64748b; margin-top: 4px; }
    .info-text { margin: 0; font-size: 14px; color: #64748b; }
  `],
})
export class ReportsFinancePageComponent implements OnInit {
  private readonly reports = inject(ReportsService);
  data = signal<FinanceReport | null>(null);
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
    this.reports.getFinanceReport(this.startDate, this.endDate).subscribe({
      next: (d) => { this.data.set(d); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }
}
