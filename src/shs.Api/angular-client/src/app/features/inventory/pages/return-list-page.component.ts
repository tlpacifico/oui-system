import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SupplierReturnService } from '../services/supplier-return.service';
import { SupplierReturnListItem } from '../../../core/models/supplier-return.model';

@Component({
  selector: 'oui-return-list-page',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="page">
      <div class="page-header">
        <div>
          <h1>Devoluções ao Fornecedor</h1>
          <p class="subtitle">Histórico de peças devolvidas</p>
        </div>
        <a class="btn btn-primary" routerLink="/consignments/returns/new">Nova Devolução</a>
      </div>

      <!-- Filters -->
      <div class="filters">
        <input
          class="search-input"
          placeholder="Pesquisar por fornecedor..."
          [(ngModel)]="searchTerm"
          (ngModelChange)="onSearchChange()"
        />
      </div>

      @if (loading()) {
        <div class="loading">A carregar...</div>
      } @else if (returns().length === 0) {
        <div class="card empty">
          <span class="empty-icon">📭</span>
          <h3>Sem devoluções registadas</h3>
          <p>Ainda não foi feita nenhuma devolução de peças ao fornecedor.</p>
          <a class="btn btn-primary" routerLink="/consignments/returns/new">Criar Devolução</a>
        </div>
      } @else {
        <div class="card table-card">
          <div class="table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>Ref.</th>
                  <th>Fornecedor</th>
                  <th>Data</th>
                  <th class="cell-center">Peças</th>
                  <th>Notas</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                @for (ret of returns(); track ret.externalId) {
                  <tr>
                    <td class="cell-mono">{{ ret.externalId.substring(0, 8).toUpperCase() }}</td>
                    <td>
                      <div class="supplier-cell">
                        <span class="initial-badge">{{ ret.supplier.initial }}</span>
                        <a class="supplier-link" [routerLink]="['/inventory/suppliers', ret.supplier.externalId]">
                          {{ ret.supplier.name }}
                        </a>
                      </div>
                    </td>
                    <td>{{ ret.returnDate | date: 'dd/MM/yyyy HH:mm' }}</td>
                    <td class="cell-center">
                      <span class="count-badge">{{ ret.itemCount }}</span>
                    </td>
                    <td class="cell-notes">{{ ret.notes || '—' }}</td>
                    <td>
                      <a class="btn btn-outline btn-sm" [routerLink]="['/consignments/returns', ret.externalId]">
                        Ver
                      </a>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>

          <!-- Pagination -->
          @if (totalPages() > 1) {
            <div class="pagination">
              <button class="btn btn-sm" [disabled]="currentPage() <= 1" (click)="goToPage(currentPage() - 1)">Anterior</button>
              <span class="page-info">Página {{ currentPage() }} de {{ totalPages() }}</span>
              <button class="btn btn-sm" [disabled]="currentPage() >= totalPages()" (click)="goToPage(currentPage() + 1)">Seguinte</button>
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .page { max-width: 1100px; margin: 0 auto; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 24px; }
    .page-header h1 { font-size: 24px; font-family: 'DM Serif Display', Georgia, serif; font-weight: 400; margin: 0; }
    .subtitle { font-size: 13px; color: #78716C; margin: 4px 0 0; }

    .filters { margin-bottom: 16px; }
    .search-input { padding: 10px 14px; border: 1px solid #D6D3D1; border-radius: 10px; font-size: 14px; width: 300px; }
    .search-input:focus { outline: none; border-color: #5B7153; box-shadow: 0 0 0 3px rgba(91, 113, 83, 0.12); }

    .loading { text-align: center; padding: 48px; color: #78716C; font-size: 14px; }
    .card { background: #fff; border: 1px solid #E7E5E4; border-radius: 14px; padding: 24px; margin-bottom: 20px; }
    .table-card { padding: 0; overflow: hidden; }
    .empty { text-align: center; padding: 48px 24px; }
    .empty-icon { font-size: 40px; }
    .empty h3 { font-size: 16px; margin: 16px 0 8px; }
    .empty p { font-size: 13px; color: #78716C; margin: 0 0 20px; }

    .table-wrapper { overflow-x: auto; }
    table { width: 100%; border-collapse: collapse; font-size: 13px; }
    th { text-align: left; padding: 12px 16px; font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; color: #78716C; border-bottom: 2px solid #E7E5E4; white-space: nowrap; }
    td { padding: 12px 16px; border-bottom: 1px solid #F5F5F4; }
    .cell-mono { font-family: monospace; font-size: 12px; color: #78716C; }
    .cell-center { text-align: center; }
    .cell-notes { max-width: 200px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; color: #78716C; font-size: 12px; }

    .supplier-cell { display: flex; align-items: center; gap: 8px; }
    .initial-badge { width: 28px; height: 28px; border-radius: 50%; background: #E8EFE6; color: #5B7153; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 700; flex-shrink: 0; }
    .supplier-link { color: #5B7153; text-decoration: none; font-weight: 600; }
    .supplier-link:hover { text-decoration: underline; }

    .count-badge { display: inline-flex; align-items: center; justify-content: center; min-width: 28px; height: 24px; background: #F5F5F4; color: #44403C; border-radius: 14px; font-size: 12px; font-weight: 600; padding: 0 8px; }

    .pagination { display: flex; align-items: center; justify-content: center; gap: 12px; padding: 16px; border-top: 1px solid #E7E5E4; }
    .page-info { font-size: 13px; color: #78716C; }

    .btn { display: inline-flex; align-items: center; gap: 6px; padding: 8px 16px; border: 1px solid #D6D3D1; border-radius: 10px; font-size: 13px; font-weight: 600; cursor: pointer; background: #fff; color: #44403C; text-decoration: none; transition: all 0.15s; }
    .btn:hover { background: #FAF9F7; }
    .btn:disabled { opacity: 0.5; cursor: not-allowed; }
    .btn-primary { background: #5B7153; color: #fff; border-color: #5B7153; }
    .btn-primary:hover { background: #4A5E43; }
    .btn-outline { background: transparent; }
    .btn-sm { padding: 4px 10px; font-size: 12px; }

    tbody tr { transition: background 0.15s; }
    tbody tr:hover { background: #FAF9F7; }
  `]
})
export class ReturnListPageComponent implements OnInit {
  private readonly returnService = inject(SupplierReturnService);

  returns = signal<SupplierReturnListItem[]>([]);
  loading = signal(true);
  currentPage = signal(1);
  totalPages = signal(1);
  searchTerm = '';
  private searchTimeout: any;

  ngOnInit(): void {
    this.loadData();
  }

  onSearchChange(): void {
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.currentPage.set(1);
      this.loadData();
    }, 300);
  }

  goToPage(page: number): void {
    this.currentPage.set(page);
    this.loadData();
  }

  private loadData(): void {
    this.loading.set(true);
    this.returnService.getAll(this.currentPage(), 20, undefined, this.searchTerm || undefined).subscribe({
      next: (result) => {
        this.returns.set(result.data);
        this.totalPages.set(result.totalPages);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }
}
