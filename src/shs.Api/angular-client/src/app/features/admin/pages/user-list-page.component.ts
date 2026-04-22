import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../services/user.service';
import { User, CreateUserRequest } from '../../../core/models/user.model';
import { HasPermissionDirective } from '../../../core/auth/directives/has-permission.directive';

@Component({
  selector: 'oui-user-list-page',
  standalone: true,
  imports: [CommonModule, FormsModule, HasPermissionDirective],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Utilizadores</h1>
        <p class="page-subtitle">{{ users().length }} utilizadores no sistema</p>
      </div>
      <div class="page-header-actions">
        <button class="btn btn-primary" (click)="openCreate()" *hasPermission="'admin.users.create'">
          + Novo Utilizador
        </button>
      </div>
    </div>

    <!-- Search -->
    <div class="card filters-card">
      <div class="filters-bar">
        <input
          type="text"
          placeholder="Pesquisar por email ou nome..."
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
    } @else if (users().length === 0) {
      <div class="state-message">
        @if (searchText()) {
          Nenhum utilizador encontrado para "{{ searchText() }}".
        } @else {
          Nenhum utilizador registado.
        }
      </div>
    } @else {
      <div class="card table-card">
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Email</th>
                <th>Nome</th>
                <th>Roles</th>
                <th>Criado em</th>
              </tr>
            </thead>
            <tbody>
              @for (user of users(); track user.externalId) {
                <tr class="clickable-row" (click)="navigateToDetail(user.externalId)">
                  <td><b>{{ user.email }}</b></td>
                  <td>{{ user.displayName || '—' }}</td>
                  <td>
                    @for (role of user.roles; track role.externalId) {
                      <span class="badge badge-blue">{{ role.name }}</span>
                    }
                    @if (user.roles.length === 0) {
                      <span class="text-muted">Sem roles</span>
                    }
                  </td>
                  <td>{{ user.createdOn | date: 'dd/MM/yyyy' }}</td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    }

    <!-- Create Modal -->
    @if (showModal()) {
      <div class="modal-overlay" (click)="closeModal()"></div>
      <div class="modal">
        <div class="modal-header">
          <h2>Novo Utilizador</h2>
          <button class="modal-close" (click)="closeModal()">&times;</button>
        </div>
        <div class="modal-body">
          <div class="form-group">
            <label class="form-label">Email *</label>
            <input
              type="email"
              class="form-input"
              [ngModel]="formData().email"
              (ngModelChange)="setFormField('email', $event)"
              placeholder="utilizador@exemplo.com"
              [class.error]="formErrors().email"
            />
            @if (formErrors().email) {
              <span class="form-error">{{ formErrors().email }}</span>
            }
          </div>
          <div class="form-group">
            <label class="form-label">Password *</label>
            <input
              type="password"
              class="form-input"
              [ngModel]="formData().password"
              (ngModelChange)="setFormField('password', $event)"
              placeholder="Mínimo 6 caracteres"
              [class.error]="formErrors().password"
            />
            @if (formErrors().password) {
              <span class="form-error">{{ formErrors().password }}</span>
            }
          </div>
          <div class="form-group">
            <label class="form-label">Nome</label>
            <input
              type="text"
              class="form-input"
              [ngModel]="formData().displayName"
              (ngModelChange)="setFormField('displayName', $event)"
              placeholder="Nome do utilizador"
            />
          </div>
          @if (formErrors().general) {
            <div class="alert alert-danger">{{ formErrors().general }}</div>
          }
        </div>
        <div class="modal-footer">
          <button class="btn btn-outline" (click)="closeModal()">Cancelar</button>
          <button class="btn btn-primary" (click)="saveUser()" [disabled]="saving()">
            {{ saving() ? 'A criar...' : 'Criar' }}
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    :host { display: block; }

    .filter-search { width: 300px; }
  `]
})
export class UserListPageComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly router = inject(Router);

  readonly users = signal<User[]>([]);
  readonly loading = signal(false);
  readonly searchText = signal('');

  // Modal state
  readonly showModal = signal(false);
  readonly saving = signal(false);
  readonly formData = signal<CreateUserRequest>({ email: '', password: '', displayName: null });
  readonly formErrors = signal<any>({});

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.loading.set(true);
    this.userService.getAll(this.searchText() || undefined).subscribe({
      next: (users) => {
        this.users.set(users);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  onSearchChange() {
    this.loadUsers();
  }

  clearSearch() {
    this.searchText.set('');
    this.loadUsers();
  }

  navigateToDetail(externalId: string) {
    this.router.navigate(['/admin/users', externalId]);
  }

  openCreate() {
    this.formData.set({ email: '', password: '', displayName: null });
    this.formErrors.set({});
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
  }

  setFormField(field: string, value: string | null) {
    this.formData.update(d => ({ ...d, [field]: value || null }));
  }

  saveUser() {
    const errors: any = {};
    const data = this.formData();

    if (!data.email?.trim()) {
      errors.email = 'O email é obrigatório';
    }
    if (!data.password?.trim()) {
      errors.password = 'A password é obrigatória';
    } else if (data.password.length < 6) {
      errors.password = 'A password deve ter pelo menos 6 caracteres';
    }

    if (Object.keys(errors).length > 0) {
      this.formErrors.set(errors);
      return;
    }

    this.saving.set(true);
    this.userService.create(data).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.loadUsers();
      },
      error: (error: { error?: { message?: string } }) => {
        this.saving.set(false);
        this.formErrors.set({ general: error.error?.message || 'Erro ao criar utilizador' });
      }
    });
  }
}
