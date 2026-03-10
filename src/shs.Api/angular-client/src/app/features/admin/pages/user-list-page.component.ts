import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../services/user.service';
import { User } from '../../../core/models/user.model';

@Component({
  selector: 'oui-user-list-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Utilizadores</h1>
        <p class="page-subtitle">{{ users().length }} utilizadores no sistema</p>
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

    /* ── Filters ── */
    .filters-bar {
      display: flex;
      gap: 12px;
      flex-wrap: wrap;
      align-items: center;
    }

    .filter-input {
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
      width: 300px;
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

    /* ── Badges ── */
    .badge {
      display: inline-block;
      padding: 3px 10px;
      border-radius: 20px;
      font-size: 11px;
      font-weight: 600;
      white-space: nowrap;
    }

    .badge + .badge {
      margin-left: 4px;
    }

    .badge-blue { background: #dbeafe; color: #1e40af; }

    .text-muted {
      color: #9ca3af;
      font-size: 13px;
    }

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
export class UserListPageComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly router = inject(Router);

  readonly users = signal<User[]>([]);
  readonly loading = signal(false);
  readonly searchText = signal('');

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
}
