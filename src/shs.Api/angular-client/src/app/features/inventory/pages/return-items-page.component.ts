import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { SupplierService } from '../services/supplier.service';
import { SupplierReturnService } from '../services/supplier-return.service';
import { SupplierListItem } from '../../../core/models/supplier.model';
import { ReturnableItem } from '../../../core/models/supplier-return.model';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'oui-return-items-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page">
      <div class="page-header">
        <div>
          <h1>Devolver Peças ao Fornecedor</h1>
          <p class="subtitle">Selecione o fornecedor e as peças a devolver</p>
        </div>
        <a class="btn btn-outline" routerLink="/consignments/returns">Ver Histórico</a>
      </div>

      <!-- Step 1: Select Supplier -->
      <div class="card">
        <div class="card-title">1. Selecionar Fornecedor</div>
        <div class="supplier-select">
          <select
            class="form-select"
            [ngModel]="selectedSupplierExternalId()"
            (ngModelChange)="onSupplierChange($event)"
            [disabled]="returnCreated()"
          >
            <option value="">-- Selecione um fornecedor --</option>
            @for (s of suppliers(); track s.externalId) {
              <option [value]="s.externalId">{{ s.name }} ({{ s.initial }})</option>
            }
          </select>
        </div>
      </div>

      @if (selectedSupplierExternalId() && !returnCreated()) {
        <!-- Step 2: Select Items -->
        <div class="card">
          <div class="card-title-row">
            <span class="card-title">2. Selecionar Peças para Devolver</span>
            <span class="item-count">{{ selectedItems().size }} selecionados</span>
          </div>

          @if (loadingItems()) {
            <div class="loading">A carregar peças...</div>
          } @else if (returnableItems().length === 0) {
            <div class="empty">
              <span class="empty-icon">📭</span>
              <span>Este fornecedor não tem peças disponíveis para devolução.</span>
            </div>
          } @else {
            <div class="selection-toolbar">
              <button class="btn btn-outline btn-sm" (click)="selectAll()">Selecionar Todos</button>
              <button class="btn btn-outline btn-sm" (click)="deselectAll()">Limpar Seleção</button>
              <span class="toolbar-info">{{ returnableItems().length }} peças disponíveis</span>
            </div>

            <div class="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th class="cell-check"><input type="checkbox" [checked]="allSelected()" (change)="toggleAll()"></th>
                    <th>ID</th>
                    <th>Nome</th>
                    <th>Marca</th>
                    <th>Tam.</th>
                    <th>Cor</th>
                    <th>Cond.</th>
                    <th class="cell-right">Preço</th>
                    <th>Estado</th>
                    <th class="cell-right">Dias</th>
                  </tr>
                </thead>
                <tbody>
                  @for (item of returnableItems(); track item.externalId) {
                    <tr
                      [class.selected]="selectedItems().has(item.externalId)"
                      [class.row-rejected]="item.isRejected"
                      (click)="toggleItem(item.externalId)"
                    >
                      <td class="cell-check">
                        <input
                          type="checkbox"
                          [checked]="selectedItems().has(item.externalId)"
                          (click)="$event.stopPropagation()"
                          (change)="toggleItem(item.externalId)"
                        >
                      </td>
                      <td class="cell-mono">{{ item.identificationNumber }}</td>
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
                          <span class="badge badge-green">À Venda</span>
                        }
                      </td>
                      <td class="cell-right cell-days" [class.days-warning]="item.daysInStock >= 30" [class.days-danger]="item.daysInStock >= 60">
                        {{ item.daysInStock }}d
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }
        </div>

        <!-- Step 3: Confirm -->
        @if (selectedItems().size > 0) {
          <div class="card confirm-card">
            <div class="card-title">3. Confirmar Devolução</div>

            <div class="confirm-summary">
              <div class="stat">
                <div class="stat-value">{{ selectedItems().size }}</div>
                <div class="stat-label">Peças</div>
              </div>
              <div class="stat">
                <div class="stat-value">{{ selectedTotalValue() | currency: 'EUR' }}</div>
                <div class="stat-label">Valor Total</div>
              </div>
            </div>

            <div class="form-group">
              <label>Observações (opcional)</label>
              <textarea
                class="form-control"
                rows="2"
                placeholder="Motivo da devolução, notas adicionais..."
                [(ngModel)]="notes"
              ></textarea>
            </div>

            @if (error()) {
              <div class="alert alert-error">{{ error() }}</div>
            }

            <button
              class="btn btn-primary btn-lg"
              (click)="confirmReturn()"
              [disabled]="submitting()"
            >
              {{ submitting() ? 'A processar...' : 'Confirmar Devolução de ' + selectedItems().size + ' peças' }}
            </button>
          </div>
        }
      }

      <!-- Success state -->
      @if (returnCreated()) {
        <div class="card success-card">
          <div class="success-icon">&#10003;</div>
          <h2>Devolução Registada com Sucesso</h2>
          <p class="success-detail">
            {{ createdReturn()!.itemCount }} peças devolvidas a
            <b>{{ createdReturn()!.supplier.name }}</b>
          </p>

          <div class="success-items">
            <table>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Nome</th>
                  <th>Marca</th>
                  <th>Tam.</th>
                  <th class="cell-right">Preço</th>
                </tr>
              </thead>
              <tbody>
                @for (item of createdReturn()!.items; track item.externalId) {
                  <tr>
                    <td class="cell-mono">{{ item.identificationNumber }}</td>
                    <td><b>{{ item.name }}</b></td>
                    <td>{{ item.brand }}</td>
                    <td>{{ item.size }}</td>
                    <td class="cell-right">{{ item.evaluatedPrice | currency: 'EUR' }}</td>
                  </tr>
                }
              </tbody>
            </table>
          </div>

          <div class="success-actions">
            <button class="btn btn-primary" (click)="resetForm()">Nova Devolução</button>
            <a class="btn btn-outline" [routerLink]="['/consignments/returns', createdReturn()!.externalId]">Ver Detalhes</a>
            <a class="btn btn-outline" routerLink="/consignments/returns">Ver Histórico</a>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .page { max-width: 1100px; margin: 0 auto; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 24px; }
    .page-header h1 { font-size: 24px; font-family: 'DM Serif Display', Georgia, serif; font-weight: 400; margin: 0; }
    .subtitle { font-size: 13px; color: #78716C; margin: 4px 0 0; }
    .card { background: #fff; border: 1px solid #E7E5E4; border-radius: 14px; padding: 24px; margin-bottom: 20px; }
    .card-title { font-size: 15px; font-weight: 700; color: #1C1917; margin-bottom: 16px; }
    .card-title-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
    .item-count { font-size: 13px; font-weight: 600; color: #5B7153; background: #E8EFE6; padding: 4px 12px; border-radius: 20px; }

    .supplier-select { max-width: 400px; }
    .form-select { width: 100%; padding: 10px 12px; border: 1px solid #D6D3D1; border-radius: 10px; font-size: 14px; background: #fff; }
    .form-select:focus { outline: none; border-color: #5B7153; box-shadow: 0 0 0 3px rgba(91, 113, 83, 0.12); }

    .loading { text-align: center; padding: 32px; color: #78716C; font-size: 14px; }
    .empty { text-align: center; padding: 40px 16px; color: #78716C; display: flex; flex-direction: column; align-items: center; gap: 8px; }
    .empty-icon { font-size: 32px; }

    .selection-toolbar { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; }
    .toolbar-info { margin-left: auto; font-size: 12px; color: #A8A29E; }

    .table-wrapper { overflow-x: auto; }
    table { width: 100%; border-collapse: collapse; font-size: 13px; }
    th { text-align: left; padding: 8px 10px; font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; color: #78716C; border-bottom: 2px solid #E7E5E4; white-space: nowrap; }
    td { padding: 10px; border-bottom: 1px solid #F5F5F4; }
    .cell-check { width: 36px; text-align: center; }
    .cell-mono { font-family: monospace; font-size: 12px; color: #78716C; }
    .cell-right { text-align: right; }
    .cell-days { font-weight: 600; color: #78716C; }
    .days-warning { color: #f59e0b; }
    .days-danger { color: #C45B5B; }

    tbody tr { cursor: pointer; transition: background 0.15s; }
    tbody tr:hover { background: #FAF9F7; }
    tbody tr.selected { background: #E8EFE6; }
    tbody tr.row-rejected { background: rgba(196, 91, 91, 0.06); }
    tbody tr.row-rejected.selected { background: #fde8e8; }

    .badge { display: inline-block; padding: 2px 8px; border-radius: 14px; font-size: 11px; font-weight: 600; }
    .badge-green { background: #E8EFE6; color: #5B7153; }
    .badge-red { background: rgba(196, 91, 91, 0.06); color: #C45B5B; }

    .confirm-card { border-color: #5B7153; }
    .confirm-summary { display: flex; gap: 24px; margin-bottom: 20px; }
    .stat { text-align: center; padding: 16px 24px; background: #FAF9F7; border: 1px solid #E7E5E4; border-radius: 10px; min-width: 120px; }
    .stat-value { font-size: 24px; font-weight: 700; color: #1C1917; }
    .stat-label { font-size: 12px; color: #78716C; margin-top: 2px; }

    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; font-size: 13px; font-weight: 600; color: #44403C; margin-bottom: 6px; }
    .form-control { width: 100%; padding: 10px 12px; border: 1px solid #D6D3D1; border-radius: 10px; font-size: 14px; font-family: inherit; resize: vertical; }
    .form-control:focus { outline: none; border-color: #5B7153; box-shadow: 0 0 0 3px rgba(91, 113, 83, 0.12); }

    .alert { padding: 12px 16px; border-radius: 10px; font-size: 13px; margin-bottom: 16px; }
    .alert-error { background: rgba(196, 91, 91, 0.06); color: #C45B5B; border: 1px solid rgba(196, 91, 91, 0.3); }

    .btn { display: inline-flex; align-items: center; gap: 6px; padding: 8px 16px; border: 1px solid #D6D3D1; border-radius: 10px; font-size: 13px; font-weight: 600; cursor: pointer; background: #fff; color: #44403C; text-decoration: none; transition: all 0.15s; }
    .btn:hover { background: #FAF9F7; }
    .btn-primary { background: #5B7153; color: #fff; border-color: #5B7153; }
    .btn-primary:hover { background: #4A5E43; }
    .btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn-outline { background: transparent; }
    .btn-sm { padding: 4px 10px; font-size: 12px; }
    .btn-lg { padding: 12px 24px; font-size: 14px; width: 100%; justify-content: center; }

    .success-card { text-align: center; border-color: #5B7153; }
    .success-icon { width: 56px; height: 56px; border-radius: 50%; background: #E8EFE6; color: #5B7153; font-size: 28px; display: flex; align-items: center; justify-content: center; margin: 0 auto 16px; border: 2px solid #C2D4BC; }
    .success-card h2 { font-size: 18px; margin: 0 0 8px; }
    .success-detail { font-size: 14px; color: #78716C; margin: 0 0 20px; }
    .success-items { margin-bottom: 20px; text-align: left; }
    .success-items table { font-size: 12px; }
    .success-actions { display: flex; justify-content: center; gap: 12px; flex-wrap: wrap; }
  `]
})
export class ReturnItemsPageComponent implements OnInit {
  private readonly supplierService = inject(SupplierService);
  private readonly returnService = inject(SupplierReturnService);
  private readonly router = inject(Router);
  private readonly baseUrl = environment.apiUrl.replace('/api', '');

  suppliers = signal<SupplierListItem[]>([]);
  selectedSupplierExternalId = signal('');
  returnableItems = signal<ReturnableItem[]>([]);
  loadingItems = signal(false);
  selectedItems = signal<Set<string>>(new Set());
  notes = '';
  submitting = signal(false);
  error = signal<string | null>(null);
  returnCreated = signal(false);
  createdReturn = signal<any>(null);

  allSelected = computed(() => {
    const items = this.returnableItems();
    const selected = this.selectedItems();
    return items.length > 0 && items.every(i => selected.has(i.externalId));
  });

  selectedTotalValue = computed(() => {
    const selected = this.selectedItems();
    return this.returnableItems()
      .filter(i => selected.has(i.externalId))
      .reduce((sum, i) => sum + i.evaluatedPrice, 0);
  });

  ngOnInit(): void {
    this.supplierService.getAll().subscribe({
      next: (suppliers) => this.suppliers.set(suppliers),
    });
  }

  onSupplierChange(supplierId: string): void {
    this.selectedSupplierExternalId.set(supplierId);
    this.selectedItems.set(new Set());
    this.error.set(null);

    if (supplierId) {
      this.loadingItems.set(true);
      this.returnService.getReturnableItems(supplierId).subscribe({
        next: (items) => {
          this.returnableItems.set(items);
          this.loadingItems.set(false);
        },
        error: () => {
          this.loadingItems.set(false);
          this.error.set('Erro ao carregar peças.');
        },
      });
    } else {
      this.returnableItems.set([]);
    }
  }

  toggleItem(externalId: string): void {
    const current = new Set(this.selectedItems());
    if (current.has(externalId)) {
      current.delete(externalId);
    } else {
      current.add(externalId);
    }
    this.selectedItems.set(current);
  }

  toggleAll(): void {
    if (this.allSelected()) {
      this.deselectAll();
    } else {
      this.selectAll();
    }
  }

  selectAll(): void {
    const all = new Set(this.returnableItems().map(i => i.externalId));
    this.selectedItems.set(all);
  }

  deselectAll(): void {
    this.selectedItems.set(new Set());
  }

  confirmReturn(): void {
    const supplier = this.selectedSupplierExternalId();
    const items = Array.from(this.selectedItems());

    if (!supplier || items.length === 0) return;

    this.submitting.set(true);
    this.error.set(null);

    this.returnService.create({
      supplierExternalId: supplier,
      itemExternalIds: items,
      notes: this.notes.trim() || undefined,
    }).subscribe({
      next: (result) => {
        this.submitting.set(false);
        this.createdReturn.set(result);
        this.returnCreated.set(true);
      },
      error: (err) => {
        this.submitting.set(false);
        this.error.set(err.error?.error || 'Erro ao processar a devolução.');
      },
    });
  }

  resetForm(): void {
    this.returnCreated.set(false);
    this.createdReturn.set(null);
    this.selectedItems.set(new Set());
    this.notes = '';
    this.error.set(null);
    this.onSupplierChange(this.selectedSupplierExternalId());
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

  getPhotoUrl(path?: string): string {
    if (!path) return '';
    return `${this.baseUrl}${path}`;
  }
}
