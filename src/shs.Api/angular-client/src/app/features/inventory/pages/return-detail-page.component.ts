import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { SupplierReturnService } from '../services/supplier-return.service';
import { SupplierReturnDetail } from '../../../core/models/supplier-return.model';

@Component({
  selector: 'oui-return-detail-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    @if (loading()) {
      <div class="loading">A carregar...</div>
    } @else if (error()) {
      <div class="card error-card">
        <p>{{ error() }}</p>
        <a class="btn btn-outline" routerLink="/consignments/returns">Voltar</a>
      </div>
    } @else if (ret()) {
      <div class="page">
        <div class="topbar">
          <a class="btn btn-outline" routerLink="/consignments/returns">← Voltar</a>
        </div>

        <!-- Header -->
        <div class="header">
          <div class="header-info">
            <span class="ref-label">Devolução</span>
            <h1 class="ref-title">{{ ret()!.externalId.substring(0, 8).toUpperCase() }}</h1>
            <div class="header-meta">
              <span class="initial-badge">{{ ret()!.supplier.initial }}</span>
              <a class="supplier-link" [routerLink]="['/inventory/suppliers', ret()!.supplier.externalId]">
                {{ ret()!.supplier.name }}
              </a>
              <span class="meta-sep">·</span>
              <span>{{ ret()!.returnDate | date: 'dd/MM/yyyy HH:mm' }}</span>
            </div>
          </div>
          <span class="badge badge-lg badge-returned">Devolvido</span>
        </div>

        <!-- Stats -->
        <div class="stat-grid">
          <div class="stat-card">
            <div class="stat-value">{{ ret()!.itemCount }}</div>
            <div class="stat-label">Peças Devolvidas</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ totalValue() | currency: 'EUR' }}</div>
            <div class="stat-label">Valor Total</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ rejectedCount() }}</div>
            <div class="stat-label">Eram Rejeitados</div>
          </div>
          <div class="stat-card">
            <div class="stat-value">{{ acceptedCount() }}</div>
            <div class="stat-label">Eram À Venda</div>
          </div>
        </div>

        <!-- Info -->
        @if (ret()!.notes) {
          <div class="card">
            <div class="card-title">Observações</div>
            <p class="notes-text">{{ ret()!.notes }}</p>
          </div>
        }

        <!-- Items Table -->
        <div class="card table-card">
          <div class="card-title-bar">
            <span class="card-title">Peças Devolvidas ({{ ret()!.items.length }})</span>
          </div>
          <div class="table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>#</th>
                  <th>ID</th>
                  <th>Nome</th>
                  <th>Marca</th>
                  <th>Tam.</th>
                  <th>Cor</th>
                  <th>Cond.</th>
                  <th class="cell-right">Preço</th>
                  <th>Origem</th>
                </tr>
              </thead>
              <tbody>
                @for (item of ret()!.items; track item.externalId; let idx = $index) {
                  <tr>
                    <td class="cell-num">{{ idx + 1 }}</td>
                    <td class="cell-mono">
                      <a class="link" [routerLink]="['/inventory/items', item.externalId]">{{ item.identificationNumber }}</a>
                    </td>
                    <td><b>{{ item.name }}</b></td>
                    <td>{{ item.brand }}</td>
                    <td>{{ item.size }}</td>
                    <td>{{ item.color }}</td>
                    <td>{{ getConditionLabel(item.condition) }}</td>
                    <td class="cell-right">{{ item.evaluatedPrice | currency: 'EUR' }}</td>
                    <td>
                      @if (item.isRejected) {
                        <span class="badge badge-red">Rejeitado</span>
                      } @else {
                        <span class="badge badge-amber">Era À Venda</span>
                      }
                    </td>
                  </tr>
                }
              </tbody>
              <tfoot>
                <tr>
                  <td colspan="7" class="foot-label">Total</td>
                  <td class="cell-right foot-value">{{ totalValue() | currency: 'EUR' }}</td>
                  <td></td>
                </tr>
              </tfoot>
            </table>
          </div>
        </div>

        <!-- Timeline -->
        <div class="card">
          <div class="card-title">Histórico</div>
          <div class="timeline">
            <div class="timeline-item">
              <div class="timeline-dot dot-orange"></div>
              <div class="timeline-content">
                <span class="timeline-label">Devolução registada</span>
                <span class="timeline-date">{{ ret()!.createdOn | date: 'dd/MM/yyyy HH:mm' }}</span>
                <span class="timeline-detail">{{ ret()!.itemCount }} peças devolvidas a {{ ret()!.supplier.name }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .page { max-width: 1100px; margin: 0 auto; }
    .loading { text-align: center; padding: 48px; color: #64748b; }
    .error-card { text-align: center; padding: 48px; }
    .topbar { display: flex; justify-content: space-between; margin-bottom: 20px; }

    .header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 24px; }
    .ref-label { font-size: 12px; text-transform: uppercase; letter-spacing: 0.5px; color: #64748b; }
    .ref-title { font-size: 26px; font-weight: 800; margin: 4px 0 8px; letter-spacing: 1px; font-family: monospace; }
    .header-meta { display: flex; align-items: center; gap: 8px; font-size: 13px; color: #64748b; }
    .initial-badge { width: 28px; height: 28px; border-radius: 50%; background: #eef2ff; color: #6366f1; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 700; }
    .supplier-link { color: #6366f1; text-decoration: none; font-weight: 600; }
    .supplier-link:hover { text-decoration: underline; }
    .meta-sep { color: #cbd5e1; }

    .badge { display: inline-block; padding: 2px 8px; border-radius: 12px; font-size: 11px; font-weight: 600; }
    .badge-lg { padding: 6px 14px; font-size: 13px; }
    .badge-returned { background: #fff7ed; color: #ea580c; border: 1px solid #fed7aa; }
    .badge-red { background: #fef2f2; color: #dc2626; }
    .badge-amber { background: #fffbeb; color: #d97706; }

    .stat-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin-bottom: 24px; }
    .stat-card { background: #fff; border: 1px solid #e2e8f0; border-radius: 12px; padding: 20px; text-align: center; }
    .stat-value { font-size: 24px; font-weight: 700; color: #1e293b; }
    .stat-label { font-size: 12px; color: #64748b; margin-top: 4px; }

    .card { background: #fff; border: 1px solid #e2e8f0; border-radius: 12px; padding: 24px; margin-bottom: 20px; }
    .card-title { font-size: 15px; font-weight: 700; color: #1e293b; margin-bottom: 12px; }
    .card-title-bar { padding: 16px 16px 8px; }
    .table-card { padding: 0; overflow: hidden; }
    .notes-text { font-size: 14px; color: #475569; margin: 0; line-height: 1.6; }

    .table-wrapper { overflow-x: auto; }
    table { width: 100%; border-collapse: collapse; font-size: 13px; }
    th { text-align: left; padding: 8px 16px; font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; color: #64748b; border-bottom: 2px solid #e2e8f0; white-space: nowrap; }
    td { padding: 10px 16px; border-bottom: 1px solid #f1f5f9; }
    .cell-num { color: #94a3b8; width: 36px; }
    .cell-mono { font-family: monospace; font-size: 12px; color: #64748b; }
    .cell-right { text-align: right; }
    .link { color: #6366f1; text-decoration: none; font-weight: 600; }
    .link:hover { text-decoration: underline; }
    tfoot td { border-top: 2px solid #e2e8f0; border-bottom: none; }
    .foot-label { text-align: right; font-weight: 600; color: #374151; }
    .foot-value { font-weight: 700; color: #1e293b; }

    .timeline { padding-left: 20px; border-left: 2px solid #e2e8f0; }
    .timeline-item { position: relative; padding: 0 0 0 20px; margin-bottom: 16px; }
    .timeline-item:last-child { margin-bottom: 0; }
    .timeline-dot { position: absolute; left: -27px; top: 2px; width: 12px; height: 12px; border-radius: 50%; border: 2px solid #fff; }
    .dot-orange { background: #f97316; }
    .timeline-content { display: flex; flex-direction: column; gap: 2px; }
    .timeline-label { font-size: 13px; font-weight: 600; color: #1e293b; }
    .timeline-date { font-size: 12px; color: #94a3b8; }
    .timeline-detail { font-size: 12px; color: #64748b; }

    .btn { display: inline-flex; align-items: center; gap: 6px; padding: 8px 16px; border: 1px solid #d1d5db; border-radius: 8px; font-size: 13px; font-weight: 600; cursor: pointer; background: #fff; color: #374151; text-decoration: none; transition: all 0.15s; }
    .btn:hover { background: #f8fafc; }
    .btn-outline { background: transparent; }

    @media (max-width: 768px) {
      .stat-grid { grid-template-columns: repeat(2, 1fr); }
    }
  `]
})
export class ReturnDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly returnService = inject(SupplierReturnService);

  ret = signal<SupplierReturnDetail | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  totalValue = signal(0);
  rejectedCount = signal(0);
  acceptedCount = signal(0);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.returnService.getById(id).subscribe({
        next: (data) => {
          this.ret.set(data);
          this.totalValue.set(data.items.reduce((s, i) => s + i.evaluatedPrice, 0));
          this.rejectedCount.set(data.items.filter(i => i.isRejected).length);
          this.acceptedCount.set(data.items.filter(i => !i.isRejected).length);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Devolução não encontrada.');
          this.loading.set(false);
        },
      });
    }
  }

  getConditionLabel(condition: string): string {
    const map: Record<string, string> = {
      Excellent: 'Excelente',
      VeryGood: 'Muito Bom',
      Good: 'Bom',
      Fair: 'Razoável',
      Poor: 'Mau',
    };
    return map[condition] || condition;
  }
}
