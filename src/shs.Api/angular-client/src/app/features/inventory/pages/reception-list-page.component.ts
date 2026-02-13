import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ReceptionService } from '../services/reception.service';
import { ReceptionListItem, ReceptionStatus } from '../../../core/models/reception.model';

@Component({
  selector: 'oui-reception-list-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Recepções</h1>
        <p class="page-subtitle">{{ totalCount() }} recepções registadas</p>
      </div>
      <div class="page-header-actions">
        <a class="btn btn-primary" routerLink="/consignments/receive">+ Nova Recepção</a>
      </div>
    </div>

    <!-- Filters -->
    <div class="card filters-card">
      <div class="filters-bar">
        <input
          type="text"
          placeholder="Pesquisar por fornecedor..."
          [(ngModel)]="searchText"
          (ngModelChange)="onFilterChange()"
          class="filter-input filter-search"
        />
        <select
          [(ngModel)]="statusFilter"
          (ngModelChange)="onFilterChange()"
          class="filter-input"
        >
          <option value="">Todos os estados</option>
          <option value="PendingEvaluation">Pendente Avaliação</option>
          <option value="Evaluated">Avaliada</option>
          <option value="ConsignmentCreated">Consignação Criada</option>
        </select>
        @if (searchText || statusFilter) {
          <button class="btn btn-outline btn-sm" (click)="clearFilters()">Limpar</button>
        }
      </div>
    </div>

    @if (loading()) {
      <div class="state-message">A carregar...</div>
    } @else if (receptions().length === 0) {
      <div class="state-message">
        @if (searchText || statusFilter) {
          Nenhuma recepção encontrada com os filtros aplicados.
        } @else {
          Nenhuma recepção registada. Clique em "+ Nova Recepção" para começar.
        }
      </div>
    } @else {
      <div class="card table-card">
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Fornecedor</th>
                <th>Data</th>
                <th>Itens</th>
                <th>Avaliados</th>
                <th>Aceites</th>
                <th>Rejeitados</th>
                <th>Estado</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              @for (reception of receptions(); track reception.externalId) {
                <tr>
                  <td>
                    <div class="supplier-cell">
                      <span class="initial-badge">{{ reception.supplier.initial }}</span>
                      <a class="link-name" [routerLink]="['/inventory/suppliers', reception.supplier.externalId]">
                        <b>{{ reception.supplier.name }}</b>
                      </a>
                    </div>
                  </td>
                  <td>{{ reception.receptionDate | date: 'dd/MM/yyyy HH:mm' }}</td>
                  <td class="cell-center">
                    <span class="badge badge-gray">{{ reception.itemCount }}</span>
                  </td>
                  <td class="cell-center">{{ reception.evaluatedCount }}</td>
                  <td class="cell-center">
                    @if (reception.acceptedCount > 0) {
                      <span class="badge badge-green">{{ reception.acceptedCount }}</span>
                    } @else {
                      <span class="text-muted">0</span>
                    }
                  </td>
                  <td class="cell-center">
                    @if (reception.rejectedCount > 0) {
                      <span class="badge badge-red">{{ reception.rejectedCount }}</span>
                    } @else {
                      <span class="text-muted">0</span>
                    }
                  </td>
                  <td>
                    <span class="badge" [ngClass]="getStatusClass(reception.status)">
                      {{ getStatusLabel(reception.status) }}
                    </span>
                  </td>
                  <td class="cell-actions">
                    <a class="btn btn-outline btn-sm" [routerLink]="['/consignments/receptions', reception.externalId]">
                      Ver
                    </a>
                    <button class="btn btn-outline btn-sm" (click)="openReceipt(reception)">
                      Recibo
                    </button>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        @if (totalPages() > 1) {
          <div class="pagination">
            <button
              class="btn btn-outline btn-sm"
              [disabled]="currentPage() <= 1"
              (click)="goToPage(currentPage() - 1)"
            >
              Anterior
            </button>
            <span class="pagination-info">
              Página {{ currentPage() }} de {{ totalPages() }}
            </span>
            <button
              class="btn btn-outline btn-sm"
              [disabled]="currentPage() >= totalPages()"
              (click)="goToPage(currentPage() + 1)"
            >
              Próxima
            </button>
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

    .page-header-actions {
      display: flex;
      gap: 8px;
    }

    .card {
      background: #ffffff;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      padding: 20px;
    }

    .filters-card {
      margin-bottom: 20px;
      padding: 16px;
    }

    .table-card { padding: 0; }

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

    .btn-sm { padding: 5px 10px; font-size: 12px; }

    .filters-bar {
      display: flex;
      gap: 12px;
      flex-wrap: wrap;
      align-items: center;
    }

    .filter-input {
      padding: 8px 12px;
      border: 1px solid #e2e8f0;
      border-radius: 8px;
      font-size: 13px;
      background: white;
      outline: none;
      color: #1e293b;
    }

    .filter-input:focus { border-color: #6366f1; }
    .filter-search { width: 300px; }

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

    .cell-center { text-align: center; }

    .supplier-cell {
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .initial-badge {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 36px;
      height: 36px;
      border-radius: 8px;
      background: #6366f1;
      color: white;
      font-size: 13px;
      font-weight: 700;
      letter-spacing: 0.5px;
      flex-shrink: 0;
    }

    .link-name {
      text-decoration: none;
      color: #1e293b;
      transition: color 0.15s;
    }

    .link-name:hover { color: #6366f1; }

    .cell-actions {
      display: flex;
      gap: 4px;
      white-space: nowrap;
    }

    .badge {
      display: inline-block;
      padding: 3px 10px;
      border-radius: 20px;
      font-size: 11px;
      font-weight: 600;
      white-space: nowrap;
    }

    .badge-gray { background: #f1f5f9; color: #475569; }
    .badge-green { background: #dcfce7; color: #166534; }
    .badge-red { background: #fee2e2; color: #991b1b; }
    .badge-yellow { background: #fef9c3; color: #854d0e; }
    .badge-blue { background: #dbeafe; color: #1e40af; }
    .badge-purple { background: #f3e8ff; color: #6b21a8; }

    .text-muted { color: #94a3b8; }

    .pagination {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 12px;
      padding: 16px;
      border-top: 1px solid #e2e8f0;
    }

    .pagination-info {
      font-size: 13px;
      color: #64748b;
    }

    .state-message {
      text-align: center;
      padding: 4rem 2rem;
      color: #64748b;
      font-size: 15px;
      background: white;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
    }

    @media (max-width: 768px) {
      .page-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 12px;
      }

      .filter-search { width: 100%; }

      .filters-bar { flex-direction: column; }
    }
  `]
})
export class ReceptionListPageComponent implements OnInit {
  private readonly receptionService = inject(ReceptionService);

  receptions = signal<ReceptionListItem[]>([]);
  loading = signal(false);
  totalCount = signal(0);
  currentPage = signal(1);
  totalPages = signal(1);

  searchText = '';
  statusFilter = '';

  ngOnInit(): void {
    this.loadReceptions();
  }

  loadReceptions(): void {
    this.loading.set(true);
    this.receptionService.getReceptions({
      search: this.searchText || undefined,
      status: this.statusFilter || undefined,
      page: this.currentPage(),
      pageSize: 20,
    }).subscribe({
      next: (result) => {
        this.receptions.set(result.data);
        this.totalCount.set(result.totalCount);
        this.totalPages.set(result.totalPages);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  onFilterChange(): void {
    this.currentPage.set(1);
    this.loadReceptions();
  }

  clearFilters(): void {
    this.searchText = '';
    this.statusFilter = '';
    this.currentPage.set(1);
    this.loadReceptions();
  }

  goToPage(page: number): void {
    this.currentPage.set(page);
    this.loadReceptions();
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
      ConsignmentCreated: 'badge-purple',
    };
    return classes[status] || 'badge-gray';
  }

  openReceipt(reception: ReceptionListItem): void {
    this.receptionService.getReceiptHtml(reception.externalId).subscribe({
      next: (html) => {
        const w = window.open('', '_blank');
        if (w) {
          w.document.write(html);
          w.document.close();
        }
      }
    });
  }
}
