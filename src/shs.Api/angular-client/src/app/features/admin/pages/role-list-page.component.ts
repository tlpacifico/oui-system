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

    .table-card {
      padding: 0;
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

    .btn-danger-outline:disabled {
      opacity: 0.4;
      cursor: not-allowed;
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

    .filter-input:focus {
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
      border-bottom: 1px solid #f1f5f9;
      vertical-align: middle;
    }

    tr:last-child td {
      border-bottom: none;
    }

    .clickable-row {
      cursor: pointer;
    }

    .clickable-row:hover td {
      background: #f8fafc;
    }

    .cell-description {
      color: #64748b;
      max-width: 280px;
    }

    .cell-actions {
      white-space: nowrap;
    }

    .cell-actions .btn {
      margin-right: 4px;
    }

    .cell-actions .btn:last-child {
      margin-right: 0;
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

    .badge-blue { background: #dbeafe; color: #1e40af; }
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
