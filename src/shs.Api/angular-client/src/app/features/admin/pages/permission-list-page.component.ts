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

    .category-card { margin-bottom: 16px; }

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

    .cell-actions-header { text-align: right; }

    .cell-actions {
      white-space: nowrap;
      text-align: right;
    }

    .cell-actions .btn { margin-left: 4px; }
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
