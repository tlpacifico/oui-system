import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoryService } from '../services/category.service';
import { CategoryListItem } from '../../../core/models/category.model';

@Component({
  selector: 'oui-category-list-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Categorias</h1>
        <p class="page-subtitle">{{ categories().length }} categorias registadas</p>
      </div>
      <div class="page-header-actions">
        <button class="btn btn-primary" (click)="openCreate()">+ Nova Categoria</button>
      </div>
    </div>

    <!-- Search -->
    <div class="card filters-card">
      <div class="filters-bar">
        <input
          type="text"
          placeholder="Pesquisar categorias..."
          [(ngModel)]="searchText"
          (ngModelChange)="onSearchChange()"
          class="filter-input filter-search"
        />
        @if (searchText) {
          <button class="btn btn-outline btn-sm" (click)="clearSearch()">Limpar</button>
        }
      </div>
    </div>

    @if (loading()) {
      <div class="state-message">A carregar...</div>
    } @else if (categories().length === 0) {
      <div class="state-message">
        @if (searchText) {
          Nenhuma categoria encontrada para "{{ searchText }}".
        } @else {
          Nenhuma categoria registada. Clique em "+ Nova Categoria" para começar.
        }
      </div>
    } @else {
      <div class="card table-card">
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Nome</th>
                <th>Descrição</th>
                <th>Categoria Pai</th>
                <th>Subcategorias</th>
                <th>Itens</th>
                <th>Criada em</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              @for (cat of categories(); track cat.externalId) {
                <tr>
                  <td><b>{{ cat.name }}</b></td>
                  <td class="cell-description">{{ cat.description || '—' }}</td>
                  <td>
                    @if (cat.parentCategory) {
                      <span class="badge badge-info">{{ cat.parentCategory.name }}</span>
                    } @else {
                      <span class="text-muted">—</span>
                    }
                  </td>
                  <td>
                    <span class="badge badge-gray">{{ cat.subCategoryCount }}</span>
                  </td>
                  <td>
                    <span class="badge badge-gray">{{ cat.itemCount }}</span>
                  </td>
                  <td>{{ cat.createdOn | date: 'dd/MM/yyyy' }}</td>
                  <td class="cell-actions">
                    <button class="btn btn-outline btn-sm" (click)="openEdit(cat)">Editar</button>
                    <button
                      class="btn btn-outline btn-sm btn-danger-outline"
                      (click)="confirmDelete(cat)"
                      [disabled]="cat.itemCount > 0 || cat.subCategoryCount > 0"
                      [title]="getDeleteTooltip(cat)"
                    >
                      Eliminar
                    </button>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    }

    <!-- Modal -->
    @if (showModal()) {
      <div class="modal-overlay" (click)="closeModal()">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h2 class="modal-title">{{ isEditing() ? 'Editar Categoria' : 'Nova Categoria' }}</h2>
            <button class="modal-close" (click)="closeModal()">&times;</button>
          </div>

          <div class="modal-body">
            @if (modalError()) {
              <div class="alert alert-error">{{ modalError() }}</div>
            }

            <div class="form-group">
              <label for="catName">Nome *</label>
              <input
                id="catName"
                type="text"
                [(ngModel)]="formName"
                class="form-input"
                placeholder="Ex: Vestidos, Calçado, Acessórios..."
                maxlength="200"
                [class.input-error]="formSubmitted && !formName.trim()"
              />
              @if (formSubmitted && !formName.trim()) {
                <span class="field-error">O nome é obrigatório.</span>
              }
            </div>

            <div class="form-group">
              <label for="catDescription">Descrição</label>
              <textarea
                id="catDescription"
                [(ngModel)]="formDescription"
                class="form-input form-textarea"
                placeholder="Descrição opcional..."
                rows="3"
                maxlength="1000"
              ></textarea>
            </div>

            <div class="form-group">
              <label for="catParent">Categoria Pai</label>
              <select
                id="catParent"
                [(ngModel)]="formParentExternalId"
                class="form-input"
              >
                <option value="">Nenhuma (raiz)</option>
                @for (cat of availableParents(); track cat.externalId) {
                  <option [value]="cat.externalId">{{ cat.name }}</option>
                }
              </select>
            </div>
          </div>

          <div class="modal-footer">
            <button class="btn btn-outline" (click)="closeModal()" [disabled]="saving()">Cancelar</button>
            <button class="btn btn-primary" (click)="saveModal()" [disabled]="saving()">
              {{ saving() ? 'A guardar...' : (isEditing() ? 'Guardar Alterações' : 'Criar Categoria') }}
            </button>
          </div>
        </div>
      </div>
    }

    <!-- Delete Confirmation -->
    @if (showDeleteConfirm()) {
      <div class="modal-overlay" (click)="cancelDelete()">
        <div class="modal modal-sm" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h2 class="modal-title">Eliminar Categoria</h2>
            <button class="modal-close" (click)="cancelDelete()">&times;</button>
          </div>
          <div class="modal-body">
            <p>Tem certeza que deseja eliminar a categoria <b>{{ deletingCategory()?.name }}</b>?</p>
            <p class="text-muted">Esta ação não pode ser revertida.</p>
          </div>
          <div class="modal-footer">
            <button class="btn btn-outline" (click)="cancelDelete()" [disabled]="deleting()">Cancelar</button>
            <button class="btn btn-danger" (click)="executeDelete()" [disabled]="deleting()">
              {{ deleting() ? 'A eliminar...' : 'Eliminar' }}
            </button>
          </div>
        </div>
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

    .page-header-actions { display: flex; gap: 8px; }

    .card {
      background: #ffffff;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      padding: 20px;
    }

    .filters-card { margin-bottom: 20px; padding: 16px; }
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
    }

    .btn:disabled { opacity: 0.5; cursor: not-allowed; }
    .btn-primary { background: #6366f1; color: white; }
    .btn-primary:hover:not(:disabled) { background: #4f46e5; }
    .btn-outline { background: white; color: #1e293b; border-color: #e2e8f0; }
    .btn-outline:hover:not(:disabled) { background: #f8fafc; }
    .btn-sm { padding: 5px 10px; font-size: 12px; }
    .btn-danger { background: #ef4444; color: white; }
    .btn-danger:hover:not(:disabled) { background: #dc2626; }
    .btn-danger-outline { color: #ef4444; border-color: #fecaca; }
    .btn-danger-outline:hover:not(:disabled) { background: #fef2f2; }

    .filters-bar { display: flex; gap: 12px; flex-wrap: wrap; align-items: center; }

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
      padding: 12px 14px;
      border-bottom: 1px solid #e2e8f0;
      vertical-align: middle;
    }

    tr:hover td { background: #f1f5f9; }

    .cell-description {
      color: #64748b;
      max-width: 250px;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .cell-actions { display: flex; gap: 4px; white-space: nowrap; }

    .badge {
      display: inline-block;
      padding: 3px 10px;
      border-radius: 20px;
      font-size: 11px;
      font-weight: 600;
      white-space: nowrap;
    }

    .badge-gray { background: #f1f5f9; color: #475569; }
    .badge-info { background: #dbeafe; color: #1e40af; }

    .text-muted { color: #94a3b8; font-size: 13px; }

    /* Modal */
    .modal-overlay {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
      padding: 20px;
    }

    .modal {
      background: white;
      border-radius: 16px;
      width: 100%;
      max-width: 520px;
      box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15);
      animation: modalIn 0.2s ease-out;
    }

    .modal-sm { max-width: 420px; }

    @keyframes modalIn {
      from { opacity: 0; transform: translateY(-10px) scale(0.98); }
      to { opacity: 1; transform: translateY(0) scale(1); }
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 20px 24px;
      border-bottom: 1px solid #e2e8f0;
    }

    .modal-title { font-size: 18px; font-weight: 700; color: #1e293b; margin: 0; }

    .modal-close {
      background: none;
      border: none;
      font-size: 24px;
      color: #94a3b8;
      cursor: pointer;
      padding: 0;
      line-height: 1;
    }

    .modal-close:hover { color: #1e293b; }
    .modal-body { padding: 24px; }

    .modal-footer {
      padding: 16px 24px;
      border-top: 1px solid #e2e8f0;
      display: flex;
      justify-content: flex-end;
      gap: 8px;
    }

    .form-group { margin-bottom: 16px; }
    .form-group:last-child { margin-bottom: 0; }

    .form-group label {
      display: block;
      font-size: 13px;
      font-weight: 600;
      color: #374151;
      margin-bottom: 6px;
    }

    .form-input {
      width: 100%;
      padding: 10px 12px;
      border: 1px solid #e2e8f0;
      border-radius: 8px;
      font-size: 14px;
      outline: none;
      color: #1e293b;
      font-family: inherit;
      transition: border-color 0.15s;
    }

    .form-input:focus {
      border-color: #6366f1;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
    }

    .form-input.input-error { border-color: #ef4444; }
    .form-textarea { resize: vertical; min-height: 80px; }
    .field-error { display: block; font-size: 12px; color: #ef4444; margin-top: 4px; }

    .alert {
      padding: 10px 14px;
      border-radius: 8px;
      font-size: 13px;
      margin-bottom: 16px;
    }

    .alert-error { background: #fef2f2; color: #991b1b; border: 1px solid #fecaca; }

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
      .page-header { flex-direction: column; align-items: flex-start; gap: 12px; }
      .filter-search { width: 100%; }
    }
  `]
})
export class CategoryListPageComponent implements OnInit {
  private readonly categoryService = inject(CategoryService);

  categories = signal<CategoryListItem[]>([]);
  loading = signal(false);
  searchText = '';

  // Modal
  showModal = signal(false);
  isEditing = signal(false);
  editingExternalId = signal<string | null>(null);
  saving = signal(false);
  modalError = signal<string | null>(null);
  formSubmitted = false;

  formName = '';
  formDescription = '';
  formParentExternalId = '';

  // Available parents for dropdown (exclude self when editing)
  availableParents = signal<CategoryListItem[]>([]);

  // Delete
  showDeleteConfirm = signal(false);
  deletingCategory = signal<CategoryListItem | null>(null);
  deleting = signal(false);

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.loading.set(true);
    this.categoryService.getAll(this.searchText || undefined).subscribe({
      next: (categories) => {
        this.categories.set(categories);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  onSearchChange(): void {
    this.loadCategories();
  }

  clearSearch(): void {
    this.searchText = '';
    this.loadCategories();
  }

  openCreate(): void {
    this.formName = '';
    this.formDescription = '';
    this.formParentExternalId = '';
    this.formSubmitted = false;
    this.isEditing.set(false);
    this.editingExternalId.set(null);
    this.modalError.set(null);
    this.availableParents.set(this.categories());
    this.showModal.set(true);
  }

  openEdit(cat: CategoryListItem): void {
    this.formName = cat.name;
    this.formDescription = cat.description || '';
    this.formParentExternalId = cat.parentCategory?.externalId || '';
    this.formSubmitted = false;
    this.isEditing.set(true);
    this.editingExternalId.set(cat.externalId);
    this.modalError.set(null);
    // Exclude self from parent options
    this.availableParents.set(this.categories().filter(c => c.externalId !== cat.externalId));
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
  }

  saveModal(): void {
    this.formSubmitted = true;
    this.modalError.set(null);

    if (!this.formName.trim()) return;

    this.saving.set(true);

    const data = {
      name: this.formName.trim(),
      description: this.formDescription.trim() || undefined,
      parentCategoryExternalId: this.formParentExternalId || undefined,
    };

    if (this.isEditing() && this.editingExternalId()) {
      this.categoryService.update(this.editingExternalId()!, data).subscribe({
        next: () => {
          this.saving.set(false);
          this.showModal.set(false);
          this.loadCategories();
        },
        error: (err) => {
          this.saving.set(false);
          this.modalError.set(err.error?.error || 'Erro ao atualizar categoria.');
        }
      });
    } else {
      this.categoryService.create(data).subscribe({
        next: () => {
          this.saving.set(false);
          this.showModal.set(false);
          this.loadCategories();
        },
        error: (err) => {
          this.saving.set(false);
          this.modalError.set(err.error?.error || 'Erro ao criar categoria.');
        }
      });
    }
  }

  confirmDelete(cat: CategoryListItem): void {
    this.deletingCategory.set(cat);
    this.showDeleteConfirm.set(true);
  }

  cancelDelete(): void {
    this.showDeleteConfirm.set(false);
    this.deletingCategory.set(null);
  }

  executeDelete(): void {
    const cat = this.deletingCategory();
    if (!cat) return;

    this.deleting.set(true);
    this.categoryService.delete(cat.externalId).subscribe({
      next: () => {
        this.deleting.set(false);
        this.showDeleteConfirm.set(false);
        this.deletingCategory.set(null);
        this.loadCategories();
      },
      error: () => {
        this.deleting.set(false);
      }
    });
  }

  getDeleteTooltip(cat: CategoryListItem): string {
    if (cat.itemCount > 0) return 'Não é possível eliminar uma categoria com itens associados';
    if (cat.subCategoryCount > 0) return 'Não é possível eliminar uma categoria com subcategorias';
    return 'Eliminar categoria';
  }
}
