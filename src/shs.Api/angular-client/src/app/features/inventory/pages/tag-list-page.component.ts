import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TagService } from '../services/tag.service';
import { TagListItem } from '../../../core/models/tag.model';

@Component({
  selector: 'oui-tag-list-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Tags</h1>
        <p class="page-subtitle">{{ tags().length }} tags registadas</p>
      </div>
      <div class="page-header-actions">
        <button class="btn btn-primary" (click)="openCreate()">+ Nova Tag</button>
      </div>
    </div>

    <!-- Search -->
    <div class="card filters-card">
      <div class="filters-bar">
        <input
          type="text"
          placeholder="Pesquisar tags..."
          [(ngModel)]="searchText"
          (ngModelChange)="onSearchChange()"
          class="filter-input filter-search"
        />
        @if (searchText) {
          <button class="btn btn-outline btn-sm" (click)="clearSearch()">Limpar</button>
        }
      </div>
    </div>

    @if (loading()) {
      <div class="state-message">A carregar...</div>
    } @else if (tags().length === 0) {
      <div class="state-message">
        @if (searchText) {
          Nenhuma tag encontrada para "{{ searchText }}".
        } @else {
          Nenhuma tag registada. Clique em "+ Nova Tag" para começar.
        }
      </div>
    } @else {
      <div class="tags-grid">
        @for (tag of tags(); track tag.externalId) {
          <div class="tag-card card">
            <div class="tag-card-header">
              <span
                class="tag-color-dot"
                [style.background-color]="tag.color || '#94a3b8'"
              ></span>
              <b class="tag-name">{{ tag.name }}</b>
            </div>
            <div class="tag-card-meta">
              <span class="tag-item-count">{{ tag.itemCount }} {{ tag.itemCount === 1 ? 'item' : 'itens' }}</span>
              <span class="tag-date">{{ tag.createdOn | date: 'dd/MM/yyyy' }}</span>
            </div>
            @if (tag.color) {
              <div class="tag-preview">
                <span
                  class="tag-badge"
                  [style.background-color]="tag.color + '22'"
                  [style.color]="tag.color"
                  [style.border-color]="tag.color + '44'"
                >
                  {{ tag.name }}
                </span>
              </div>
            }
            <div class="tag-card-actions">
              <button class="btn btn-outline btn-sm" (click)="openEdit(tag)">Editar</button>
              <button
                class="btn btn-outline btn-sm btn-danger-outline"
                (click)="confirmDelete(tag)"
                [disabled]="tag.itemCount > 0"
                [title]="tag.itemCount > 0 ? 'Não é possível eliminar uma tag com itens associados' : 'Eliminar tag'"
              >
                Eliminar
              </button>
            </div>
          </div>
        }
      </div>
    }

    <!-- Modal -->
    @if (showModal()) {
      <div class="modal-overlay" (click)="closeModal()">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h2 class="modal-title">{{ isEditing() ? 'Editar Tag' : 'Nova Tag' }}</h2>
            <button class="modal-close" (click)="closeModal()">&times;</button>
          </div>

          <div class="modal-body">
            @if (modalError()) {
              <div class="alert alert-error">{{ modalError() }}</div>
            }

            <div class="form-group">
              <label for="tagName">Nome *</label>
              <input
                id="tagName"
                type="text"
                [(ngModel)]="formName"
                class="form-input"
                placeholder="Ex: Novidade, Promoção, Vintage..."
                maxlength="100"
                [class.input-error]="formSubmitted && !formName.trim()"
              />
              @if (formSubmitted && !formName.trim()) {
                <span class="field-error">O nome é obrigatório.</span>
              }
            </div>

            <div class="form-group">
              <label for="tagColor">Cor</label>
              <div class="color-picker-row">
                <input
                  id="tagColor"
                  type="color"
                  [(ngModel)]="formColor"
                  class="color-picker"
                />
                <input
                  type="text"
                  [(ngModel)]="formColor"
                  class="form-input color-text"
                  placeholder="#6366f1"
                  maxlength="7"
                />
                @if (formColor) {
                  <span
                    class="tag-badge tag-preview-inline"
                    [style.background-color]="formColor + '22'"
                    [style.color]="formColor"
                    [style.border-color]="formColor + '44'"
                  >
                    {{ formName || 'Preview' }}
                  </span>
                }
              </div>
              @if (formSubmitted && formColor && !isValidColor(formColor)) {
                <span class="field-error">Formato inválido. Use #RRGGBB.</span>
              }
            </div>

            <div class="preset-colors">
              <label>Cores sugeridas</label>
              <div class="color-swatches">
                @for (c of presetColors; track c) {
                  <button
                    class="swatch"
                    [style.background-color]="c"
                    [class.active]="formColor === c"
                    (click)="formColor = c"
                    [title]="c"
                  ></button>
                }
              </div>
            </div>
          </div>

          <div class="modal-footer">
            <button class="btn btn-outline" (click)="closeModal()" [disabled]="saving()">Cancelar</button>
            <button class="btn btn-primary" (click)="saveModal()" [disabled]="saving()">
              {{ saving() ? 'A guardar...' : (isEditing() ? 'Guardar Alterações' : 'Criar Tag') }}
            </button>
          </div>
        </div>
      </div>
    }

    <!-- Delete Confirmation -->
    @if (showDeleteConfirm()) {
      <div class="modal-overlay" (click)="cancelDelete()">
        <div class="modal modal-sm" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h2 class="modal-title">Eliminar Tag</h2>
            <button class="modal-close" (click)="cancelDelete()">&times;</button>
          </div>
          <div class="modal-body">
            <p>Tem certeza que deseja eliminar a tag <b>{{ deletingTag()?.name }}</b>?</p>
            <p class="text-muted">Esta ação não pode ser revertida.</p>
          </div>
          <div class="modal-footer">
            <button class="btn btn-outline" (click)="cancelDelete()" [disabled]="deleting()">Cancelar</button>
            <button class="btn btn-danger" (click)="executeDelete()" [disabled]="deleting()">
              {{ deleting() ? 'A eliminar...' : 'Eliminar' }}
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    :host { display: block; }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }

    .page-title { font-size: 22px; font-weight: 700; margin: 0 0 4px; color: #1e293b; }
    .page-subtitle { font-size: 14px; color: #64748b; margin: 0; }
    .page-header-actions { display: flex; gap: 8px; }

    .card {
      background: #ffffff;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      padding: 20px;
    }

    .filters-card { margin-bottom: 20px; padding: 16px; }

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

    .btn:disabled { opacity: 0.5; cursor: not-allowed; }
    .btn-primary { background: #6366f1; color: white; }
    .btn-primary:hover:not(:disabled) { background: #4f46e5; }
    .btn-outline { background: white; color: #1e293b; border-color: #e2e8f0; }
    .btn-outline:hover:not(:disabled) { background: #f8fafc; }
    .btn-sm { padding: 5px 10px; font-size: 12px; }
    .btn-danger { background: #ef4444; color: white; }
    .btn-danger:hover:not(:disabled) { background: #dc2626; }
    .btn-danger-outline { color: #ef4444; border-color: #fecaca; }
    .btn-danger-outline:hover:not(:disabled) { background: #fef2f2; }

    .filters-bar { display: flex; gap: 12px; flex-wrap: wrap; align-items: center; }

    .filter-input {
      padding: 8px 12px;
      border: 1px solid #e2e8f0;
      border-radius: 8px;
      font-size: 13px;
      background: white;
      outline: none;
      color: #1e293b;
    }

    .filter-input:focus { border-color: #6366f1; }
    .filter-search { width: 300px; }

    /* Tags Grid */
    .tags-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
      gap: 16px;
    }

    .tag-card {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .tag-card-header {
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .tag-color-dot {
      width: 14px;
      height: 14px;
      border-radius: 50%;
      flex-shrink: 0;
    }

    .tag-name {
      font-size: 15px;
      color: #1e293b;
    }

    .tag-card-meta {
      display: flex;
      justify-content: space-between;
      font-size: 12px;
      color: #94a3b8;
    }

    .tag-preview {
      padding: 4px 0;
    }

    .tag-badge {
      display: inline-block;
      padding: 4px 12px;
      border-radius: 20px;
      font-size: 12px;
      font-weight: 600;
      border: 1px solid;
    }

    .tag-card-actions {
      display: flex;
      gap: 6px;
      margin-top: auto;
    }

    /* Modal */
    .modal-overlay {
      position: fixed;
      inset: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
      padding: 20px;
    }

    .modal {
      background: white;
      border-radius: 16px;
      width: 100%;
      max-width: 520px;
      box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15);
      animation: modalIn 0.2s ease-out;
    }

    .modal-sm { max-width: 420px; }

    @keyframes modalIn {
      from { opacity: 0; transform: translateY(-10px) scale(0.98); }
      to { opacity: 1; transform: translateY(0) scale(1); }
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 20px 24px;
      border-bottom: 1px solid #e2e8f0;
    }

    .modal-title { font-size: 18px; font-weight: 700; color: #1e293b; margin: 0; }

    .modal-close {
      background: none;
      border: none;
      font-size: 24px;
      color: #94a3b8;
      cursor: pointer;
      padding: 0;
      line-height: 1;
    }

    .modal-close:hover { color: #1e293b; }
    .modal-body { padding: 24px; }

    .modal-footer {
      padding: 16px 24px;
      border-top: 1px solid #e2e8f0;
      display: flex;
      justify-content: flex-end;
      gap: 8px;
    }

    .form-group { margin-bottom: 16px; }
    .form-group:last-child { margin-bottom: 0; }

    .form-group label {
      display: block;
      font-size: 13px;
      font-weight: 600;
      color: #374151;
      margin-bottom: 6px;
    }

    .form-input {
      width: 100%;
      padding: 10px 12px;
      border: 1px solid #e2e8f0;
      border-radius: 8px;
      font-size: 14px;
      outline: none;
      color: #1e293b;
      font-family: inherit;
      transition: border-color 0.15s;
    }

    .form-input:focus {
      border-color: #6366f1;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
    }

    .form-input.input-error { border-color: #ef4444; }
    .field-error { display: block; font-size: 12px; color: #ef4444; margin-top: 4px; }

    .color-picker-row {
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .color-picker {
      width: 42px;
      height: 42px;
      border: 1px solid #e2e8f0;
      border-radius: 8px;
      padding: 2px;
      cursor: pointer;
      flex-shrink: 0;
    }

    .color-text {
      width: 120px;
      flex-shrink: 0;
    }

    .tag-preview-inline {
      flex-shrink: 0;
    }

    .preset-colors { margin-top: 4px; }

    .preset-colors label {
      display: block;
      font-size: 12px;
      font-weight: 600;
      color: #64748b;
      margin-bottom: 8px;
    }

    .color-swatches {
      display: flex;
      gap: 6px;
      flex-wrap: wrap;
    }

    .swatch {
      width: 28px;
      height: 28px;
      border-radius: 50%;
      border: 2px solid transparent;
      cursor: pointer;
      transition: all 0.15s;
    }

    .swatch:hover {
      transform: scale(1.15);
    }

    .swatch.active {
      border-color: #1e293b;
      box-shadow: 0 0 0 2px white, 0 0 0 4px #1e293b;
    }

    .alert {
      padding: 10px 14px;
      border-radius: 8px;
      font-size: 13px;
      margin-bottom: 16px;
    }

    .alert-error { background: #fef2f2; color: #991b1b; border: 1px solid #fecaca; }
    .text-muted { font-size: 13px; color: #64748b; margin-top: 8px; }

    .state-message {
      text-align: center;
      padding: 4rem 2rem;
      color: #64748b;
      font-size: 15px;
      background: white;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
    }

    @media (max-width: 768px) {
      .page-header { flex-direction: column; align-items: flex-start; gap: 12px; }
      .filter-search { width: 100%; }
      .tags-grid { grid-template-columns: 1fr; }
    }
  `]
})
export class TagListPageComponent implements OnInit {
  private readonly tagService = inject(TagService);

  tags = signal<TagListItem[]>([]);
  loading = signal(false);
  searchText = '';

  // Modal
  showModal = signal(false);
  isEditing = signal(false);
  editingExternalId = signal<string | null>(null);
  saving = signal(false);
  modalError = signal<string | null>(null);
  formSubmitted = false;

  formName = '';
  formColor = '';

  // Delete
  showDeleteConfirm = signal(false);
  deletingTag = signal<TagListItem | null>(null);
  deleting = signal(false);

  readonly presetColors = [
    '#ef4444', '#f97316', '#f59e0b', '#eab308',
    '#84cc16', '#22c55e', '#14b8a6', '#06b6d4',
    '#3b82f6', '#6366f1', '#8b5cf6', '#a855f7',
    '#d946ef', '#ec4899', '#f43f5e', '#64748b',
  ];

  ngOnInit(): void {
    this.loadTags();
  }

  loadTags(): void {
    this.loading.set(true);
    this.tagService.getAll(this.searchText || undefined).subscribe({
      next: (tags) => {
        this.tags.set(tags);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  onSearchChange(): void {
    this.loadTags();
  }

  clearSearch(): void {
    this.searchText = '';
    this.loadTags();
  }

  openCreate(): void {
    this.formName = '';
    this.formColor = '';
    this.formSubmitted = false;
    this.isEditing.set(false);
    this.editingExternalId.set(null);
    this.modalError.set(null);
    this.showModal.set(true);
  }

  openEdit(tag: TagListItem): void {
    this.formName = tag.name;
    this.formColor = tag.color || '';
    this.formSubmitted = false;
    this.isEditing.set(true);
    this.editingExternalId.set(tag.externalId);
    this.modalError.set(null);
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
  }

  saveModal(): void {
    this.formSubmitted = true;
    this.modalError.set(null);

    if (!this.formName.trim()) return;
    if (this.formColor && !this.isValidColor(this.formColor)) return;

    this.saving.set(true);

    const data = {
      name: this.formName.trim(),
      color: this.formColor.trim() || undefined,
    };

    if (this.isEditing() && this.editingExternalId()) {
      this.tagService.update(this.editingExternalId()!, data).subscribe({
        next: () => {
          this.saving.set(false);
          this.showModal.set(false);
          this.loadTags();
        },
        error: (err) => {
          this.saving.set(false);
          this.modalError.set(err.error?.error || 'Erro ao atualizar tag.');
        }
      });
    } else {
      this.tagService.create(data).subscribe({
        next: () => {
          this.saving.set(false);
          this.showModal.set(false);
          this.loadTags();
        },
        error: (err) => {
          this.saving.set(false);
          this.modalError.set(err.error?.error || 'Erro ao criar tag.');
        }
      });
    }
  }

  confirmDelete(tag: TagListItem): void {
    this.deletingTag.set(tag);
    this.showDeleteConfirm.set(true);
  }

  cancelDelete(): void {
    this.showDeleteConfirm.set(false);
    this.deletingTag.set(null);
  }

  executeDelete(): void {
    const tag = this.deletingTag();
    if (!tag) return;

    this.deleting.set(true);
    this.tagService.delete(tag.externalId).subscribe({
      next: () => {
        this.deleting.set(false);
        this.showDeleteConfirm.set(false);
        this.deletingTag.set(null);
        this.loadTags();
      },
      error: () => {
        this.deleting.set(false);
      }
    });
  }

  isValidColor(color: string): boolean {
    return /^#[0-9a-fA-F]{6}$/.test(color);
  }
}
