import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'oui-dashboard-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-title">Dashboard</div>
    <p class="page-subtitle">
      Aqui vão ficar os KPIs principais (vendas do dia, itens em stock, acertos pendentes).
    </p>
  `,
  styles: [`
    :host { display: block; }

    .page-title {
      font-size: 22px;
      font-weight: 700;
      margin: 0 0 4px;
      color: #1e293b;
    }

    .page-subtitle {
      font-size: 14px;
      color: #64748b;
      margin: 0;
    }
  `],
})
export class DashboardPageComponent {}
