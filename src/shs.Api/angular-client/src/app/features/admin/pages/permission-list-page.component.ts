import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PermissionService } from '../services/permission.service';
import { Permission, CreatePermissionRequest } from '../../../core/models/permission.model';
import { HasPermissionDirective } from '../../../core/auth/directives/has-permission.directive';

@Component({
  selector: 'oui-permission-list-page',
  standalone: true,
  imports: [CommonModule, FormsModule, HasPermissionDirective],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Permissões</h1>
        <p class="page-subtitle">{{ permissions().length }} permissões no sistema</p>
      </div>
      <div class="page-header-actions">
        <button class="btn btn-primary" (click)="openCreate()" *hasPermission="'admin.permissions.create'">
          + Nova Permissão
        </button>
      </div>
    </div>

    <!-- Search -->
    <div class="card filters-card">
      <div class="filters-bar">
        <input
          type="text"
          placeholder="Pesquisar permissões..."
          [ngModel]="searchText()"
          (ngModelChange)="searchText.set($event); onSearchChange()"
          class="filter-input filter-search"
        />
        <select
          class="filter-select"
          [ngModel]="selectedCategory()"
          (ngModelChange)="selectedCategory.set($event); onCategoryChange()"
        >
          <option value="">Todas as categorias</option>
          @for (category of categories(); track category) {
            <option [value]="category">{{ category }}</option>
          }
        </select>
        @if (searchText() || selectedCategory()) {
          <button class="btn btn-outline btn-sm" (click)="clearFilters()">Limpar</button>
        }
      </div>
    </div>

    @if (loading()) {
      <div class="state-message">A carregar...</div>
    } @else if (permissionsByCategory().length === 0) {
      <div class="state-message">
        Nenhuma permissão encontrada.
      </div>
    } @else {
      @for (categoryGroup of permissionsByCategory(); track categoryGroup.name) {
        <div class="card category-card">
          <div class="category-header">
            <div class="category-title-row">
              <span class="category-icon">{{ getCategoryIcon(categoryGroup.name) }}</span>
              <h2 class="category-name">{{ categoryGroup.name }}</h2>
              <span class="badge badge-gray">{{ categoryGroup.permissions.length }}</span>
            </div>
          </div>
          <div class="table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>Permissão</th>
                  <th>Descrição</th>
                  <th class="cell-actions-header">Ações</th>
                </tr>
              </thead>
              <tbody>
                @for (permission of categoryGroup.permissions; track permission.externalId) {
                  <tr>
                    <td>
                      <code class="permission-code">{{ permission.name }}</code>
                    </td>
                    <td class="cell-description">{{ permission.description || '—' }}</td>
                    <td class="cell-actions" (click)="$event.stopPropagation()">
                      <button class="btn btn-outline btn-sm" (click)="openEdit(permission)" *hasPermission="'admin.permissions.update'">
                        Editar
                      </button>
                      <button
                        class="btn btn-outline btn-sm btn-danger-outline"
                        (click)="confirmDelete(permission)"
                        *hasPermission="'admin.permissions.delete'"
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
    }

    <!-- Create/Edit Modal -->
    @if (showModal()) {
      <div class="modal-overlay" (click)="closeModal()"></div>
      <div class="modal">
        <div class="modal-header">
          <h2>{{ isEditing() ? 'Editar Permissão' : 'Nova Permissão' }}</h2>
          <button class="modal-close" (click)="closeModal()">&times;</button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label class="form-label">Nome *</label>
            <input
              type="text"
              class="form-input"
              [ngModel]="formData().name"
              (ngModelChange)="setFormName($event)"
              placeholder="categoria.recurso.ação (ex: admin.users.view)"
              [class.error]="formErrors().name"
            />
            @if (formErrors().name) {
              <span class="form-error">{{ formErrors().name }}</span>
            }
          </div>
          <div class="form-group">
            <label class="form-label">Descrição</label>
            <textarea
              class="form-input"
              [ngModel]="formData().description"
              (ngModelChange)="setFormDescription($event)"
              placeholder="Descrição da permissão..."
              rows="3"
            ></textarea>
          </div>
          @if (formErrors().general) {
            <div class="alert alert-danger">{{ formErrors().general }}</div>
          }
        </div>
        <div class="modal-footer">
          <button class="btn btn-outline" (click)="closeModal()">Cancelar</button>
          <button class="btn btn-primary" (click)="savePermission()" [disabled]="saving()">
            {{ saving() ? 'A guardar...' : 'Guardar' }}
          </button>
        </div>
      </div>
    }

    <!-- Delete Confirmation -->
    @if (showDeleteConfirm()) {
      <div class="modal-overlay" (click)="cancelDelete()"></div>
      <div class="modal modal-sm">
        <div class="modal-header">
          <h2>Confirmar Eliminação</h2>
          <button class="modal-close" (click)="cancelDelete()">&times;</button>
        </div>
        <div class="modal-body">
          <p>Tem a certeza que deseja eliminar a permissão <strong>{{ permissionToDelete()?.name }}</strong>?</p>
        </div>
        <div class="modal-footer">
          <button class="btn btn-outline" (click)="cancelDelete()">Cancelar</button>
          <button class="btn btn-danger" (click)="deletePermission()" [disabled]="deleting()">
            {{ deleting() ? 'A eliminar...' : 'Eliminar' }}
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    :host { display: block; }

    /* ── Page header ── */
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

    /* ── Cards ── */
    .card {
      background: #ffffff;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
    }

    .filters-card {
      margin-bottom: 20px;
      padding: 16px;
    }

    .category-card {
      margin-bottom: 16px;
    }

    .category-header {
      padding: 16px 20px 12px;
      border-bottom: 1px solid #e2e8f0;
    }

    .category-title-row {
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .category-icon {
      font-size: 18px;
      width: 32px;
      height: 32px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: #f1f5f9;
      border-radius: 8px;
    }

    .category-name {
      font-size: 15px;
      font-weight: 700;
      color: #1e293b;
      text-transform: capitalize;
      margin: 0;
    }

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
    }

    .btn-primary {
      background: #6366f1;
      color: white;
    }

    .btn-primary:hover {
      background: #4f46e5;
    }

    .btn-primary:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .btn-outline {
      background: white;
      color: #1e293b;
      border-color: #e2e8f0;
    }

    .btn-outline:hover {
      background: #f8fafc;
    }

    .btn-sm {
      padding: 5px 10px;
      font-size: 12px;
    }

    .btn-danger {
      background: #ef4444;
      color: white;
      border-color: #ef4444;
    }

    .btn-danger:hover {
      background: #dc2626;
    }

    .btn-danger:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .btn-danger-outline {
      color: #ef4444;
      border-color: #fecaca;
    }

    .btn-danger-outline:hover {
      background: #fef2f2;
      border-color: #ef4444;
    }

    /* ── Filters ── */
    .filters-bar {
      display: flex;
      gap: 12px;
      flex-wrap: wrap;
      align-items: center;
    }

    .filter-input,
    .filter-select {
      padding: 8px 12px;
      border: 1px solid #e2e8f0;
      border-radius: 8px;
      font-size: 13px;
      background: white;
      outline: none;
      color: #1e293b;
    }

    .filter-input:focus,
    .filter-select:focus {
      border-color: #6366f1;
    }

    .filter-search {
      width: 260px;
    }

    /* ── Table ── */
    .table-wrapper {
      overflow-x: auto;
    }

    table {
      width: 100%;
      border-collapse: collapse;
      font-size: 13px;
    }

    th {
      background: #f8fafc;
      padding: 10px 20px;
      text-align: left;
      font-weight: 600;
      font-size: 11px;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      color: #64748b;
      border-bottom: 1px solid #e2e8f0;
    }

    td {
      padding: 10px 20px;
      border-bottom: 1px solid #f1f5f9;
      vertical-align: middle;
    }

    tr:last-child td {
      border-bottom: none;
    }

    tr:hover td {
      background: #f8fafc;
    }

    .permission-code {
      font-family: 'JetBrains Mono', 'Fira Code', 'Courier New', monospace;
      font-size: 12.5px;
      font-weight: 500;
      color: #6366f1;
      background: #eef2ff;
      padding: 3px 8px;
      border-radius: 4px;
      white-space: nowrap;
    }

    .cell-description {
      color: #64748b;
      font-size: 13px;
      max-width: 300px;
    }

    .cell-actions-header {
      text-align: right;
    }

    .cell-actions {
      white-space: nowrap;
      text-align: right;
    }

    .cell-actions .btn {
      margin-left: 4px;
    }

    /* ── Badges ── */
    .badge {
      display: inline-block;
      padding: 3px 10px;
      border-radius: 20px;
      font-size: 11px;
      font-weight: 600;
      white-space: nowrap;
    }

    .badge-gray { background: #f1f5f9; color: #475569; }

    /* ── States ── */
    .state-message {
      text-align: center;
      padding: 4rem 2rem;
      color: #64748b;
      font-size: 15px;
      background: white;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
    }

    /* ── Modal ── */
    .modal-overlay {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.4);
      z-index: 100;
    }

    .modal {
      position: fixed;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      background: white;
      border-radius: 12px;
      box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15);
      z-index: 101;
      width: 480px;
      max-width: 90vw;
      max-height: 85vh;
      overflow-y: auto;
    }

    .modal-sm {
      width: 400px;
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 20px 24px 16px;
      border-bottom: 1px solid #e2e8f0;
    }

    .modal-header h2 {
      font-size: 17px;
      font-weight: 700;
      color: #1e293b;
      margin: 0;
    }

    .modal-close {
      background: none;
      border: none;
      font-size: 22px;
      color: #94a3b8;
      cursor: pointer;
      padding: 4px;
      line-height: 1;
    }

    .modal-close:hover {
      color: #1e293b;
    }

    .modal-body {
      padding: 20px 24px;
    }

    .modal-footer {
      display: flex;
      justify-content: flex-end;
      gap: 8px;
      padding: 16px 24px 20px;
      border-top: 1px solid #e2e8f0;
    }

    /* ── Form ── */
    .form-group {
      margin-bottom: 16px;
    }

    .form-label {
      display: block;
      font-size: 13px;
      font-weight: 600;
      color: #374151;
      margin-bottom: 6px;
    }

    .form-input {
      width: 100%;
      padding: 9px 12px;
      border: 1px solid #d1d5db;
      border-radius: 8px;
      font-size: 14px;
      outline: none;
      color: #1e293b;
      transition: border-color 0.15s;
      font-family: inherit;
    }

    .form-input:focus {
      border-color: #6366f1;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
    }

    .form-input.error {
      border-color: #ef4444;
    }

    .form-error {
      display: block;
      color: #ef4444;
      font-size: 12px;
      margin-top: 4px;
    }

    textarea.form-input {
      resize: vertical;
      min-height: 60px;
    }

    .alert-danger {
      background: #fef2f2;
      color: #991b1b;
      padding: 10px 14px;
      border-radius: 8px;
      font-size: 13px;
      border: 1px solid #fecaca;
      margin-top: 8px;
    }

    /* ── Responsive ── */
    @media (max-width: 768px) {
      .page-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 12px;
      }

      .filters-bar {
        flex-direction: column;
      }

      .filter-search {
        width: 100%;
      }
    }
  `]
})
export class PermissionListPageComponent implements OnInit {
  private readonly permissionService = inject(PermissionService);

  readonly permissions = signal<Permission[]>([]);
  readonly categories = signal<string[]>([]);
  readonly loading = signal(false);
  readonly searchText = signal('');
  readonly selectedCategory = signal('');

  // Modal state
  readonly showModal = signal(false);
  readonly isEditing = signal(false);
  readonly saving = signal(false);
  readonly formData = signal<CreatePermissionRequest>({ name: '', description: null });
  readonly formErrors = signal<any>({});
  readonly editingPermissionId = signal<string | null>(null);

  // Delete state
  readonly showDeleteConfirm = signal(false);
  readonly deleting = signal(false);
  readonly permissionToDelete = signal<Permission | null>(null);

  private readonly categoryIcons: Record<string, string> = {
    admin: '🛡️',
    inventory: '📦',
    consignment: '🤝',
    pos: '💰',
    dashboard: '📊',
    reports: '📈',
    ecommerce: '🛒',
  };

  readonly permissionsByCategory = computed(() => {
    const perms = this.permissions();
    const grouped: Array<{ name: string; permissions: Permission[] }> = [];
    const categoryMap = new Map<string, Permission[]>();

    perms.forEach(p => {
      if (!categoryMap.has(p.category)) {
        categoryMap.set(p.category, []);
      }
      categoryMap.get(p.category)!.push(p);
    });

    categoryMap.forEach((permissions, category) => {
      grouped.push({ name: category, permissions });
    });

    return grouped.sort((a, b) => a.name.localeCompare(b.name));
  });

  ngOnInit() {
    this.loadCategories();
    this.loadPermissions();
  }

  getCategoryIcon(category: string): string {
    return this.categoryIcons[category] || '🔑';
  }

  loadCategories() {
    this.permissionService.getCategories().subscribe({
      next: (categories) => {
        this.categories.set(categories);
      }
    });
  }

  loadPermissions() {
    this.loading.set(true);
    this.permissionService.getAll(
      this.selectedCategory() || undefined,
      this.searchText() || undefined
    ).subscribe({
      next: (permissions) => {
        this.permissions.set(permissions);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  onSearchChange() {
    this.loadPermissions();
  }

  onCategoryChange() {
    this.loadPermissions();
  }

  clearFilters() {
    this.searchText.set('');
    this.selectedCategory.set('');
    this.loadPermissions();
  }

  openCreate() {
    this.isEditing.set(false);
    this.formData.set({ name: '', description: null });
    this.formErrors.set({});
    this.editingPermissionId.set(null);
    this.showModal.set(true);
  }

  openEdit(permission: Permission) {
    this.isEditing.set(true);
    this.formData.set({ name: permission.name, description: permission.description });
    this.formErrors.set({});
    this.editingPermissionId.set(permission.externalId);
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
  }

  setFormName(value: string) {
    this.formData.update(d => ({ ...d, name: value }));
  }

  setFormDescription(value: string | null) {
    this.formData.update(d => ({ ...d, description: value }));
  }

  savePermission() {
    const errors: any = {};
    const name = this.formData().name.trim();
    if (!name) {
      errors.name = 'O nome é obrigatório';
    } else if (name.split('.').length < 2) {
      errors.name = 'Formato: categoria.recurso.ação (ex: admin.users.view)';
    }

    if (Object.keys(errors).length > 0) {
      this.formErrors.set(errors);
      return;
    }

    this.saving.set(true);
    const data = this.formData();

    if (this.isEditing()) {
      this.permissionService.update(this.editingPermissionId()!, data).subscribe({
        next: () => {
          this.saving.set(false);
          this.closeModal();
          this.loadPermissions();
          this.loadCategories();
        },
        error: (error: { error?: { message?: string } }) => {
          this.saving.set(false);
          this.formErrors.set({ general: error.error?.message || 'Erro ao guardar permissão' });
        }
      });
    } else {
      this.permissionService.create(data).subscribe({
        next: () => {
          this.saving.set(false);
          this.closeModal();
          this.loadPermissions();
          this.loadCategories();
        },
        error: (error: { error?: { message?: string } }) => {
          this.saving.set(false);
          this.formErrors.set({ general: error.error?.message || 'Erro ao guardar permissão' });
        }
      });
    }
  }

  confirmDelete(permission: Permission) {
    this.permissionToDelete.set(permission);
    this.showDeleteConfirm.set(true);
  }

  cancelDelete() {
    this.showDeleteConfirm.set(false);
    this.permissionToDelete.set(null);
  }

  deletePermission() {
    const permission = this.permissionToDelete();
    if (!permission) return;

    this.deleting.set(true);
    this.permissionService.delete(permission.externalId).subscribe({
      next: () => {
        this.deleting.set(false);
        this.cancelDelete();
        this.loadPermissions();
        this.loadCategories();
      },
      error: (error: { error?: { message?: string } }) => {
        this.deleting.set(false);
        alert(error.error?.message || 'Erro ao eliminar permissão');
      }
    });
  }
}
