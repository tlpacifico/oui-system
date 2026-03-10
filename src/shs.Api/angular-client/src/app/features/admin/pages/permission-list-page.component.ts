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
        <div class="card">
          <div class="card-header">
            <h2 class="card-title">
              {{ categoryGroup.name }}
              <span class="badge badge-gray">{{ categoryGroup.permissions.length }}</span>
            </h2>
          </div>
          <div class="card-body">
            <div class="permission-grid">
              @for (permission of categoryGroup.permissions; track permission.externalId) {
                <div class="permission-card">
                  <div class="permission-card-header">
                    <div class="permission-name">{{ permission.name }}</div>
                    <div class="permission-actions" (click)="$event.stopPropagation()">
                      <button class="btn-icon" (click)="openEdit(permission)" title="Editar" *hasPermission="'admin.permissions.update'">✏️</button>
                      <button class="btn-icon" (click)="confirmDelete(permission)" title="Eliminar" *hasPermission="'admin.permissions.delete'">🗑️</button>
                    </div>
                  </div>
                  @if (permission.description) {
                    <div class="permission-description">{{ permission.description }}</div>
                  }
                </div>
              }
            </div>
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
    .permission-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 1rem;
    }
    .permission-card {
      padding: 1rem;
      background: #f9fafb;
      border-radius: 6px;
      border: 1px solid #e5e7eb;
    }
    .permission-card-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
    }
    .permission-name {
      font-size: 0.875rem;
      font-weight: 500;
      color: #111827;
      font-family: 'Courier New', monospace;
    }
    .permission-description {
      font-size: 0.75rem;
      color: #6b7280;
      margin-top: 0.5rem;
    }
    .permission-actions {
      display: flex;
      gap: 0.25rem;
    }
    .btn-icon {
      background: none;
      border: none;
      cursor: pointer;
      padding: 0.25rem;
      font-size: 0.75rem;
      opacity: 0.6;
      transition: opacity 0.2s;
    }
    .btn-icon:hover {
      opacity: 1;
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
