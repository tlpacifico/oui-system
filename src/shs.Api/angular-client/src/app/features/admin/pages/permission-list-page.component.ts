import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PermissionService } from '../services/permission.service';
import { Permission } from '../../../core/models/permission.model';

@Component({
  selector: 'oui-permission-list-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Permissões</h1>
        <p class="page-subtitle">{{ permissions().length }} permissões no sistema</p>
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
                  <div class="permission-name">{{ permission.name }}</div>
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
  `]
})
export class PermissionListPageComponent implements OnInit {
  private readonly permissionService = inject(PermissionService);

  readonly permissions = signal<Permission[]>([]);
  readonly categories = signal<string[]>([]);
  readonly loading = signal(false);
  readonly searchText = signal('');
  readonly selectedCategory = signal('');

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
}
