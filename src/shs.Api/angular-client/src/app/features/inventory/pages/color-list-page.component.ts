import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ColorService } from '../services/color.service';
import { ColorListItem } from '../../../core/models/color.model';

@Component({
  selector: 'oui-color-list-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Cores</h1>
        <p class="page-subtitle">{{ colors().length }} cores registadas</p>
      </div>
      <div class="page-header-actions">
        <button class="btn btn-primary" (click)="openCreate()">+ Nova Cor</button>
      </div>
    </div>

    <!-- Search -->
    <div class="card filters-card">
      <div class="filters-bar">
        <input
          type="text"
          placeholder="Pesquisar cores..."
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
    } @else if (colors().length === 0) {
      <div class="state-message">
        @if (searchText) {
          Nenhuma cor encontrada para "{{ searchText }}".
        } @else {
          Nenhuma cor registada. Clique em "+ Nova Cor" para começar.
        }
      </div>
    } @else {
      <div class="colors-grid">
        @for (color of colors(); track color.externalId) {
          <div class="color-card card">
            <div class="color-card-header">
              <span
                class="color-swatch-dot"
                [style.background-color]="color.hexCode || '#94a3b8'"
              ></span>
              <b class="color-name">{{ color.name }}</b>
            </div>
            <div class="color-card-meta">
              <span class="color-item-count">{{ color.itemCount }} {{ color.itemCount === 1 ? 'item' : 'itens' }}</span>
              <span class="color-hex">{{ color.hexCode || '—' }}</span>
            </div>
            <div class="color-card-actions">
              <button class="btn btn-outline btn-sm" (click)="openEdit(color)">Editar</button>
              <button
                class="btn btn-outline btn-sm btn-danger-outline"
                (click)="confirmDelete(color)"
                [disabled]="color.itemCount > 0"
                [title]="color.itemCount > 0 ? 'Não é possível eliminar uma cor com itens associados' : 'Eliminar cor'"
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
            <h2 class="modal-title">{{ isEditing() ? 'Editar Cor' : 'Nova Cor' }}</h2>
            <button class="modal-close" (click)="closeModal()">&times;</button>
          </div>

          <div class="modal-body">
            @if (modalError()) {
              <div class="alert alert-error">{{ modalError() }}</div>
            }

            <div class="form-group">
              <label for="colorName">Nome *</label>
              <input
                id="colorName"
                type="text"
                [(ngModel)]="formName"
                class="form-input"
                placeholder="Ex: Azul Marinho, Bege, Verde Militar..."
                maxlength="100"
                [class.input-error]="formSubmitted && !formName.trim()"
              />
              @if (formSubmitted && !formName.trim()) {
                <span class="field-error">O nome é obrigatório.</span>
              }
            </div>

            <div class="form-group">
              <label for="colorHex">Cor</label>
              <div class="color-picker-row">
                <input
                  id="colorHex"
                  type="color"
                  [(ngModel)]="formHex"
                  class="color-picker"
                />
                <input
                  type="text"
                  [(ngModel)]="formHex"
                  class="form-input color-text"
                  placeholder="#1C1917"
                  maxlength="7"
                />
                @if (formHex) {
                  <span
                    class="color-badge color-preview-inline"
                    [style.background-color]="formHex"
                  ></span>
                }
              </div>
              @if (formSubmitted && formHex && !isValidColor(formHex)) {
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
                    [class.active]="formHex === c"
                    (click)="formHex = c"
                    [title]="c"
                  ></button>
                }
              </div>
            </div>
          </div>

          <div class="modal-footer">
            <button class="btn btn-outline" (click)="closeModal()" [disabled]="saving()">Cancelar</button>
            <button class="btn btn-primary" (click)="saveModal()" [disabled]="saving()">
              {{ saving() ? 'A guardar...' : (isEditing() ? 'Guardar Alterações' : 'Criar Cor') }}
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
            <h2 class="modal-title">Eliminar Cor</h2>
            <button class="modal-close" (click)="cancelDelete()">&times;</button>
          </div>
          <div class="modal-body">
            <p>Tem certeza que deseja eliminar a cor <b>{{ deletingColor()?.name }}</b>?</p>
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

    .page-title { font-size: 24px; font-family: 'DM Serif Display', Georgia, serif; font-weight: 400; margin: 0 0 4px; color: #1C1917; }
    .page-subtitle { font-size: 14px; color: #78716C; margin: 0; }
    .page-header-actions { display: flex; gap: 8px; }

    .card {
      background: #ffffff;
      border-radius: 14px;
      border: 1px solid #E7E5E4;
      padding: 20px;
    }

    .filters-card { margin-bottom: 20px; padding: 16px; }

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

    .btn:disabled { opacity: 0.5; cursor: not-allowed; }
    .btn-primary { background: #5B7153; color: white; }
    .btn-primary:hover:not(:disabled) { background: #4A5E43; }
    .btn-outline { background: white; color: #1C1917; border-color: #E7E5E4; }
    .btn-outline:hover:not(:disabled) { background: #FAF9F7; }
    .btn-sm { padding: 5px 10px; font-size: 12px; }
    .btn-danger { background: #C45B5B; color: white; }
    .btn-danger:hover:not(:disabled) { background: #A84848; }
    .btn-danger-outline { color: #C45B5B; border-color: rgba(196, 91, 91, 0.3); }
    .btn-danger-outline:hover:not(:disabled) { background: rgba(196, 91, 91, 0.06); }

    .filters-bar { display: flex; gap: 12px; flex-wrap: wrap; align-items: center; }

    .filter-input {
      padding: 8px 12px;
      border: 1px solid #E7E5E4;
      border-radius: 10px;
      font-size: 13px;
      background: white;
      outline: none;
      color: #1C1917;
    }

    .filter-input:focus { border-color: #5B7153; }
    .filter-search { width: 300px; }

    /* Colors Grid */
    .colors-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
      gap: 16px;
    }

    .color-card {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .color-card-header {
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .color-swatch-dot {
      width: 20px;
      height: 20px;
      border-radius: 50%;
      flex-shrink: 0;
      border: 1px solid rgba(28, 25, 23, 0.12);
    }

    .color-name {
      font-size: 15px;
      color: #1C1917;
    }

    .color-card-meta {
      display: flex;
      justify-content: space-between;
      font-size: 12px;
      color: #A8A29E;
    }

    .color-hex {
      font-family: 'SFMono-Regular', Consolas, monospace;
      text-transform: uppercase;
    }

    .color-card-actions {
      display: flex;
      gap: 6px;
      margin-top: auto;
    }

    /* Modal */
    .modal-overlay {
      position: fixed;
      inset: 0;
      background: rgba(28, 25, 23, 0.45);
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
      box-shadow: 0 20px 60px rgba(28, 25, 23, 0.18);
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
      border-bottom: 1px solid #E7E5E4;
    }

    .modal-title { font-size: 18px; font-weight: 700; color: #1C1917; margin: 0; }

    .modal-close {
      background: none;
      border: none;
      font-size: 24px;
      color: #A8A29E;
      cursor: pointer;
      padding: 0;
      line-height: 1;
    }

    .modal-close:hover { color: #1C1917; }
    .modal-body { padding: 24px; }

    .modal-footer {
      padding: 16px 24px;
      border-top: 1px solid #E7E5E4;
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
      color: #44403C;
      margin-bottom: 6px;
    }

    .form-input {
      width: 100%;
      padding: 10px 12px;
      border: 1px solid #E7E5E4;
      border-radius: 10px;
      font-size: 14px;
      outline: none;
      color: #1C1917;
      font-family: inherit;
      transition: border-color 0.15s;
    }

    .form-input:focus {
      border-color: #5B7153;
      box-shadow: 0 0 0 3px rgba(91, 113, 83, 0.12);
    }

    .form-input.input-error { border-color: #C45B5B; }
    .field-error { display: block; font-size: 12px; color: #C45B5B; margin-top: 4px; }

    .color-picker-row {
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .color-picker {
      width: 42px;
      height: 42px;
      border: 1px solid #E7E5E4;
      border-radius: 10px;
      padding: 2px;
      cursor: pointer;
      flex-shrink: 0;
    }

    .color-text {
      width: 120px;
      flex-shrink: 0;
    }

    .color-badge {
      display: inline-block;
      width: 42px;
      height: 42px;
      border-radius: 10px;
      border: 1px solid rgba(28, 25, 23, 0.12);
    }

    .color-preview-inline { flex-shrink: 0; }

    .preset-colors { margin-top: 4px; }

    .preset-colors label {
      display: block;
      font-size: 12px;
      font-weight: 600;
      color: #78716C;
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

    .swatch:hover { transform: scale(1.15); }

    .swatch.active {
      border-color: #1C1917;
      box-shadow: 0 0 0 2px white, 0 0 0 4px #1C1917;
    }

    .alert {
      padding: 10px 14px;
      border-radius: 10px;
      font-size: 13px;
      margin-bottom: 16px;
    }

    .alert-error { background: rgba(196, 91, 91, 0.06); color: #8B3A3A; border: 1px solid rgba(196, 91, 91, 0.3); }
    .text-muted { font-size: 13px; color: #78716C; margin-top: 8px; }

    .state-message {
      text-align: center;
      padding: 4rem 2rem;
      color: #78716C;
      font-size: 15px;
      background: white;
      border-radius: 14px;
      border: 1px solid #E7E5E4;
    }

    @media (max-width: 768px) {
      .page-header { flex-direction: column; align-items: flex-start; gap: 12px; }
      .filter-search { width: 100%; }
      .colors-grid { grid-template-columns: 1fr; }
    }
  `]
})
export class ColorListPageComponent implements OnInit {
  private readonly colorService = inject(ColorService);

  colors = signal<ColorListItem[]>([]);
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
  formHex = '';

  // Delete
  showDeleteConfirm = signal(false);
  deletingColor = signal<ColorListItem | null>(null);
  deleting = signal(false);

  readonly presetColors = [
    '#1C1917', '#FFFFFF', '#9CA3AF', '#D9C3A5',
    '#6F4E37', '#2563EB', '#1E3A5F', '#DC2626',
    '#EC4899', '#16A34A', '#4B5320', '#EAB308',
    '#F97316', '#7C3AED', '#7B1E2B', '#C9A227',
  ];

  ngOnInit(): void {
    this.loadColors();
  }

  loadColors(): void {
    this.loading.set(true);
    this.colorService.getAll(this.searchText || undefined).subscribe({
      next: (colors) => {
        this.colors.set(colors);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  onSearchChange(): void {
    this.loadColors();
  }

  clearSearch(): void {
    this.searchText = '';
    this.loadColors();
  }

  openCreate(): void {
    this.formName = '';
    this.formHex = '';
    this.formSubmitted = false;
    this.isEditing.set(false);
    this.editingExternalId.set(null);
    this.modalError.set(null);
    this.showModal.set(true);
  }

  openEdit(color: ColorListItem): void {
    this.formName = color.name;
    this.formHex = color.hexCode || '';
    this.formSubmitted = false;
    this.isEditing.set(true);
    this.editingExternalId.set(color.externalId);
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
    if (this.formHex && !this.isValidColor(this.formHex)) return;

    this.saving.set(true);

    const data = {
      name: this.formName.trim(),
      hexCode: this.formHex.trim() || undefined,
    };

    if (this.isEditing() && this.editingExternalId()) {
      this.colorService.update(this.editingExternalId()!, data).subscribe({
        next: () => {
          this.saving.set(false);
          this.showModal.set(false);
          this.loadColors();
        },
        error: (err) => {
          this.saving.set(false);
          this.modalError.set(err.error?.error || 'Erro ao atualizar cor.');
        }
      });
    } else {
      this.colorService.create(data).subscribe({
        next: () => {
          this.saving.set(false);
          this.showModal.set(false);
          this.loadColors();
        },
        error: (err) => {
          this.saving.set(false);
          this.modalError.set(err.error?.error || 'Erro ao criar cor.');
        }
      });
    }
  }

  confirmDelete(color: ColorListItem): void {
    this.deletingColor.set(color);
    this.showDeleteConfirm.set(true);
  }

  cancelDelete(): void {
    this.showDeleteConfirm.set(false);
    this.deletingColor.set(null);
  }

  executeDelete(): void {
    const color = this.deletingColor();
    if (!color) return;

    this.deleting.set(true);
    this.colorService.delete(color.externalId).subscribe({
      next: () => {
        this.deleting.set(false);
        this.showDeleteConfirm.set(false);
        this.deletingColor.set(null);
        this.loadColors();
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
