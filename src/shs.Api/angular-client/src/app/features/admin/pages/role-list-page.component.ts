import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { RoleService } from '../services/role.service';
import { Role, CreateRoleRequest, UpdateRoleRequest } from '../../../core/models/role.model';
import { HasPermissionDirective } from '../../../core/auth/directives/has-permission.directive';

@Component({
  selector: 'oui-role-list-page',
  standalone: true,
  imports: [CommonModule, FormsModule, HasPermissionDirective],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Roles</h1>
        <p class="page-subtitle">{{ roles().length }} roles no sistema</p>
      </div>
      <div class="page-header-actions">
        <button class="btn btn-primary" (click)="openCreate()" *hasPermission="'admin.roles.create'">
          + Nova Role
        </button>
      </div>
    </div>

    <!-- Search -->
    <div class="card filters-card">
      <div class="filters-bar">
        <input
          type="text"
          placeholder="Pesquisar roles..."
          [ngModel]="searchText()"
          (ngModelChange)="searchText.set($event); onSearchChange()"
          class="filter-input filter-search"
        />
        @if (searchText()) {
          <button class="btn btn-outline btn-sm" (click)="clearSearch()">Limpar</button>
        }
      </div>
    </div>

    @if (loading()) {
      <div class="state-message">A carregar...</div>
    } @else if (roles().length === 0) {
      <div class="state-message">
        @if (searchText()) {
          Nenhuma role encontrada para "{{ searchText() }}".
        } @else {
          Nenhuma role registada.
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
                <th>Sistema</th>
                <th>Utilizadores</th>
                <th>Permissões</th>
                <th>Criada em</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              @for (role of roles(); track role.externalId) {
                <tr [class.clickable-row]="true" (click)="navigateToDetail(role.externalId)">
                  <td><b>{{ role.name }}</b></td>
                  <td class="cell-description">{{ role.description || '—' }}</td>
                  <td>
                    @if (role.isSystemRole) {
                      <span class="badge badge-blue">Sistema</span>
                    }
                  </td>
                  <td>
                    <span class="badge badge-gray">{{ role.userCount }}</span>
                  </td>
                  <td>
                    <span class="badge badge-gray">{{ role.permissionCount }}</span>
                  </td>
                  <td>{{ role.createdOn | date: 'dd/MM/yyyy' }}</td>
                  <td class="cell-actions" (click)="$event.stopPropagation()">
                    <button class="btn btn-outline btn-sm" (click)="openEdit(role)" *hasPermission="'admin.roles.update'">
                      Editar
                    </button>
                    <button
                      class="btn btn-outline btn-sm btn-danger-outline"
                      (click)="confirmDelete(role)"
                      [disabled]="role.isSystemRole || role.userCount > 0"
                      [title]="role.isSystemRole ? 'Não é possível eliminar roles de sistema' : (role.userCount > 0 ? 'Não é possível eliminar uma role com utilizadores' : 'Eliminar role')"
                      *hasPermission="'admin.roles.delete'"
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
      <div class="modal-overlay" (click)="closeModal()"></div>
      <div class="modal">
        <div class="modal-header">
          <h2>{{ isEditing() ? 'Editar Role' : 'Nova Role' }}</h2>
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
              placeholder="Admin, Manager, Cashier..."
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
              placeholder="Descrição da role..."
              rows="3"
            ></textarea>
          </div>
          @if (formErrors().general) {
            <div class="alert alert-danger">{{ formErrors().general }}</div>
          }
        </div>
        <div class="modal-footer">
          <button class="btn btn-outline" (click)="closeModal()">Cancelar</button>
          <button class="btn btn-primary" (click)="saveRole()" [disabled]="saving()">
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
          <p>Tem a certeza que deseja eliminar a role <strong>{{ roleToDelete()?.name }}</strong>?</p>
        </div>
        <div class="modal-footer">
          <button class="btn btn-outline" (click)="cancelDelete()">Cancelar</button>
          <button class="btn btn-danger" (click)="deleteRole()" [disabled]="deleting()">
            {{ deleting() ? 'A eliminar...' : 'Eliminar' }}
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    .clickable-row {
      cursor: pointer;
    }
    .clickable-row:hover {
      background-color: rgba(0, 0, 0, 0.02);
    }
    .cell-actions {
      white-space: nowrap;
    }
  `]
})
export class RoleListPageComponent implements OnInit {
  private readonly roleService = inject(RoleService);
  private readonly router = inject(Router);

  readonly roles = signal<Role[]>([]);
  readonly loading = signal(false);
  readonly searchText = signal('');

  // Modal state
  readonly showModal = signal(false);
  readonly isEditing = signal(false);
  readonly saving = signal(false);
  readonly formData = signal<CreateRoleRequest>({ name: '', description: null });
  readonly formErrors = signal<any>({});
  readonly editingRoleId = signal<string | null>(null);

  // Delete state
  readonly showDeleteConfirm = signal(false);
  readonly deleting = signal(false);
  readonly roleToDelete = signal<Role | null>(null);

  ngOnInit() {
    this.loadRoles();
  }

  loadRoles() {
    this.loading.set(true);
    this.roleService.getAll(this.searchText() || undefined).subscribe({
      next: (roles) => {
        this.roles.set(roles);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  onSearchChange() {
    this.loadRoles();
  }

  clearSearch() {
    this.searchText.set('');
    this.loadRoles();
  }

  navigateToDetail(externalId: string) {
    this.router.navigate(['/admin/roles', externalId]);
  }

  openCreate() {
    this.isEditing.set(false);
    this.formData.set({ name: '', description: null });
    this.formErrors.set({});
    this.editingRoleId.set(null);
    this.showModal.set(true);
  }

  openEdit(role: Role) {
    this.isEditing.set(true);
    this.formData.set({ name: role.name, description: role.description });
    this.formErrors.set({});
    this.editingRoleId.set(role.externalId);
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

  saveRole() {
    // Validate
    const errors: any = {};
    if (!this.formData().name.trim()) {
      errors.name = 'O nome é obrigatório';
    }

    if (Object.keys(errors).length > 0) {
      this.formErrors.set(errors);
      return;
    }

    this.saving.set(true);
    const data = this.formData();

    if (this.isEditing()) {
      this.roleService.update(this.editingRoleId()!, data).subscribe({
        next: () => {
          this.saving.set(false);
          this.closeModal();
          this.loadRoles();
        },
        error: (error: { error?: { message?: string } }) => {
          this.saving.set(false);
          this.formErrors.set({ general: error.error?.message || 'Erro ao guardar role' });
        }
      });
    } else {
      this.roleService.create(data).subscribe({
        next: () => {
          this.saving.set(false);
          this.closeModal();
          this.loadRoles();
        },
        error: (error: { error?: { message?: string } }) => {
          this.saving.set(false);
          this.formErrors.set({ general: error.error?.message || 'Erro ao guardar role' });
        }
      });
    }
  }

  confirmDelete(role: Role) {
    this.roleToDelete.set(role);
    this.showDeleteConfirm.set(true);
  }

  cancelDelete() {
    this.showDeleteConfirm.set(false);
    this.roleToDelete.set(null);
  }

  deleteRole() {
    const role = this.roleToDelete();
    if (!role) return;

    this.deleting.set(true);
    this.roleService.delete(role.externalId).subscribe({
      next: () => {
        this.deleting.set(false);
        this.cancelDelete();
        this.loadRoles();
      },
      error: (error: { error?: { message?: string } }) => {
        this.deleting.set(false);
        alert(error.error?.message || 'Erro ao eliminar role');
      }
    });
  }
}
