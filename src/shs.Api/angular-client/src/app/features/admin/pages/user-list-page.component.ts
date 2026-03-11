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

    .filter-search { width: 300px; }
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
