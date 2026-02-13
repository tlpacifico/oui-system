import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RoleService } from '../services/role.service';
import { PermissionService } from '../services/permission.service';
import { RolePermissionService } from '../services/role-permission.service';
import { RoleDetail, Permission } from '../../../core/models/role.model';
import { PermissionsByCategory } from '../../../core/models/permission.model';
import { HasPermissionDirective } from '../../../core/auth/directives/has-permission.directive';

@Component({
  selector: 'oui-role-detail-page',
  standalone: true,
  imports: [CommonModule, FormsModule, HasPermissionDirective],
  template: `
    @if (loading()) {
      <div class="state-message">A carregar...</div>
    } @else if (role()) {
      <div class="page-header">
        <div>
          <div class="breadcrumbs">
            <a (click)="goBack()" class="breadcrumb-link">Roles</a>
            <span class="breadcrumb-separator">›</span>
            <span>{{ role()!.name }}</span>
          </div>
          <h1 class="page-title">
            {{ role()!.name }}
            @if (role()!.isSystemRole) {
              <span class="badge badge-blue">Sistema</span>
            }
          </h1>
          @if (role()!.description) {
            <p class="page-subtitle">{{ role()!.description }}</p>
          }
        </div>
      </div>

      <!-- Stats -->
      <div class="stats-grid">
        <div class="stat-card">
          <div class="stat-value">{{ role()!.userCount }}</div>
          <div class="stat-label">Utilizadores</div>
        </div>
        <div class="stat-card">
          <div class="stat-value">{{ role()!.permissionCount }}</div>
          <div class="stat-label">Permissões</div>
        </div>
      </div>

      <!-- Permissions -->
      <div class="card">
        <div class="card-header">
          <h2 class="card-title">Permissões</h2>
          <button
            class="btn btn-primary btn-sm"
            (click)="openPermissionModal()"
            *hasPermission="'admin.roles.manage-permissions'"
          >
            + Adicionar Permissões
          </button>
        </div>
        <div class="card-body">
          @if (permissionsByCategory().length === 0) {
            <div class="state-message">Esta role ainda não tem permissões atribuídas.</div>
          } @else {
            @for (category of permissionsByCategory(); track category.name) {
              <div class="permission-category">
                <h3 class="permission-category-title">
                  {{ category.name }}
                  <span class="badge badge-gray">{{ category.permissions.length }}</span>
                </h3>
                <div class="permission-list">
                  @for (permission of category.permissions; track permission.externalId) {
                    <div class="permission-item">
                      <div>
                        <div class="permission-name">{{ permission.name }}</div>
                        @if (permission.description) {
                          <div class="permission-description">{{ permission.description }}</div>
                        }
                      </div>
                      <button
                        class="btn btn-outline btn-sm btn-danger-outline"
                        (click)="revokePermission(permission)"
                        *hasPermission="'admin.roles.manage-permissions'"
                      >
                        Revocar
                      </button>
                    </div>
                  }
                </div>
              </div>
            }
          }
        </div>
      </div>

      <!-- Permission Assignment Modal -->
      @if (showPermissionModal()) {
        <div class="modal-overlay" (click)="closePermissionModal()"></div>
        <div class="modal modal-lg">
          <div class="modal-header">
            <h2>Adicionar Permissões</h2>
            <button class="modal-close" (click)="closePermissionModal()">&times;</button>
          </div>
          <div class="modal-body">
            @if (loadingPermissions()) {
              <div class="state-message">A carregar permissões...</div>
            } @else {
              @for (category of availablePermissionsByCategory(); track category.name) {
                <div class="permission-category">
                  <label class="permission-category-checkbox">
                    <input
                      type="checkbox"
                      [checked]="isCategorySelected(category.name)"
                      (change)="toggleCategory(category.name)"
                    />
                    <h3 class="permission-category-title">{{ category.name }}</h3>
                  </label>
                  <div class="permission-list">
                    @for (permission of category.permissions; track permission.externalId) {
                      <label class="permission-checkbox">
                        <input
                          type="checkbox"
                          [checked]="selectedPermissions().has(permission.externalId)"
                          (change)="togglePermission(permission.externalId)"
                        />
                        <div>
                          <div class="permission-name">{{ permission.name }}</div>
                          @if (permission.description) {
                            <div class="permission-description">{{ permission.description }}</div>
                          }
                        </div>
                      </label>
                    }
                  </div>
                </div>
              }
            }
          </div>
          <div class="modal-footer">
            <button class="btn btn-outline" (click)="closePermissionModal()">Cancelar</button>
            <button
              class="btn btn-primary"
              (click)="assignSelectedPermissions()"
              [disabled]="assigningPermissions() || selectedPermissions().size === 0"
            >
              {{ assigningPermissions() ? 'A atribuir...' : \`Atribuir (\${selectedPermissions().size})\` }}
            </button>
          </div>
        </div>
      }
    } @else {
      <div class="state-message">Role não encontrada</div>
    }
  `,
  styles: [`
    .breadcrumbs {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.875rem;
      color: #6b7280;
      margin-bottom: 0.5rem;
    }
    .breadcrumb-link {
      color: #3b82f6;
      cursor: pointer;
      text-decoration: none;
    }
    .breadcrumb-link:hover {
      text-decoration: underline;
    }
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1rem;
      margin-bottom: 1.5rem;
    }
    .stat-card {
      background: white;
      border: 1px solid #e5e7eb;
      border-radius: 8px;
      padding: 1.5rem;
    }
    .stat-value {
      font-size: 2rem;
      font-weight: 700;
      color: #111827;
    }
    .stat-label {
      font-size: 0.875rem;
      color: #6b7280;
      margin-top: 0.25rem;
    }
    .permission-category {
      margin-bottom: 2rem;
    }
    .permission-category:last-child {
      margin-bottom: 0;
    }
    .permission-category-title {
      font-size: 1rem;
      font-weight: 600;
      color: #111827;
      margin-bottom: 0.75rem;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }
    .permission-category-checkbox {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      cursor: pointer;
      margin-bottom: 0.75rem;
    }
    .permission-list {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
    .permission-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.75rem;
      background: #f9fafb;
      border-radius: 6px;
    }
    .permission-checkbox {
      display: flex;
      gap: 0.75rem;
      padding: 0.75rem;
      background: #f9fafb;
      border-radius: 6px;
      cursor: pointer;
    }
    .permission-checkbox:hover {
      background: #f3f4f6;
    }
    .permission-name {
      font-size: 0.875rem;
      font-weight: 500;
      color: #111827;
    }
    .permission-description {
      font-size: 0.75rem;
      color: #6b7280;
      margin-top: 0.25rem;
    }
    .modal-lg {
      max-width: 800px;
      max-height: 80vh;
      overflow-y: auto;
    }
  `]
})
export class RoleDetailPageComponent implements OnInit {
  private readonly roleService = inject(RoleService);
  private readonly permissionService = inject(PermissionService);
  private readonly rolePermissionService = inject(RolePermissionService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly role = signal<RoleDetail | null>(null);
  readonly loading = signal(false);
  readonly permissionsByCategory = computed(() => {
    const permissions = this.role()?.permissions || [];
    const grouped: Array<{ name: string; permissions: Permission[] }> = [];
    const categoryMap = new Map<string, Permission[]>();

    permissions.forEach(p => {
      if (!categoryMap.has(p.category)) {
        categoryMap.set(p.category, []);
      }
      categoryMap.get(p.category)!.push(p);
    });

    categoryMap.forEach((perms, category) => {
      grouped.push({ name: category, permissions: perms });
    });

    return grouped.sort((a, b) => a.name.localeCompare(b.name));
  });

  // Permission modal state
  readonly showPermissionModal = signal(false);
  readonly loadingPermissions = signal(false);
  readonly allPermissions = signal<Permission[]>([]);
  readonly selectedPermissions = signal(new Set<string>());
  readonly assigningPermissions = signal(false);

  readonly availablePermissionsByCategory = computed(() => {
    const currentPermissionIds = new Set((this.role()?.permissions || []).map(p => p.externalId));
    const available = this.allPermissions().filter(p => !currentPermissionIds.has(p.externalId));

    const grouped: Array<{ name: string; permissions: Permission[] }> = [];
    const categoryMap = new Map<string, Permission[]>();

    available.forEach(p => {
      if (!categoryMap.has(p.category)) {
        categoryMap.set(p.category, []);
      }
      categoryMap.get(p.category)!.push(p);
    });

    categoryMap.forEach((perms, category) => {
      grouped.push({ name: category, permissions: perms });
    });

    return grouped.sort((a, b) => a.name.localeCompare(b.name));
  });

  ngOnInit() {
    const externalId = this.route.snapshot.paramMap.get('id');
    if (externalId) {
      this.loadRole(externalId);
    }
  }

  loadRole(externalId: string) {
    this.loading.set(true);
    this.roleService.getById(externalId).subscribe({
      next: (role) => {
        this.role.set(role);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.router.navigate(['/admin/roles']);
      }
    });
  }

  goBack() {
    this.router.navigate(['/admin/roles']);
  }

  openPermissionModal() {
    this.showPermissionModal.set(true);
    this.selectedPermissions.set(new Set());
    this.loadAllPermissions();
  }

  closePermissionModal() {
    this.showPermissionModal.set(false);
  }

  loadAllPermissions() {
    this.loadingPermissions.set(true);
    this.permissionService.getAll().subscribe({
      next: (permissions) => {
        this.allPermissions.set(permissions);
        this.loadingPermissions.set(false);
      },
      error: () => {
        this.loadingPermissions.set(false);
      }
    });
  }

  togglePermission(permissionId: string) {
    const selected = new Set(this.selectedPermissions());
    if (selected.has(permissionId)) {
      selected.delete(permissionId);
    } else {
      selected.add(permissionId);
    }
    this.selectedPermissions.set(selected);
  }

  toggleCategory(categoryName: string) {
    const category = this.availablePermissionsByCategory().find(c => c.name === categoryName);
    if (!category) return;

    const selected = new Set(this.selectedPermissions());
    const allSelected = category.permissions.every(p => selected.has(p.externalId));

    if (allSelected) {
      category.permissions.forEach(p => selected.delete(p.externalId));
    } else {
      category.permissions.forEach(p => selected.add(p.externalId));
    }

    this.selectedPermissions.set(selected);
  }

  isCategorySelected(categoryName: string): boolean {
    const category = this.availablePermissionsByCategory().find(c => c.name === categoryName);
    if (!category || category.permissions.length === 0) return false;

    const selected = this.selectedPermissions();
    return category.permissions.every(p => selected.has(p.externalId));
  }

  assignSelectedPermissions() {
    const roleId = this.role()?.externalId;
    if (!roleId) return;

    const permissionIds = Array.from(this.selectedPermissions());
    if (permissionIds.length === 0) return;

    this.assigningPermissions.set(true);
    this.rolePermissionService.assignBulkPermissions(roleId, permissionIds).subscribe({
      next: () => {
        this.assigningPermissions.set(false);
        this.closePermissionModal();
        this.loadRole(roleId);
      },
      error: (error) => {
        this.assigningPermissions.set(false);
        alert(error.error?.message || 'Erro ao atribuir permissões');
      }
    });
  }

  revokePermission(permission: Permission) {
    const roleId = this.role()?.externalId;
    if (!roleId) return;

    if (!confirm(`Revogar permissão "${permission.name}"?`)) return;

    this.rolePermissionService.revokePermission(roleId, permission.externalId).subscribe({
      next: () => {
        this.loadRole(roleId);
      },
      error: (error) => {
        alert(error.error?.message || 'Erro ao revogar permissão');
      }
    });
  }
}
