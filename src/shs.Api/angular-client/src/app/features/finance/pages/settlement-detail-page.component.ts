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
                class="btn btn-outline btn-danger-outline"
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
          <div class="header-left">
            <h1>Acerto – {{ settlement()!.supplierName }}</h1>
            <div class="header-meta">
              <span>{{ formatPeriod(settlement()!.periodStart, settlement()!.periodEnd) }}</span>
              <span class="meta-sep">·</span>
              <span class="badge" [ngClass]="getStatusClass(settlement()!.status)">
                {{ getStatusLabel(settlement()!.status) }}
              </span>
            </div>
          </div>
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
          <div class="card-title-bar">
            <span class="card-title">Itens do Acerto</span>
          </div>
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
    .page { max-width: 1000px; margin: 0 auto; }
    .detail-topbar {
      display: flex; justify-content: space-between; align-items: center;
      margin-bottom: 24px;
    }
    .detail-topbar-right { display: flex; gap: 8px; }
    .settlement-header { margin-bottom: 24px; }
    .settlement-header h1 { font-size: 22px; font-weight: 700; margin: 0; }
    .header-meta { font-size: 14px; color: #64748b; margin-top: 4px; }
    .meta-sep { margin: 0 8px; }
    .badge { padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 500; }
    .badge-pending { background: #fef3c7; color: #92400e; }
    .badge-paid { background: #d1fae5; color: #065f46; }
    .badge-cancelled { background: #fee2e2; color: #991b1b; }
    .stat-grid {
      display: grid; grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
      gap: 16px; margin-bottom: 24px;
    }
    .stat-card {
      padding: 16px; text-align: center;
    }
    .stat-card.highlight { border: 2px solid #0f172a; }
    .stat-label { font-size: 12px; color: #64748b; margin-bottom: 4px; }
    .stat-value { font-size: 18px; font-weight: 700; }
    .stat-success { color: #059669; }
    .stat-info { color: #0284c7; }
    .info-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 12px; }
    .info-row label { display: block; font-size: 12px; color: #64748b; margin-bottom: 2px; }
    .info-row span { font-size: 14px; }
    .cell-right { text-align: right; }
    .cell-mono { font-family: monospace; font-size: 13px; }
    .state-message { text-align: center; padding: 48px; color: #64748b; }
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
