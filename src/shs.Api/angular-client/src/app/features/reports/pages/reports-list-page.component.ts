import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'oui-reports-list-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="page">
      <div class="page-header">
        <h1>Relatórios</h1>
        <p class="subtitle">Análise e métricas do negócio</p>
      </div>
      <div class="report-cards">
        <a class="report-card" routerLink="/reports/sales">
          <span class="report-icon">📊</span>
          <h3>Vendas</h3>
          <p>Receita, ticket médio, top marcas e categorias, breakdown por método de pagamento</p>
        </a>
        <a class="report-card" routerLink="/reports/inventory">
          <span class="report-icon">📦</span>
          <h3>Inventário</h3>
          <p>Distribuição por idade, taxa de venda, itens parados</p>
        </a>
        <a class="report-card" routerLink="/reports/suppliers">
          <span class="report-icon">👥</span>
          <h3>Fornecedores</h3>
          <p>Ranking por receita, dias médios de venda, taxa de devolução</p>
        </a>
        <a class="report-card" routerLink="/reports/finance">
          <span class="report-icon">💰</span>
          <h3>Financeiro</h3>
          <p>Receita bruta, comissões, acertos pendentes e pagos</p>
        </a>
      </div>
    </div>
  `,
  styles: [`
    .page { max-width: 800px; margin: 0 auto; }
    .page-header { margin-bottom: 32px; }
    .subtitle { color: #64748b; margin: 4px 0 0; }
    .report-cards { display: grid; grid-template-columns: repeat(auto-fill, minmax(280px, 1fr)); gap: 20px; }
    .report-card {
      display: block; padding: 24px; border-radius: 8px; border: 1px solid #e2e8f0;
      background: #fff; text-decoration: none; color: inherit; transition: border-color 0.2s, box-shadow 0.2s;
    }
    .report-card:hover { border-color: #0f172a; box-shadow: 0 4px 12px rgba(0,0,0,0.08); }
    .report-icon { font-size: 32px; display: block; margin-bottom: 12px; }
    .report-card h3 { margin: 0 0 8px; font-size: 18px; }
    .report-card p { margin: 0; font-size: 14px; color: #64748b; line-height: 1.5; }
  `],
})
export class ReportsListPageComponent {}
