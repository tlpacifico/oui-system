import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../services/user.service';
import { UserRoleService } from '../services/user-role.service';
import { RoleService } from '../services/role.service';
import { UserDetail } from '../../../core/models/user.model';
import { Role } from '../../../core/models/role.model';
import { HasPermissionDirective } from '../../../core/auth/directives/has-permission.directive';

@Component({
  selector: 'oui-user-detail-page',
  standalone: true,
  imports: [CommonModule, FormsModule, HasPermissionDirective],
  template: `
    @if (loading()) {
      <div class="state-message">A carregar...</div>
    } @else if (!user()) {
      <div class="state-message">Utilizador não encontrado.</div>
    } @else {
      <div class="page-header">
        <div>
          <button class="btn btn-outline btn-sm" (click)="goBack()">← Voltar</button>
          <h1 class="page-title" style="margin-top: 0.5rem">{{ user()!.displayName || user()!.email }}</h1>
          <p class="page-subtitle">{{ user()!.email }}</p>
        </div>
      </div>

      <!-- User Info -->
      <div class="card">
        <div class="card-header">
          <h2 class="card-title">Informações</h2>
        </div>
        <div class="card-body">
          <div class="info-grid">
            <div class="info-item">
              <span class="info-label">Email</span>
              <span class="info-value">{{ user()!.email }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Nome</span>
              <span class="info-value">{{ user()!.displayName || '—' }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Criado em</span>
              <span class="info-value">{{ user()!.createdOn | date: 'dd/MM/yyyy HH:mm' }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Roles -->
      <div class="card">
        <div class="card-header">
          <h2 class="card-title">
            Roles
            <span class="badge badge-gray">{{ user()!.roles.length }}</span>
          </h2>
          <button class="btn btn-primary btn-sm" (click)="openAssignModal()" *hasPermission="'admin.users.manage-roles'">
            + Atribuir Role
          </button>
        </div>
        <div class="card-body">
          @if (user()!.roles.length === 0) {
            <p class="text-muted">Nenhuma role atribuída.</p>
          } @else {
            <div class="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Role</th>
                    <th>Atribuída em</th>
                    <th>Atribuída por</th>
                    <th>Ações</th>
                  </tr>
                </thead>
                <tbody>
                  @for (role of user()!.roles; track role.externalId) {
                    <tr>
                      <td><b>{{ role.name }}</b></td>
                      <td>{{ role.assignedOn | date: 'dd/MM/yyyy HH:mm' }}</td>
                      <td>{{ role.assignedBy }}</td>
                      <td>
                        <button
                          class="btn btn-outline btn-sm btn-danger-outline"
                          (click)="confirmRevoke(role)"
                          *hasPermission="'admin.users.manage-roles'"
                        >
                          Remover
                        </button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }
        </div>
      </div>
    }

    <!-- Assign Role Modal -->
    @if (showAssignModal()) {
      <div class="modal-overlay" (click)="closeAssignModal()"></div>
      <div class="modal">
        <div class="modal-header">
          <h2>Atribuir Roles</h2>
          <button class="modal-close" (click)="closeAssignModal()">&times;</button>
        </div>
        <div class="modal-body">
          @if (loadingRoles()) {
            <div class="state-message">A carregar roles...</div>
          } @else {
            <div class="role-list">
              @for (role of availableRoles(); track role.externalId) {
                <label class="role-checkbox">
                  <input
                    type="checkbox"
                    [checked]="selectedRoleIds().includes(role.externalId)"
                    (change)="toggleRole(role.externalId)"
                  />
                  <div>
                    <div class="role-checkbox-name">{{ role.name }}</div>
                    @if (role.description) {
                      <div class="role-checkbox-desc">{{ role.description }}</div>
                    }
                  </div>
                </label>
              }
              @if (availableRoles().length === 0) {
                <p class="text-muted">Todas as roles já estão atribuídas.</p>
              }
            </div>
          }
          @if (assignError()) {
            <div class="alert alert-danger" style="margin-top: 1rem">{{ assignError() }}</div>
          }
        </div>
        <div class="modal-footer">
          <button class="btn btn-outline" (click)="closeAssignModal()">Cancelar</button>
          <button
            class="btn btn-primary"
            (click)="assignRoles()"
            [disabled]="assigning() || selectedRoleIds().length === 0"
          >
            {{ assigning() ? 'A atribuir...' : 'Atribuir' }}
          </button>
        </div>
      </div>
    }

    <!-- Revoke Confirmation -->
    @if (showRevokeConfirm()) {
      <div class="modal-overlay" (click)="cancelRevoke()"></div>
      <div class="modal modal-sm">
        <div class="modal-header">
          <h2>Confirmar Remoção</h2>
          <button class="modal-close" (click)="cancelRevoke()">&times;</button>
        </div>
        <div class="modal-body">
          <p>Tem a certeza que deseja remover a role <strong>{{ roleToRevoke()?.name }}</strong> deste utilizador?</p>
        </div>
        <div class="modal-footer">
          <button class="btn btn-outline" (click)="cancelRevoke()">Cancelar</button>
          <button class="btn btn-danger" (click)="revokeRole()" [disabled]="revoking()">
            {{ revoking() ? 'A remover...' : 'Remover' }}
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    .info-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
      gap: 1.5rem;
    }
    .info-item {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }
    .info-label {
      font-size: 0.75rem;
      color: #6b7280;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .info-value {
      font-size: 0.9375rem;
      color: #111827;
    }
    .text-muted {
      color: #9ca3af;
      font-size: 0.875rem;
    }
    .role-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }
    .role-checkbox {
      display: flex;
      align-items: flex-start;
      gap: 0.75rem;
      padding: 0.75rem;
      border: 1px solid #e5e7eb;
      border-radius: 6px;
      cursor: pointer;
    }
    .role-checkbox:hover {
      background: #f9fafb;
    }
    .role-checkbox input {
      margin-top: 0.125rem;
    }
    .role-checkbox-name {
      font-weight: 500;
      color: #111827;
    }
    .role-checkbox-desc {
      font-size: 0.75rem;
      color: #6b7280;
      margin-top: 0.25rem;
    }
  `]
})
export class UserDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly userService = inject(UserService);
  private readonly userRoleService = inject(UserRoleService);
  private readonly roleService = inject(RoleService);

  readonly user = signal<UserDetail | null>(null);
  readonly loading = signal(false);

  // Assign modal state
  readonly showAssignModal = signal(false);
  readonly loadingRoles = signal(false);
  readonly allRoles = signal<Role[]>([]);
  readonly availableRoles = signal<Role[]>([]);
  readonly selectedRoleIds = signal<string[]>([]);
  readonly assigning = signal(false);
  readonly assignError = signal('');

  // Revoke state
  readonly showRevokeConfirm = signal(false);
  readonly revoking = signal(false);
  readonly roleToRevoke = signal<{ externalId: string; name: string } | null>(null);

  ngOnInit() {
    this.loadUser();
  }

  loadUser() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.loading.set(true);
    this.userService.getById(id).subscribe({
      next: (user) => {
        this.user.set(user);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  goBack() {
    this.router.navigate(['/admin/users']);
  }

  openAssignModal() {
    this.showAssignModal.set(true);
    this.selectedRoleIds.set([]);
    this.assignError.set('');
    this.loadingRoles.set(true);

    this.roleService.getAll().subscribe({
      next: (roles) => {
        this.allRoles.set(roles);
        const userRoleIds = new Set(this.user()?.roles.map(r => r.externalId) || []);
        this.availableRoles.set(roles.filter(r => !userRoleIds.has(r.externalId)));
        this.loadingRoles.set(false);
      },
      error: () => {
        this.loadingRoles.set(false);
      }
    });
  }

  closeAssignModal() {
    this.showAssignModal.set(false);
  }

  toggleRole(roleId: string) {
    this.selectedRoleIds.update(ids => {
      if (ids.includes(roleId)) {
        return ids.filter(id => id !== roleId);
      }
      return [...ids, roleId];
    });
  }

  assignRoles() {
    const user = this.user();
    if (!user || this.selectedRoleIds().length === 0) return;

    this.assigning.set(true);
    this.assignError.set('');

    this.userRoleService.assignBulkRoles(user.externalId, {
      roleExternalIds: this.selectedRoleIds()
    }).subscribe({
      next: () => {
        this.assigning.set(false);
        this.closeAssignModal();
        this.loadUser();
      },
      error: (error: { error?: { message?: string } }) => {
        this.assigning.set(false);
        this.assignError.set(error.error?.message || 'Erro ao atribuir roles');
      }
    });
  }

  confirmRevoke(role: { externalId: string; name: string }) {
    this.roleToRevoke.set(role);
    this.showRevokeConfirm.set(true);
  }

  cancelRevoke() {
    this.showRevokeConfirm.set(false);
    this.roleToRevoke.set(null);
  }

  revokeRole() {
    const user = this.user();
    const role = this.roleToRevoke();
    if (!user || !role) return;

    this.revoking.set(true);
    this.userRoleService.revokeRole(user.externalId, role.externalId).subscribe({
      next: () => {
        this.revoking.set(false);
        this.cancelRevoke();
        this.loadUser();
      },
      error: (error: { error?: { message?: string } }) => {
        this.revoking.set(false);
        alert(error.error?.message || 'Erro ao remover role');
      }
    });
  }
}
