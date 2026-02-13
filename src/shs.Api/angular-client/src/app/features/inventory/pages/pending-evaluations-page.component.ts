import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ReceptionService } from '../services/reception.service';
import { ReceptionListItem } from '../../../core/models/reception.model';

@Component({
  selector: 'oui-pending-evaluations-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Avaliações Pendentes</h1>
        <p class="page-subtitle">Recepções aguardando avaliação de peças</p>
      </div>
    </div>

    @if (loading()) {
      <div class="state-message">A carregar...</div>
    } @else if (receptions().length === 0) {
      <div class="state-message">
        <div class="empty-icon">✓</div>
        <p>Não existem avaliações pendentes de momento.</p>
        <p class="text-muted-sm">Todas as recepções foram avaliadas.</p>
      </div>
    } @else {
      <div class="grid">
        @for (reception of receptions(); track reception.externalId) {
          <div class="card reception-card">
            <div class="card-top">
              <div class="supplier-info">
                <span class="initial-badge">{{ reception.supplier.initial }}</span>
                <div>
                  <span class="supplier-name">{{ reception.supplier.name }}</span>
                  <span class="reception-date">{{ reception.receptionDate | date: 'dd/MM/yyyy HH:mm' }}</span>
                </div>
              </div>
              <span class="badge badge-yellow">Pendente</span>
            </div>

            <div class="card-stats">
              <div class="stat">
                <span class="stat-value">{{ reception.itemCount }}</span>
                <span class="stat-label">Peças recebidas</span>
              </div>
              <div class="stat">
                <span class="stat-value">{{ reception.evaluatedCount }}</span>
                <span class="stat-label">Avaliadas</span>
              </div>
              <div class="stat">
                <span class="stat-value remaining"
                  [class.remaining-zero]="(reception.itemCount - reception.evaluatedCount) === 0">
                  {{ reception.itemCount - reception.evaluatedCount }}
                </span>
                <span class="stat-label">Faltam</span>
              </div>
            </div>

            <div class="progress-bar-container">
              <div class="progress-bar" [style.width.%]="getProgress(reception)"></div>
            </div>
            <span class="progress-label">{{ getProgress(reception) | number: '1.0-0' }}% avaliado</span>

            @if (reception.notes) {
              <div class="card-notes">
                <span class="notes-label">Notas:</span> {{ reception.notes }}
              </div>
            }

            <div class="card-actions">
              <a class="btn btn-primary" [routerLink]="['/consignments/receptions', reception.externalId, 'evaluate']">
                Avaliar Peças
              </a>
            </div>
          </div>
        }
      </div>
    }
  `,
  styles: [`
    :host { display: block; }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }

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

    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(360px, 1fr));
      gap: 16px;
    }

    .card {
      background: #ffffff;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      padding: 20px;
    }

    .reception-card {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .card-top {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
    }

    .supplier-info {
      display: flex;
      gap: 10px;
      align-items: center;
    }

    .initial-badge {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 40px;
      height: 40px;
      border-radius: 8px;
      background: #6366f1;
      color: white;
      font-size: 14px;
      font-weight: 700;
      flex-shrink: 0;
    }

    .supplier-name {
      display: block;
      font-size: 15px;
      font-weight: 700;
      color: #1e293b;
    }

    .reception-date {
      display: block;
      font-size: 12px;
      color: #64748b;
    }

    .badge {
      display: inline-block;
      padding: 3px 10px;
      border-radius: 20px;
      font-size: 11px;
      font-weight: 600;
      white-space: nowrap;
    }

    .badge-yellow { background: #fef9c3; color: #854d0e; }

    .card-stats {
      display: flex;
      gap: 24px;
    }

    .stat {
      display: flex;
      flex-direction: column;
    }

    .stat-value {
      font-size: 22px;
      font-weight: 700;
      color: #1e293b;
    }

    .stat-label {
      font-size: 11px;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.3px;
    }

    .remaining { color: #ea580c; }
    .remaining-zero { color: #16a34a; }

    .progress-bar-container {
      width: 100%;
      height: 6px;
      background: #e2e8f0;
      border-radius: 3px;
      overflow: hidden;
    }

    .progress-bar {
      height: 100%;
      background: #6366f1;
      border-radius: 3px;
      transition: width 0.3s;
    }

    .progress-label {
      font-size: 12px;
      color: #64748b;
    }

    .card-notes {
      font-size: 13px;
      color: #475569;
      background: #f8fafc;
      padding: 10px 12px;
      border-radius: 8px;
      border: 1px solid #f1f5f9;
    }

    .notes-label {
      font-weight: 600;
      color: #64748b;
    }

    .card-actions {
      display: flex;
      justify-content: flex-end;
    }

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

    .btn-primary { background: #6366f1; color: white; }
    .btn-primary:hover { background: #4f46e5; }

    .state-message {
      text-align: center;
      padding: 4rem 2rem;
      color: #64748b;
      font-size: 15px;
      background: white;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
    }

    .empty-icon {
      width: 56px;
      height: 56px;
      border-radius: 50%;
      background: #dcfce7;
      color: #16a34a;
      font-size: 28px;
      font-weight: 700;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      margin-bottom: 12px;
    }

    .text-muted-sm {
      font-size: 13px;
      color: #94a3b8;
      margin-top: 4px;
    }

    @media (max-width: 768px) {
      .grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class PendingEvaluationsPageComponent implements OnInit {
  private readonly receptionService = inject(ReceptionService);

  receptions = signal<ReceptionListItem[]>([]);
  loading = signal(false);

  ngOnInit(): void {
    this.loadPendingReceptions();
  }

  loadPendingReceptions(): void {
    this.loading.set(true);
    this.receptionService.getReceptions({
      status: 'PendingEvaluation',
      pageSize: 50,
    }).subscribe({
      next: (result) => {
        this.receptions.set(result.data);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  getProgress(reception: ReceptionListItem): number {
    if (reception.itemCount === 0) return 0;
    return (reception.evaluatedCount / reception.itemCount) * 100;
  }
}
