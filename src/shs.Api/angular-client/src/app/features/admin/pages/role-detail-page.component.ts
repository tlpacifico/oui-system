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
                        <code class="permission-code">{{ permission.name }}</code>
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
                          <code class="permission-code">{{ permission.name }}</code>
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
    :host { display: block; }

    /* ── Page header ── */
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }

    .page-title {
      font-size: 24px;
      font-family: 'DM Serif Display', Georgia, serif;
      font-weight: 400;
      margin: 0 0 4px;
      color: #1C1917;
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .page-subtitle {
      font-size: 14px;
      color: #78716C;
      margin: 0;
    }

    /* ── Breadcrumbs ── */
    .breadcrumbs {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 13px;
      color: #78716C;
      margin-bottom: 8px;
    }

    .breadcrumb-link {
      color: #5B7153;
      cursor: pointer;
      text-decoration: none;
      font-weight: 500;
    }

    .breadcrumb-link:hover {
      text-decoration: underline;
    }

    .breadcrumb-separator {
      color: #A8A29E;
    }

    /* ── Stats ── */
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 16px;
      margin-bottom: 20px;
    }

    .stat-card {
      background: #ffffff;
      border: 1px solid #E7E5E4;
      border-radius: 14px;
      padding: 20px;
    }

    .stat-value {
      font-size: 28px;
      font-weight: 700;
      color: #1C1917;
    }

    .stat-label {
      font-size: 13px;
      color: #78716C;
      margin-top: 4px;
    }

    /* ── Cards ── */
    .card {
      background: #ffffff;
      border-radius: 14px;
      border: 1px solid #E7E5E4;
    }

    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 20px;
      border-bottom: 1px solid #E7E5E4;
    }

    .card-title {
      font-size: 15px;
      font-weight: 700;
      color: #1C1917;
      margin: 0;
    }

    .card-body {
      padding: 20px;
    }

    /* ── Buttons ── */
    .btn {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 8px 16px;
      border-radius: 10px;
      font-size: 13px;
      font-weight: 600;
      cursor: pointer;
      border: 1px solid transparent;
      transition: all 0.15s;
    }

    .btn-primary {
      background: #5B7153;
      color: white;
    }

    .btn-primary:hover {
      background: #4A5E43;
    }

    .btn-primary:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .btn-outline {
      background: white;
      color: #1C1917;
      border-color: #E7E5E4;
    }

    .btn-outline:hover {
      background: #FAF9F7;
    }

    .btn-sm {
      padding: 5px 10px;
      font-size: 12px;
    }

    .btn-danger {
      background: #C45B5B;
      color: white;
      border-color: #C45B5B;
    }

    .btn-danger:hover {
      background: #A84848;
    }

    .btn-danger-outline {
      color: #C45B5B;
      border-color: rgba(196, 91, 91, 0.3);
    }

    .btn-danger-outline:hover {
      background: rgba(196, 91, 91, 0.06);
      border-color: #C45B5B;
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

    .badge-blue { background: #E8EFE6; color: #5B7153; }
    .badge-gray { background: #F5F5F4; color: #57534E; }

    /* ── Permission categories ── */
    .permission-category {
      margin-bottom: 24px;
    }

    .permission-category:last-child {
      margin-bottom: 0;
    }

    .permission-category-title {
      font-size: 14px;
      font-weight: 700;
      color: #1C1917;
      margin: 0 0 12px;
      display: flex;
      align-items: center;
      gap: 8px;
      text-transform: capitalize;
    }

    .permission-category-checkbox {
      display: flex;
      align-items: center;
      gap: 8px;
      cursor: pointer;
      margin-bottom: 12px;
    }

    .permission-category-checkbox input[type="checkbox"] {
      width: 16px;
      height: 16px;
      accent-color: #5B7153;
    }

    .permission-list {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .permission-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 10px 14px;
      background: #FAF9F7;
      border-radius: 10px;
      border: 1px solid #F5F5F4;
    }

    .permission-item:hover {
      background: #F5F5F4;
    }

    .permission-checkbox {
      display: flex;
      gap: 10px;
      padding: 10px 14px;
      background: #FAF9F7;
      border-radius: 10px;
      border: 1px solid #F5F5F4;
      cursor: pointer;
      align-items: center;
    }

    .permission-checkbox:hover {
      background: #F5F5F4;
    }

    .permission-checkbox input[type="checkbox"] {
      width: 16px;
      height: 16px;
      accent-color: #5B7153;
      flex-shrink: 0;
    }

    .permission-code {
      font-family: 'JetBrains Mono', 'Fira Code', 'Courier New', monospace;
      font-size: 12.5px;
      font-weight: 500;
      color: #5B7153;
      background: #E8EFE6;
      padding: 3px 8px;
      border-radius: 4px;
      white-space: nowrap;
    }

    .permission-description {
      font-size: 12px;
      color: #78716C;
      margin-top: 4px;
    }

    /* ── States ── */
    .state-message {
      text-align: center;
      padding: 4rem 2rem;
      color: #78716C;
      font-size: 15px;
      background: white;
      border-radius: 14px;
      border: 1px solid #E7E5E4;
    }

    /* ── Modal ── */
    .modal-overlay {
      position: fixed;
      inset: 0;
      background: rgba(28, 25, 23, 0.45);
      z-index: 100;
    }

    .modal {
      position: fixed;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      background: white;
      border-radius: 14px;
      box-shadow: 0 20px 60px rgba(28, 25, 23, 0.18);
      z-index: 101;
      width: 480px;
      max-width: 90vw;
      max-height: 85vh;
      overflow-y: auto;
    }

    .modal-lg {
      width: 700px;
      max-width: 90vw;
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 20px 24px 16px;
      border-bottom: 1px solid #E7E5E4;
    }

    .modal-header h2 {
      font-size: 17px;
      font-weight: 700;
      color: #1C1917;
      margin: 0;
    }

    .modal-close {
      background: none;
      border: none;
      font-size: 22px;
      color: #A8A29E;
      cursor: pointer;
      padding: 4px;
      line-height: 1;
    }

    .modal-close:hover {
      color: #1C1917;
    }

    .modal-body {
      padding: 20px 24px;
    }

    .modal-footer {
      display: flex;
      justify-content: flex-end;
      gap: 8px;
      padding: 16px 24px 20px;
      border-top: 1px solid #E7E5E4;
    }

    /* ── Responsive ── */
    @media (max-width: 768px) {
      .page-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 12px;
      }

      .stats-grid {
        grid-template-columns: 1fr;
      }

      .permission-item {
        flex-direction: column;
        align-items: flex-start;
        gap: 8px;
      }
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
