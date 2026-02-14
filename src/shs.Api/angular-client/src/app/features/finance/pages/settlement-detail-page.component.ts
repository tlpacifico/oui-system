import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FinanceService, SettlementDetail, SettlementStatus } from '../finance.service';

@Component({
  selector: 'oui-settlement-detail-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (loading()) {
      <div class="state-message">A carregar...</div>
    } @else if (settlement()) {
      <div class="page">
        <div class="detail-topbar">
          <button class="btn btn-outline" (click)="goBack()">← Voltar</button>
          <div class="detail-topbar-right">
            @if (settlement()!.status === 1) {
              <button
                class="btn btn-outline btn-danger"
                (click)="confirmCancel()"
              >
                Cancelar Acerto
              </button>
              <button
                class="btn btn-primary"
                (click)="processPayment()"
                [disabled]="processing()"
              >
                {{ processing() ? 'A processar...' : 'Processar Pagamento' }}
              </button>
            }
          </div>
        </div>

        <!-- Header -->
        <div class="settlement-header">
          <div class="header-info">
            <span class="settlement-period">{{ formatPeriod(settlement()!.periodStart, settlement()!.periodEnd) }}</span>
            <h1 class="settlement-name">Acerto – {{ settlement()!.supplierName }}</h1>
          </div>
          <span class="badge badge-lg" [ngClass]="getStatusClass(settlement()!.status)">
            {{ getStatusLabel(settlement()!.status) }}
          </span>
        </div>

        <!-- KPIs -->
        <div class="stat-grid">
          <div class="card stat-card">
            <div class="stat-label">Itens</div>
            <div class="stat-value">{{ settlement()!.items.length }}</div>
          </div>
          <div class="card stat-card">
            <div class="stat-label">Total Vendas</div>
            <div class="stat-value">{{ settlement()!.totalSalesAmount | currency: 'EUR' }}</div>
          </div>
          <div class="card stat-card">
            <div class="stat-label">Crédito em Loja</div>
            <div class="stat-value stat-success">{{ settlement()!.storeCreditAmount | currency: 'EUR' }}</div>
          </div>
          <div class="card stat-card">
            <div class="stat-label">Resgate em Dinheiro</div>
            <div class="stat-value stat-info">{{ settlement()!.cashRedemptionAmount | currency: 'EUR' }}</div>
          </div>
          <div class="card stat-card highlight">
            <div class="stat-label">Total a Pagar</div>
            <div class="stat-value">{{ settlement()!.netAmountToSupplier | currency: 'EUR' }}</div>
          </div>
        </div>

        <!-- Supplier info -->
        <div class="card">
          <div class="card-title">Fornecedor</div>
          <div class="info-grid">
            <div class="info-row">
              <label>Nome</label>
              <span>{{ settlement()!.supplierName }}</span>
            </div>
            <div class="info-row">
              <label>Email</label>
              <span>{{ settlement()!.supplierEmail || '—' }}</span>
            </div>
            <div class="info-row">
              <label>Telefone</label>
              <span>{{ formatPhone(settlement()!.supplierPhone) }}</span>
            </div>
            <div class="info-row">
              <label>Comissões</label>
              <span>Crédito loja {{ settlement()!.creditPercentageInStore }}% · Resgate {{ settlement()!.cashRedemptionPercentage }}%</span>
            </div>
            @if (settlement()!.paidOn) {
              <div class="info-row">
                <label>Pago em</label>
                <span>{{ settlement()!.paidOn | date: 'dd/MM/yyyy HH:mm' }} por {{ settlement()!.paidBy }}</span>
              </div>
            }
          </div>
        </div>

        <!-- Items table -->
        <div class="card table-card">
          <div class="card-title">Itens do Acerto</div>
          <div class="table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Nome</th>
                  <th>Marca</th>
                  <th class="cell-right">Preço Venda</th>
                  <th>Data Venda</th>
                </tr>
              </thead>
              <tbody>
                @for (item of settlement()!.items; track item.externalId) {
                  <tr>
                    <td class="cell-mono">{{ item.identificationNumber }}</td>
                    <td>{{ item.name }}</td>
                    <td>{{ item.brandName }}</td>
                    <td class="cell-right">{{ item.finalPrice | currency: 'EUR' }}</td>
                    <td>{{ item.saleDate | date: 'dd/MM/yyyy' }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>

        @if (settlement()!.notes) {
          <div class="card">
            <div class="card-title">Notas</div>
            <p>{{ settlement()!.notes }}</p>
          </div>
        }
      </div>
    } @else {
      <div class="state-message">Acerto não encontrado.</div>
    }
  `,
  styles: [`
    :host { display: block; }

    .page { max-width: 1000px; margin: 0 auto; }

    .detail-topbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }

    .detail-topbar-right { display: flex; gap: 8px; }

    .settlement-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 24px;
      gap: 20px;
    }

    .header-info { flex: 1; min-width: 0; }

    .settlement-period {
      font-size: 12px;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      display: block;
      margin-bottom: 4px;
    }

    .settlement-name {
      font-size: 24px;
      font-weight: 700;
      margin: 0;
      color: #1e293b;
    }

    .stat-grid {
      display: grid;
      grid-template-columns: repeat(5, 1fr);
      gap: 16px;
      margin-bottom: 24px;
    }

    .stat-card {
      text-align: center;
      padding: 20px;
    }

    .stat-card.highlight {
      border: 2px solid #6366f1;
      background: #faf5ff;
    }

    .stat-label {
      font-size: 12px;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      font-weight: 600;
      margin-bottom: 8px;
    }

    .stat-value {
      font-size: 22px;
      font-weight: 800;
      color: #1e293b;
    }

    .stat-success { color: #059669; }
    .stat-info { color: #0284c7; }

    .card {
      background: #ffffff;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      padding: 20px;
    }

    .table-card { padding: 0; }

    .card-title {
      font-size: 13px;
      font-weight: 600;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-bottom: 0;
    }

    .table-card .card-title {
      padding: 20px 20px 16px;
      margin-bottom: 0;
    }

    .table-wrapper { overflow-x: auto; }

    table {
      width: 100%;
      border-collapse: collapse;
      font-size: 13px;
    }

    th {
      background: #f8fafc;
      padding: 10px 14px;
      text-align: left;
      font-weight: 600;
      font-size: 11px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      color: #64748b;
      border-bottom: 1px solid #e2e8f0;
    }

    td {
      padding: 12px 14px;
      border-bottom: 1px solid #e2e8f0;
      vertical-align: middle;
    }

    tr:hover td { background: #f1f5f9; }

    .info-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
      gap: 16px;
    }

    .info-row label {
      display: block;
      font-size: 12px;
      color: #64748b;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-bottom: 4px;
    }

    .info-row span { font-size: 14px; color: #1e293b; font-weight: 500; }

    .cell-right { text-align: right; }
    .cell-mono { font-family: monospace; font-size: 12px; }

    .badge {
      display: inline-block;
      padding: 3px 10px;
      border-radius: 20px;
      font-size: 11px;
      font-weight: 600;
      white-space: nowrap;
    }

    .badge-lg { padding: 6px 16px; font-size: 13px; }

    .badge-pending { background: #fef3c7; color: #92400e; }
    .badge-paid { background: #dcfce7; color: #166534; }
    .badge-cancelled { background: #fee2e2; color: #991b1b; }

    .btn {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 8px 16px;
      border-radius: 8px;
      font-size: 13px;
      font-weight: 600;
      cursor: pointer;
      border: 1px solid transparent;
      transition: all 0.15s;
    }

    .btn-primary { background: #6366f1; color: white; }
    .btn-primary:hover:not(:disabled) { background: #4f46e5; }

    .btn-outline {
      background: white;
      color: #1e293b;
      border-color: #e2e8f0;
    }

    .btn-outline:hover { background: #f8fafc; }

    .btn-danger {
      border-color: #fecaca;
      color: #991b1b;
    }

    .btn-danger:hover { background: #fee2e2; }

    .state-message {
      text-align: center;
      padding: 4rem 2rem;
      color: #64748b;
      font-size: 15px;
      background: white;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
    }

    @media (max-width: 1024px) {
      .stat-grid { grid-template-columns: repeat(2, 1fr); }
    }

    @media (max-width: 768px) {
      .settlement-header { flex-direction: column; }
      .stat-grid { grid-template-columns: 1fr 1fr; }
    }
  `],
})
export class SettlementDetailPageComponent implements OnInit {
  private readonly finance = inject(FinanceService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  settlement = signal<SettlementDetail | null>(null);
  loading = signal(true);
  processing = signal(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.finance.getSettlementById(id).subscribe({
        next: (s) => {
          this.settlement.set(s);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
    } else {
      this.loading.set(false);
    }
  }

  processPayment(): void {
    const s = this.settlement();
    if (!s || s.status !== 1) return;
    this.processing.set(true);
    this.finance.processPayment(s.externalId).subscribe({
      next: () => {
        this.processing.set(false);
        this.finance.getSettlementById(s.externalId).subscribe({
          next: (updated) => this.settlement.set(updated),
        });
      },
      error: (err) => {
        this.processing.set(false);
        alert(err.error?.error ?? 'Erro ao processar pagamento.');
      },
    });
  }

  confirmCancel(): void {
    if (!confirm('Tem a certeza que deseja cancelar este acerto? Os itens voltarão a ficar disponíveis para acerto.')) {
      return;
    }
    const s = this.settlement();
    if (!s || s.status !== 1) return;
    this.finance.cancelSettlement(s.externalId).subscribe({
      next: () => this.router.navigate(['/finance/settlements']),
      error: (err) => alert(err.error?.error ?? 'Erro ao cancelar acerto.'),
    });
  }

  formatPeriod(start: string, end: string): string {
    const s = new Date(start);
    const e = new Date(end);
    return `${s.toLocaleDateString('pt-PT')} – ${e.toLocaleDateString('pt-PT')}`;
  }

  formatPhone(phone: string | undefined): string {
    if (!phone) return '—';
    const digits = phone.replace(/\D/g, '');
    if (digits.length === 9 && digits.startsWith('9')) {
      return `+351 ${digits.slice(0, 3)} ${digits.slice(3, 6)} ${digits.slice(6)}`;
    }
    return phone;
  }

  getStatusLabel(status: SettlementStatus): string {
    const map: Record<number, string> = { 1: 'Pendente', 2: 'Pago', 3: 'Cancelado' };
    return map[status] ?? '—';
  }

  getStatusClass(status: SettlementStatus): string {
    const map: Record<number, string> = { 1: 'badge-pending', 2: 'badge-paid', 3: 'badge-cancelled' };
    return map[status] ?? '';
  }

  goBack(): void {
    this.router.navigate(['/finance/settlements']);
  }
}
