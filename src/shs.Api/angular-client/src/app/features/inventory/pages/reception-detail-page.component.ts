import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ReceptionService } from '../services/reception.service';
import { ReceptionDetail, EvaluationItemResponse } from '../../../core/models/reception.model';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'oui-reception-detail-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    @if (loading()) {
      <div class="state-message">A carregar...</div>
    } @else if (error()) {
      <div class="state-message">{{ error() }}</div>
    } @else if (reception()) {
      <!-- Top bar -->
      <div class="topbar">
        <button class="btn btn-outline" (click)="goBack()">← Voltar</button>
        <div class="topbar-actions">
          <button class="btn btn-outline" (click)="openReceipt()">Imprimir Recibo</button>
          @if (reception()!.status === 'PendingEvaluation') {
            <a class="btn btn-primary" [routerLink]="['/consignments/receptions', reception()!.externalId, 'evaluate']">
              Avaliar Peças
            </a>
          }
          @if (reception()!.status === 'Evaluated') {
            <button class="btn btn-outline btn-email" (click)="sendEmail()" [disabled]="sendingEmail()">
              {{ sendingEmail() ? 'A enviar...' : 'Enviar Email' }}
            </button>
          }
        </div>
      </div>

      <!-- Header -->
      <div class="header">
        <div class="header-info">
          <span class="ref-label">Recepção</span>
          <h1 class="ref-title">{{ reception()!.externalId.substring(0, 8).toUpperCase() }}</h1>
          <div class="header-meta">
            <span class="initial-badge">{{ reception()!.supplier.initial }}</span>
            <a class="supplier-link" [routerLink]="['/inventory/suppliers', reception()!.supplier.externalId]">
              {{ reception()!.supplier.name }}
            </a>
            <span class="meta-sep">·</span>
            <span>{{ reception()!.receptionDate | date: 'dd/MM/yyyy HH:mm' }}</span>
          </div>
        </div>
        <span class="badge badge-lg" [ngClass]="getStatusClass(reception()!.status)">
          {{ getStatusLabel(reception()!.status) }}
        </span>
      </div>

      <!-- KPI Stats -->
      <div class="stat-grid">
        <div class="card stat-card">
          <div class="stat-label">Total Peças</div>
          <div class="stat-value">{{ reception()!.itemCount }}</div>
        </div>
        <div class="card stat-card">
          <div class="stat-label">Avaliadas</div>
          <div class="stat-value stat-indigo">{{ reception()!.evaluatedCount }}</div>
        </div>
        <div class="card stat-card">
          <div class="stat-label">Aceites</div>
          <div class="stat-value stat-green">{{ reception()!.acceptedCount }}</div>
        </div>
        <div class="card stat-card">
          <div class="stat-label">Rejeitados</div>
          <div class="stat-value" [class.stat-red]="reception()!.rejectedCount > 0">{{ reception()!.rejectedCount }}</div>
        </div>
      </div>

      <!-- Info cards row -->
      <div class="info-row">
        <!-- Reception Info -->
        <div class="card">
          <div class="card-title">Informações da Recepção</div>
          <div class="info-grid">
            <div class="info-item">
              <label>Referência</label>
              <span class="mono">{{ reception()!.externalId.substring(0, 8).toUpperCase() }}</span>
            </div>
            <div class="info-item">
              <label>Data de Recepção</label>
              <span>{{ reception()!.receptionDate | date: 'dd/MM/yyyy HH:mm' }}</span>
            </div>
            <div class="info-item">
              <label>Fornecedor</label>
              <a [routerLink]="['/inventory/suppliers', reception()!.supplier.externalId]" class="link">
                {{ reception()!.supplier.name }}
              </a>
            </div>
            <div class="info-item">
              <label>Criado por</label>
              <span>{{ reception()!.createdBy || '—' }}</span>
            </div>
            @if (reception()!.evaluatedAt) {
              <div class="info-item">
                <label>Data de Avaliação</label>
                <span>{{ reception()!.evaluatedAt | date: 'dd/MM/yyyy HH:mm' }}</span>
              </div>
              <div class="info-item">
                <label>Avaliado por</label>
                <span>{{ reception()!.evaluatedBy || '—' }}</span>
              </div>
            }
          </div>
          @if (reception()!.notes) {
            <div class="notes-section">
              <label>Observações</label>
              <p class="notes-text">{{ reception()!.notes }}</p>
            </div>
          }
        </div>

        <!-- Financial Summary (only if evaluated) -->
        @if (reception()!.status === 'Evaluated' && items().length > 0) {
          <div class="card">
            <div class="card-title">Resumo Financeiro</div>
            <div class="financial-grid">
              <div class="financial-item">
                <label>Total Peças Aceites</label>
                <span class="financial-value">{{ acceptedItems().length }}</span>
              </div>
              <div class="financial-item">
                <label>Valor Total (Venda)</label>
                <span class="financial-value financial-price">{{ totalValue() | currency: 'EUR' }}</span>
              </div>
              <div class="financial-item">
                <label>Comissão Média</label>
                <span class="financial-value">{{ avgCommission() | number: '1.0-0' }}%</span>
              </div>
              <div class="financial-item">
                <label>Valor Est. Fornecedor</label>
                <span class="financial-value">{{ supplierValue() | currency: 'EUR' }}</span>
              </div>
            </div>
          </div>
        }
      </div>

      <!-- Email status -->
      @if (emailMessage()) {
        <div class="alert" [ngClass]="emailError() ? 'alert-error' : 'alert-success'">
          {{ emailMessage() }}
        </div>
      }

      <!-- Items Table -->
      @if (items().length > 0) {
        <div class="card table-card">
          <div class="card-title-bar">
            <span class="card-title">Peças Avaliadas ({{ items().length }})</span>
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
                  <th>Condição</th>
                  <th>Preço</th>
                  <th>Comissão</th>
                  <th>Estado</th>
                </tr>
              </thead>
              <tbody>
                @for (item of items(); track item.externalId; let idx = $index) {
                  <tr [class.row-rejected]="item.isRejected">
                    <td class="cell-num">{{ idx + 1 }}</td>
                    <td class="cell-mono">
                      <a [routerLink]="['/inventory/items', item.externalId]" class="link">{{ item.identificationNumber }}</a>
                    </td>
                    <td><b>{{ item.name }}</b></td>
                    <td>{{ item.brand }}</td>
                    <td>{{ item.size }}</td>
                    <td>{{ item.color }}</td>
                    <td>{{ getConditionLabel(item.condition) }}</td>
                    <td class="cell-price">{{ item.evaluatedPrice | currency: 'EUR' }}</td>
                    <td class="cell-center">{{ item.commissionPercentage }}%</td>
                    <td>
                      @if (item.isRejected) {
                        <span class="badge badge-red" [title]="item.rejectionReason || ''">Rejeitado</span>
                      } @else {
                        <span class="badge badge-green">{{ item.status === 'ToSell' ? 'À Venda' : 'Aceite' }}</span>
                      }
                    </td>
                  </tr>
                }
              </tbody>
              <tfoot>
                <tr>
                  <td colspan="7" class="foot-label">Total Aceites</td>
                  <td class="cell-price foot-value">{{ totalValue() | currency: 'EUR' }}</td>
                  <td colspan="2"></td>
                </tr>
              </tfoot>
            </table>
          </div>
        </div>
      } @else if (reception()!.status === 'PendingEvaluation') {
        <div class="card empty-card">
          <div class="empty-icon">🔍</div>
          <h3 class="empty-title">Avaliação Pendente</h3>
          <p class="empty-subtitle">Ainda não foram avaliadas peças nesta recepção.</p>
          <a class="btn btn-primary" [routerLink]="['/consignments/receptions', reception()!.externalId, 'evaluate']">
            Iniciar Avaliação
          </a>
        </div>
      }

      <!-- Timeline -->
      <div class="card">
        <div class="card-title">Histórico</div>
        <div class="timeline">
          <div class="timeline-item">
            <div class="timeline-dot dot-blue"></div>
            <div class="timeline-content">
              <span class="timeline-label">Recepção criada</span>
              <span class="timeline-date">{{ reception()!.createdOn | date: 'dd/MM/yyyy HH:mm' }}</span>
              <span class="timeline-detail">{{ reception()!.itemCount }} peças recebidas de {{ reception()!.supplier.name }}</span>
            </div>
          </div>
          @if (reception()!.evaluatedAt) {
            <div class="timeline-item">
              <div class="timeline-dot dot-green"></div>
              <div class="timeline-content">
                <span class="timeline-label">Avaliação concluída</span>
                <span class="timeline-date">{{ reception()!.evaluatedAt | date: 'dd/MM/yyyy HH:mm' }}</span>
                <span class="timeline-detail">{{ reception()!.acceptedCount }} aceites · {{ reception()!.rejectedCount }} rejeitados</span>
              </div>
            </div>
          }
        </div>
      </div>
    }
  `,
  styles: [`
    :host { display: block; }

    /* ── Topbar ── */
    .topbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }

    .topbar-actions { display: flex; gap: 8px; }

    /* ── Header ── */
    .header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 24px;
      gap: 20px;
    }

    .ref-label {
      font-size: 12px;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      font-weight: 600;
    }

    .ref-title {
      font-size: 28px;
      font-weight: 800;
      color: #1e293b;
      margin: 2px 0 8px;
      font-family: monospace;
      letter-spacing: 1px;
    }

    .header-meta {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 14px;
      color: #475569;
    }

    .initial-badge {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 28px;
      height: 28px;
      border-radius: 6px;
      background: #6366f1;
      color: white;
      font-size: 11px;
      font-weight: 700;
      flex-shrink: 0;
    }

    .supplier-link {
      font-weight: 600;
      color: #1e293b;
      text-decoration: none;
    }

    .supplier-link:hover { color: #6366f1; text-decoration: underline; }
    .meta-sep { color: #cbd5e1; }

    /* ── Stat Grid ── */
    .stat-grid {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 16px;
      margin-bottom: 20px;
    }

    .stat-card { text-align: center; padding: 20px; }

    .stat-label {
      font-size: 12px;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      font-weight: 600;
      margin-bottom: 8px;
    }

    .stat-value { font-size: 28px; font-weight: 800; color: #1e293b; }
    .stat-indigo { color: #6366f1; }
    .stat-green { color: #16a34a; }
    .stat-red { color: #ef4444; }

    /* ── Info Row ── */
    .info-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
      margin-bottom: 20px;
    }

    .info-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .info-item { display: flex; flex-direction: column; gap: 4px; }

    .info-item label {
      font-size: 11px;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      font-weight: 600;
    }

    .info-item span, .info-item a { font-size: 14px; color: #1e293b; font-weight: 500; }
    .mono { font-family: monospace; letter-spacing: 0.5px; }
    .link { color: #6366f1; text-decoration: none; }
    .link:hover { text-decoration: underline; }

    .notes-section {
      margin-top: 16px;
      padding-top: 16px;
      border-top: 1px solid #f1f5f9;
    }

    .notes-section label {
      display: block;
      font-size: 11px;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      font-weight: 600;
      margin-bottom: 6px;
    }

    .notes-text {
      font-size: 13px;
      color: #475569;
      padding: 10px 14px;
      background: #f8fafc;
      border-radius: 8px;
      margin: 0;
      line-height: 1.5;
    }

    /* ── Financial ── */
    .financial-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 20px;
    }

    .financial-item { display: flex; flex-direction: column; gap: 4px; }

    .financial-item label {
      font-size: 11px;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      font-weight: 600;
    }

    .financial-value { font-size: 20px; font-weight: 700; color: #1e293b; }
    .financial-price { color: #6366f1; }

    /* ── Cards ── */
    .card {
      background: #ffffff;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      padding: 20px;
      margin-bottom: 16px;
    }

    .table-card { padding: 0; margin-bottom: 16px; }

    .card-title {
      font-size: 13px;
      font-weight: 600;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-bottom: 16px;
    }

    .card-title-bar {
      padding: 14px 20px;
      border-bottom: 1px solid #e2e8f0;
    }

    .card-title-bar .card-title { margin-bottom: 0; }

    /* ── Table ── */
    .table-wrapper { overflow-x: auto; }

    table { width: 100%; border-collapse: collapse; font-size: 13px; }

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
      padding: 10px 14px;
      border-bottom: 1px solid #e2e8f0;
      vertical-align: middle;
    }

    tr:hover td { background: #f8fafc; }
    .row-rejected td { background: #fef2f2; }
    .row-rejected:hover td { background: #fee2e2; }

    .cell-num { width: 40px; color: #94a3b8; text-align: center; }
    .cell-mono { font-family: monospace; font-size: 12px; }
    .cell-price { font-weight: 600; white-space: nowrap; }
    .cell-center { text-align: center; }

    tfoot td {
      background: #f8fafc;
      font-weight: 700;
      border-top: 2px solid #e2e8f0;
      border-bottom: none;
    }

    .foot-label { text-align: right; color: #475569; }
    .foot-value { color: #6366f1; }

    /* ── Badge ── */
    .badge {
      display: inline-block;
      padding: 3px 10px;
      border-radius: 20px;
      font-size: 11px;
      font-weight: 600;
      white-space: nowrap;
    }

    .badge-lg { padding: 6px 16px; font-size: 13px; }
    .badge-yellow { background: #fef9c3; color: #854d0e; }
    .badge-green { background: #dcfce7; color: #166534; }
    .badge-red { background: #fee2e2; color: #991b1b; }
    .badge-blue { background: #dbeafe; color: #1e40af; }

    /* ── Buttons ── */
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
      text-decoration: none;
    }

    .btn:disabled { opacity: 0.5; cursor: not-allowed; }
    .btn-primary { background: #6366f1; color: white; }
    .btn-primary:hover:not(:disabled) { background: #4f46e5; }
    .btn-outline { background: white; color: #1e293b; border-color: #e2e8f0; }
    .btn-outline:hover:not(:disabled) { background: #f8fafc; }
    .btn-email { border-color: #c7d2fe; color: #4f46e5; }
    .btn-email:hover:not(:disabled) { background: #eef2ff; }

    /* ── Empty ── */
    .empty-card {
      text-align: center;
      padding: 48px 24px;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 8px;
    }

    .empty-icon { font-size: 40px; margin-bottom: 8px; }
    .empty-title { font-size: 18px; font-weight: 700; color: #1e293b; margin: 0; }
    .empty-subtitle { font-size: 14px; color: #64748b; margin: 0 0 12px; }

    /* ── Alert ── */
    .alert {
      padding: 10px 14px;
      border-radius: 8px;
      font-size: 13px;
      margin-bottom: 16px;
    }

    .alert-success { background: #f0fdf4; color: #166534; border: 1px solid #bbf7d0; }
    .alert-error { background: #fef2f2; color: #991b1b; border: 1px solid #fecaca; }

    /* ── Timeline ── */
    .timeline {
      display: flex;
      flex-direction: column;
      gap: 0;
    }

    .timeline-item {
      display: flex;
      gap: 12px;
      padding: 12px 0;
      position: relative;
    }

    .timeline-item:not(:last-child)::after {
      content: '';
      position: absolute;
      left: 7px;
      top: 28px;
      bottom: -4px;
      width: 2px;
      background: #e2e8f0;
    }

    .timeline-dot {
      width: 16px;
      height: 16px;
      border-radius: 50%;
      flex-shrink: 0;
      margin-top: 2px;
    }

    .dot-blue { background: #6366f1; }
    .dot-green { background: #16a34a; }

    .timeline-content { display: flex; flex-direction: column; gap: 2px; }

    .timeline-label { font-size: 14px; font-weight: 600; color: #1e293b; }
    .timeline-date { font-size: 12px; color: #64748b; }
    .timeline-detail { font-size: 13px; color: #475569; }

    /* ── State ── */
    .state-message {
      text-align: center;
      padding: 4rem 2rem;
      color: #64748b;
      font-size: 15px;
      background: white;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
    }

    /* ── Responsive ── */
    @media (max-width: 1024px) {
      .stat-grid { grid-template-columns: repeat(2, 1fr); }
      .info-row { grid-template-columns: 1fr; }
    }

    @media (max-width: 768px) {
      .topbar { flex-direction: column; align-items: flex-start; gap: 12px; }
      .header { flex-direction: column; }
      .stat-grid { grid-template-columns: 1fr 1fr; }
    }
  `]
})
export class ReceptionDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly receptionService = inject(ReceptionService);

  reception = signal<ReceptionDetail | null>(null);
  items = signal<EvaluationItemResponse[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  sendingEmail = signal(false);
  emailMessage = signal<string | null>(null);
  emailError = signal(false);

  // Computed
  acceptedItems = signal<EvaluationItemResponse[]>([]);
  totalValue = signal(0);
  avgCommission = signal(0);
  supplierValue = signal(0);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error.set('ID de recepção inválido.');
      this.loading.set(false);
      return;
    }
    this.loadData(id);
  }

  private loadData(id: string): void {
    forkJoin({
      reception: this.receptionService.getById(id),
      items: this.receptionService.getReceptionItems(id),
    }).subscribe({
      next: ({ reception, items }) => {
        this.reception.set(reception);
        this.items.set(items);
        this.computeFinancials(items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Erro ao carregar dados da recepção.');
        this.loading.set(false);
      }
    });
  }

  private computeFinancials(items: EvaluationItemResponse[]): void {
    const accepted = items.filter(i => !i.isRejected);
    this.acceptedItems.set(accepted);

    const total = accepted.reduce((sum, i) => sum + i.evaluatedPrice, 0);
    this.totalValue.set(total);

    const avgComm = accepted.length > 0
      ? accepted.reduce((sum, i) => sum + i.commissionPercentage, 0) / accepted.length
      : 0;
    this.avgCommission.set(avgComm);

    const supplierVal = accepted.reduce((sum, i) => {
      return sum + (i.evaluatedPrice * (1 - i.commissionPercentage / 100));
    }, 0);
    this.supplierValue.set(supplierVal);
  }

  goBack(): void {
    this.router.navigate(['/consignments/receptions']);
  }

  openReceipt(): void {
    this.receptionService.getReceiptHtml(this.reception()!.externalId).subscribe({
      next: (html) => {
        const w = window.open('', '_blank');
        if (w) {
          w.document.write(html);
          w.document.close();
        }
      }
    });
  }

  sendEmail(): void {
    this.sendingEmail.set(true);
    this.emailMessage.set(null);
    this.emailError.set(false);

    this.receptionService.sendEvaluationEmail(this.reception()!.externalId).subscribe({
      next: (result) => {
        this.sendingEmail.set(false);
        this.emailMessage.set(`Email enviado com sucesso para ${result.sentTo}`);
        this.emailError.set(false);
      },
      error: (err) => {
        this.sendingEmail.set(false);
        this.emailMessage.set(err.error?.error || 'Erro ao enviar email.');
        this.emailError.set(true);
      }
    });
  }

  getStatusLabel(status: string): string {
    const labels: Record<string, string> = {
      PendingEvaluation: 'Pendente Avaliação',
      Evaluated: 'Avaliada',
      ConsignmentCreated: 'Consignação Criada',
    };
    return labels[status] || status;
  }

  getStatusClass(status: string): string {
    const classes: Record<string, string> = {
      PendingEvaluation: 'badge-yellow',
      Evaluated: 'badge-blue',
      ConsignmentCreated: 'badge-green',
    };
    return classes[status] || 'badge-yellow';
  }

  getConditionLabel(condition: string): string {
    const labels: Record<string, string> = {
      Excellent: 'Excelente',
      VeryGood: 'Muito Bom',
      Good: 'Bom',
      Fair: 'Razoável',
      Poor: 'Mau',
    };
    return labels[condition] || condition;
  }
}
