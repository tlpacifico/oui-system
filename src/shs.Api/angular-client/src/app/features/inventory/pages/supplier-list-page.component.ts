import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { SupplierService } from '../services/supplier.service';
import { SupplierListItem, CreateSupplierRequest, UpdateSupplierRequest } from '../../../core/models/supplier.model';

@Component({
  selector: 'oui-supplier-list-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Fornecedores</h1>
        <p class="page-subtitle">{{ suppliers().length }} fornecedores registados</p>
      </div>
      <div class="page-header-actions">
        <button class="btn btn-primary" (click)="openCreate()">+ Novo Fornecedor</button>
      </div>
    </div>

    <!-- Search -->
    <div class="card filters-card">
      <div class="filters-bar">
        <input
          type="text"
          placeholder="Pesquisar fornecedores..."
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
    } @else if (suppliers().length === 0) {
      <div class="state-message">
        @if (searchText) {
          Nenhum fornecedor encontrado para "{{ searchText }}".
        } @else {
          Nenhum fornecedor registado. Clique em "+ Novo Fornecedor" para começar.
        }
      </div>
    } @else {
      <div class="card table-card">
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Inicial</th>
                <th>Nome</th>
                <th>Email</th>
                <th>Telefone</th>
                <th>NIF</th>
                <th>Itens</th>
                <th>Criado em</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              @for (supplier of suppliers(); track supplier.externalId) {
                <tr>
                  <td>
                    <span class="initial-badge">{{ supplier.initial }}</span>
                  </td>
                  <td><a class="link-name" [routerLink]="['/inventory/suppliers', supplier.externalId]"><b>{{ supplier.name }}</b></a></td>
                  <td class="cell-email">{{ supplier.email }}</td>
                  <td>{{ formatPhone(supplier.phoneNumber) }}</td>
                  <td>{{ supplier.taxNumber || '—' }}</td>
                  <td>
                    <span class="badge badge-gray">{{ supplier.itemCount }}</span>
                  </td>
                  <td>{{ supplier.createdOn | date: 'dd/MM/yyyy' }}</td>
                  <td class="cell-actions">
                    <a class="btn btn-outline btn-sm" [routerLink]="['/inventory/suppliers', supplier.externalId]">Ver</a>
                    <button class="btn btn-outline btn-sm" (click)="openEdit(supplier)">Editar</button>
                    <button
                      class="btn btn-outline btn-sm btn-danger-outline"
                      (click)="confirmDelete(supplier)"
                      [disabled]="supplier.itemCount > 0"
                      [title]="supplier.itemCount > 0 ? 'Não é possível eliminar um fornecedor com itens associados' : 'Eliminar fornecedor'"
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

    <!-- Create/Edit Modal -->
    @if (showModal()) {
      <div class="modal-overlay" (click)="closeModal()">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h2 class="modal-title">{{ isEditing() ? 'Editar Fornecedor' : 'Novo Fornecedor' }}</h2>
            <button class="modal-close" (click)="closeModal()">&times;</button>
          </div>

          <div class="modal-body">
            @if (modalError()) {
              <div class="alert alert-error">{{ modalError() }}</div>
            }

            <div class="form-row">
              <div class="form-group form-group-grow">
                <label for="supplierName">Nome *</label>
                <input
                  id="supplierName"
                  type="text"
                  [(ngModel)]="formName"
                  class="form-input"
                  placeholder="Ex: Maria Silva"
                  maxlength="256"
                  [class.input-error]="formSubmitted && !formName.trim()"
                />
                @if (formSubmitted && !formName.trim()) {
                  <span class="field-error">O nome é obrigatório.</span>
                }
              </div>

              <div class="form-group form-group-small">
                <label for="supplierInitial">Inicial *</label>
                <input
                  id="supplierInitial"
                  type="text"
                  [(ngModel)]="formInitial"
                  class="form-input"
                  placeholder="Ex: MS"
                  maxlength="5"
                  (input)="formInitial = formInitial.toUpperCase()"
                  [class.input-error]="formSubmitted && !formInitial.trim()"
                />
                @if (formSubmitted && !formInitial.trim()) {
                  <span class="field-error">A inicial é obrigatória.</span>
                }
                @if (formSubmitted && formInitial.trim() && !isValidInitial(formInitial)) {
                  <span class="field-error">Apenas letras são permitidas.</span>
                }
              </div>
            </div>

            <div class="form-group">
              <label for="supplierEmail">Email *</label>
              <input
                id="supplierEmail"
                type="email"
                [(ngModel)]="formEmail"
                class="form-input"
                placeholder="Ex: maria.silva@email.com"
                maxlength="256"
                [class.input-error]="formSubmitted && !formEmail.trim()"
              />
              @if (formSubmitted && !formEmail.trim()) {
                <span class="field-error">O email é obrigatório.</span>
              }
            </div>

            <div class="form-row">
              <div class="form-group form-group-grow">
                <label for="supplierPhone">Telefone *</label>
                <input
                  id="supplierPhone"
                  type="tel"
                  [(ngModel)]="formPhone"
                  class="form-input"
                  placeholder="+351XXXXXXXXX"
                  maxlength="13"
                  [class.input-error]="formSubmitted && !formPhone.trim()"
                />
                @if (formSubmitted && !formPhone.trim()) {
                  <span class="field-error">O telefone é obrigatório.</span>
                }
                @if (formSubmitted && formPhone.trim() && !isValidPhone(formPhone)) {
                  <span class="field-error">Formato: +351XXXXXXXXX</span>
                }
              </div>

              <div class="form-group form-group-grow">
                <label for="supplierNif">NIF</label>
                <input
                  id="supplierNif"
                  type="text"
                  [(ngModel)]="formTaxNumber"
                  class="form-input"
                  placeholder="9 dígitos (opcional)"
                  maxlength="9"
                  [class.input-error]="formSubmitted && formTaxNumber.trim() && !isValidNif(formTaxNumber)"
                />
                @if (formSubmitted && formTaxNumber.trim() && !isValidNif(formTaxNumber)) {
                  <span class="field-error">O NIF deve conter 9 dígitos válidos.</span>
                }
              </div>
            </div>

            <div class="form-row">
              <div class="form-group form-group-grow">
                <label for="supplierPorcInLoja">% Crédito em Loja (PorcInLoja)</label>
                <input
                  id="supplierPorcInLoja"
                  type="number"
                  [(ngModel)]="formCreditPercentageInStore"
                  class="form-input"
                  placeholder="50"
                  min="0"
                  max="100"
                  step="0.5"
                />
                <span class="form-hint">% do valor de venda que vira crédito para compras na loja</span>
              </div>

              <div class="form-group form-group-grow">
                <label for="supplierPorcInDinheiro">% Resgate em Dinheiro (PorcInDinheiro)</label>
                <input
                  id="supplierPorcInDinheiro"
                  type="number"
                  [(ngModel)]="formCashRedemptionPercentage"
                  class="form-input"
                  placeholder="40"
                  min="0"
                  max="100"
                  step="0.5"
                />
                <span class="form-hint">% do valor de venda resgatável em numerário</span>
              </div>
            </div>

            <div class="form-group">
              <label for="supplierNotes">Notas</label>
              <textarea
                id="supplierNotes"
                [(ngModel)]="formNotes"
                class="form-input form-textarea"
                placeholder="Notas adicionais sobre o fornecedor..."
                rows="3"
                maxlength="2000"
              ></textarea>
            </div>
          </div>

          <div class="modal-footer">
            <button class="btn btn-outline" (click)="closeModal()" [disabled]="saving()">Cancelar</button>
            <button class="btn btn-primary" (click)="saveModal()" [disabled]="saving()">
              {{ saving() ? 'A guardar...' : (isEditing() ? 'Guardar Alterações' : 'Criar Fornecedor') }}
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
            <h2 class="modal-title">Eliminar Fornecedor</h2>
            <button class="modal-close" (click)="cancelDelete()">&times;</button>
          </div>
          <div class="modal-body">
            <p>Tem certeza que deseja eliminar o fornecedor <b>{{ deletingSupplier()?.name }}</b>?</p>
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

    .filter-search { width: 300px; }

    td { border-bottom: 1px solid #e2e8f0; }
    tr:hover td { background: #f1f5f9; }

    .initial-badge {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 36px;
      height: 36px;
      border-radius: 8px;
      background: #6366f1;
      color: white;
      font-size: 13px;
      font-weight: 700;
      letter-spacing: 0.5px;
    }

    .link-name {
      text-decoration: none;
      color: #1e293b;
      transition: color 0.15s;
    }

    .link-name:hover { color: #6366f1; }

    .cell-email {
      color: #64748b;
      max-width: 220px;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    /* ── Supplier modal (flex-centered overlay) ── */
    .modal-overlay {
      display: flex;
      align-items: center;
      justify-content: center;
      background: rgba(0, 0, 0, 0.5);
      z-index: 1000;
      padding: 20px;
    }

    .modal {
      position: static;
      transform: none;
      border-radius: 16px;
      width: 100%;
      max-width: 580px;
      animation: modalIn 0.2s ease-out;
    }

    .modal-sm { max-width: 420px; }

    @keyframes modalIn {
      from { opacity: 0; transform: translateY(-10px) scale(0.98); }
      to { opacity: 1; transform: translateY(0) scale(1); }
    }

    .modal-title {
      font-size: 18px;
      font-weight: 700;
      color: #1e293b;
      margin: 0;
    }

    .modal-header { padding: 20px 24px; }
    .modal-body { padding: 24px; }
    .modal-footer { padding: 16px 24px; }

    .form-group:last-child { margin-bottom: 0; }

    .form-group label {
      display: block;
      font-size: 13px;
      font-weight: 600;
      color: #374151;
      margin-bottom: 6px;
    }

    .form-row {
      display: flex;
      gap: 12px;
    }

    .form-group-grow { flex: 1; }

    .form-group-small {
      width: 100px;
      flex-shrink: 0;
    }

    .form-input {
      padding: 10px 12px;
      border-color: #e2e8f0;
    }

    .form-textarea {
      resize: vertical;
      min-height: 80px;
    }

    .form-hint {
      display: block;
      font-size: 11px;
      color: #64748b;
      margin-top: 4px;
    }

    .alert {
      padding: 10px 14px;
      border-radius: 8px;
      font-size: 13px;
      margin-bottom: 16px;
    }

    .alert-error {
      background: #fef2f2;
      color: #991b1b;
      border: 1px solid #fecaca;
    }

    .text-muted {
      font-size: 13px;
      color: #64748b;
      margin-top: 8px;
    }

    @media (max-width: 768px) {
      .form-row {
        flex-direction: column;
        gap: 0;
      }

      .form-group-small { width: 100%; }
    }
  `]
})
export class SupplierListPageComponent implements OnInit {
  private readonly supplierService = inject(SupplierService);

  suppliers = signal<SupplierListItem[]>([]);
  loading = signal(false);
  searchText = '';

  // Modal state
  showModal = signal(false);
  isEditing = signal(false);
  editingExternalId = signal<string | null>(null);
  saving = signal(false);
  modalError = signal<string | null>(null);
  formSubmitted = false;

  // Form fields
  formName = '';
  formEmail = '';
  formPhone = '';
  formTaxNumber = '';
  formInitial = '';
  formNotes = '';
  formCreditPercentageInStore = 50;
  formCashRedemptionPercentage = 40;

  // Delete state
  showDeleteConfirm = signal(false);
  deletingSupplier = signal<SupplierListItem | null>(null);
  deleting = signal(false);

  ngOnInit(): void {
    this.loadSuppliers();
  }

  loadSuppliers(): void {
    this.loading.set(true);
    this.supplierService.getAll(this.searchText || undefined).subscribe({
      next: (suppliers) => {
        this.suppliers.set(suppliers);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  onSearchChange(): void {
    this.loadSuppliers();
  }

  clearSearch(): void {
    this.searchText = '';
    this.loadSuppliers();
  }

  formatPhone(phone: string): string {
    // +351912345678 -> +351 912 345 678
    if (phone && phone.startsWith('+351') && phone.length === 13) {
      const num = phone.slice(4);
      return `+351 ${num.slice(0, 3)} ${num.slice(3, 6)} ${num.slice(6)}`;
    }
    return phone;
  }

  // ── Validation helpers ──

  isValidInitial(value: string): boolean {
    return /^[A-Za-z]+$/.test(value.trim());
  }

  isValidPhone(value: string): boolean {
    return /^\+351\d{9}$/.test(value.trim());
  }

  isValidNif(value: string): boolean {
    const nif = value.trim();
    if (!/^\d{9}$/.test(nif)) return false;
    const first = nif[0];
    if (first === '0' || first === '4') return false;
    let sum = 0;
    for (let i = 0; i < 8; i++) {
      sum += parseInt(nif[i]) * (9 - i);
    }
    const remainder = sum % 11;
    const checkDigit = remainder < 2 ? 0 : 11 - remainder;
    return parseInt(nif[8]) === checkDigit;
  }

  // ── Create / Edit Modal ──

  openCreate(): void {
    this.formName = '';
    this.formEmail = '';
    this.formPhone = '+351';
    this.formTaxNumber = '';
    this.formInitial = '';
    this.formNotes = '';
    this.formCreditPercentageInStore = 50;
    this.formCashRedemptionPercentage = 40;
    this.formSubmitted = false;
    this.isEditing.set(false);
    this.editingExternalId.set(null);
    this.modalError.set(null);
    this.showModal.set(true);
  }

  openEdit(supplier: SupplierListItem): void {
    this.formName = supplier.name;
    this.formEmail = supplier.email;
    this.formPhone = supplier.phoneNumber;
    this.formTaxNumber = supplier.taxNumber || '';
    this.formInitial = supplier.initial;
    this.formNotes = '';
    this.formCreditPercentageInStore = supplier.creditPercentageInStore ?? 50;
    this.formCashRedemptionPercentage = supplier.cashRedemptionPercentage ?? 40;
    this.formSubmitted = false;
    this.isEditing.set(true);
    this.editingExternalId.set(supplier.externalId);
    this.modalError.set(null);
    this.showModal.set(true);

    // Load full details to get notes
    this.supplierService.getById(supplier.externalId).subscribe({
      next: (full) => {
        this.formNotes = full.notes || '';
        this.formCreditPercentageInStore = full.creditPercentageInStore ?? 50;
        this.formCashRedemptionPercentage = full.cashRedemptionPercentage ?? 40;
      }
    });
  }

  closeModal(): void {
    this.showModal.set(false);
  }

  saveModal(): void {
    this.formSubmitted = true;
    this.modalError.set(null);

    // Client-side validation
    const hasErrors =
      !this.formName.trim() ||
      !this.formEmail.trim() ||
      !this.formPhone.trim() ||
      !this.isValidPhone(this.formPhone) ||
      !this.formInitial.trim() ||
      !this.isValidInitial(this.formInitial) ||
      (this.formTaxNumber.trim() && !this.isValidNif(this.formTaxNumber));

    if (hasErrors) return;

    this.saving.set(true);

    const data = {
      name: this.formName.trim(),
      email: this.formEmail.trim(),
      phoneNumber: this.formPhone.trim(),
      taxNumber: this.formTaxNumber.trim() || undefined,
      initial: this.formInitial.trim().toUpperCase(),
      notes: this.formNotes.trim() || undefined,
      creditPercentageInStore: this.formCreditPercentageInStore,
      cashRedemptionPercentage: this.formCashRedemptionPercentage,
    };

    if (this.isEditing() && this.editingExternalId()) {
      this.supplierService.update(this.editingExternalId()!, data).subscribe({
        next: () => {
          this.saving.set(false);
          this.showModal.set(false);
          this.loadSuppliers();
        },
        error: (err) => {
          this.saving.set(false);
          this.modalError.set(
            err.error?.error || err.error?.errors
              ? this.formatApiErrors(err.error.errors)
              : 'Erro ao atualizar fornecedor.'
          );
        }
      });
    } else {
      this.supplierService.create(data).subscribe({
        next: () => {
          this.saving.set(false);
          this.showModal.set(false);
          this.loadSuppliers();
        },
        error: (err) => {
          this.saving.set(false);
          this.modalError.set(
            err.error?.error || err.error?.errors
              ? this.formatApiErrors(err.error.errors)
              : 'Erro ao criar fornecedor.'
          );
        }
      });
    }
  }

  private formatApiErrors(errors: Record<string, string> | undefined): string {
    if (!errors) return 'Erro desconhecido.';
    return Object.values(errors).join(' ');
  }

  // ── Delete ──

  confirmDelete(supplier: SupplierListItem): void {
    this.deletingSupplier.set(supplier);
    this.showDeleteConfirm.set(true);
  }

  cancelDelete(): void {
    this.showDeleteConfirm.set(false);
    this.deletingSupplier.set(null);
  }

  executeDelete(): void {
    const supplier = this.deletingSupplier();
    if (!supplier) return;

    this.deleting.set(true);
    this.supplierService.delete(supplier.externalId).subscribe({
      next: () => {
        this.deleting.set(false);
        this.showDeleteConfirm.set(false);
        this.deletingSupplier.set(null);
        this.loadSuppliers();
      },
      error: () => {
        this.deleting.set(false);
      }
    });
  }
}
