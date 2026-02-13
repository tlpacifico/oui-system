import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ReceptionService } from '../services/reception.service';
import { SupplierService } from '../services/supplier.service';
import { SupplierListItem } from '../../../core/models/supplier.model';
import { ReceptionDetail } from '../../../core/models/reception.model';

@Component({
  selector: 'oui-reception-receive-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Nova Recepção</h1>
        <p class="page-subtitle">Registar a entrega de peças por um fornecedor</p>
      </div>
      <div class="page-header-actions">
        <a class="btn btn-outline" routerLink="/consignments/receptions">Voltar</a>
      </div>
    </div>

    @if (!createdReception()) {
      <!-- Reception Form -->
      <div class="card form-card">
        @if (error()) {
          <div class="alert alert-error">{{ error() }}</div>
        }

        <!-- Step indicator -->
        <div class="step-indicator">
          <div class="step" [class.active]="true">
            <span class="step-number">1</span>
            <span class="step-label">Dados da Recepção</span>
          </div>
          <div class="step-divider"></div>
          <div class="step" [class.active]="false">
            <span class="step-number">2</span>
            <span class="step-label">Avaliação (próxima etapa)</span>
          </div>
        </div>

        <!-- Supplier selection -->
        <div class="form-section">
          <h2 class="section-title">Fornecedor</h2>

          <div class="form-group">
            <label for="supplierSearch">Pesquisar fornecedor *</label>
            <input
              id="supplierSearch"
              type="text"
              [(ngModel)]="supplierSearch"
              (ngModelChange)="onSupplierSearchChange()"
              (focus)="showSupplierDropdown.set(true)"
              class="form-input"
              placeholder="Escreva o nome do fornecedor..."
              autocomplete="off"
              [class.input-error]="formSubmitted && !selectedSupplier()"
            />
            @if (formSubmitted && !selectedSupplier()) {
              <span class="field-error">Selecione um fornecedor.</span>
            }

            @if (showSupplierDropdown() && filteredSuppliers().length > 0) {
              <div class="dropdown">
                @for (supplier of filteredSuppliers(); track supplier.externalId) {
                  <div class="dropdown-item" (click)="selectSupplier(supplier)">
                    <span class="initial-badge-sm">{{ supplier.initial }}</span>
                    <div class="dropdown-item-info">
                      <span class="dropdown-item-name">{{ supplier.name }}</span>
                      <span class="dropdown-item-detail">{{ supplier.email }} · {{ formatPhone(supplier.phoneNumber) }}</span>
                    </div>
                  </div>
                }
              </div>
            }
          </div>

          @if (selectedSupplier()) {
            <div class="selected-supplier">
              <span class="initial-badge">{{ selectedSupplier()!.initial }}</span>
              <div class="selected-supplier-info">
                <span class="selected-supplier-name">{{ selectedSupplier()!.name }}</span>
                <span class="selected-supplier-detail">
                  {{ selectedSupplier()!.email }} · {{ formatPhone(selectedSupplier()!.phoneNumber) }}
                  @if (selectedSupplier()!.taxNumber) {
                    · NIF: {{ selectedSupplier()!.taxNumber }}
                  }
                </span>
              </div>
              <button class="btn-clear" (click)="clearSupplier()" title="Remover fornecedor">&times;</button>
            </div>
          }
        </div>

        <!-- Item count -->
        <div class="form-section">
          <h2 class="section-title">Peças Recebidas</h2>
          <p class="section-subtitle">
            Indique apenas a quantidade de peças entregues. A avaliação individual será feita numa etapa posterior.
          </p>

          <div class="form-row">
            <div class="form-group" style="width: 200px;">
              <label for="itemCount">Quantidade de peças *</label>
              <div class="counter-input">
                <button class="counter-btn" (click)="decrementCount()" [disabled]="itemCount <= 1">−</button>
                <input
                  id="itemCount"
                  type="number"
                  [(ngModel)]="itemCount"
                  class="form-input counter-value"
                  min="1"
                  max="500"
                  [class.input-error]="formSubmitted && itemCount <= 0"
                />
                <button class="counter-btn" (click)="incrementCount()" [disabled]="itemCount >= 500">+</button>
              </div>
              @if (formSubmitted && itemCount <= 0) {
                <span class="field-error">A quantidade deve ser maior que zero.</span>
              }
            </div>
          </div>
        </div>

        <!-- Notes -->
        <div class="form-section">
          <h2 class="section-title">Observações</h2>

          <div class="form-group">
            <label for="notes">Notas adicionais</label>
            <textarea
              id="notes"
              [(ngModel)]="notes"
              class="form-input form-textarea"
              placeholder="Ex: Sacos grandes com roupas de inverno, algumas peças precisam de lavagem..."
              rows="3"
              maxlength="2000"
            ></textarea>
          </div>
        </div>

        <!-- Summary before submit -->
        <div class="form-section summary-section">
          <h2 class="section-title">Resumo</h2>
          <div class="summary-grid">
            <div class="summary-item">
              <span class="summary-label">Fornecedor</span>
              <span class="summary-value">{{ selectedSupplier()?.name || '—' }}</span>
            </div>
            <div class="summary-item">
              <span class="summary-label">Peças</span>
              <span class="summary-value summary-highlight">{{ itemCount }}</span>
            </div>
          </div>
        </div>

        <!-- Actions -->
        <div class="form-actions">
          <a class="btn btn-outline" routerLink="/consignments/receptions">Cancelar</a>
          <button class="btn btn-primary btn-lg" (click)="submit()" [disabled]="saving()">
            {{ saving() ? 'A registar...' : 'Registar Recepção' }}
          </button>
        </div>
      </div>
    } @else {
      <!-- Success state -->
      <div class="card success-card">
        <div class="success-icon">✓</div>
        <h2 class="success-title">Recepção Registada com Sucesso!</h2>
        <p class="success-subtitle">
          Foram registadas <b>{{ createdReception()!.itemCount }} peças</b> do fornecedor
          <b>{{ createdReception()!.supplier.name }}</b>.
        </p>

        <div class="success-details">
          <div class="detail-row">
            <span class="detail-label">Referência:</span>
            <span class="detail-value">{{ createdReception()!.externalId.substring(0, 8).toUpperCase() }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Data:</span>
            <span class="detail-value">{{ createdReception()!.receptionDate | date: 'dd/MM/yyyy HH:mm' }}</span>
          </div>
          <div class="detail-row">
            <span class="detail-label">Estado:</span>
            <span class="badge badge-yellow">Pendente Avaliação</span>
          </div>
        </div>

        <div class="success-actions">
          <button class="btn btn-outline" (click)="openReceipt()">
            Imprimir Recibo
          </button>
          <button class="btn btn-primary" (click)="startNew()">
            Nova Recepção
          </button>
          <a class="btn btn-outline" routerLink="/consignments/receptions">
            Ver Todas as Recepções
          </a>
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

    .page-header-actions {
      display: flex;
      gap: 8px;
    }

    .card {
      background: #ffffff;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      padding: 24px;
    }

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
      text-decoration: none;
    }

    .btn:disabled { opacity: 0.5; cursor: not-allowed; }

    .btn-primary { background: #6366f1; color: white; }
    .btn-primary:hover:not(:disabled) { background: #4f46e5; }

    .btn-outline { background: white; color: #1e293b; border-color: #e2e8f0; }
    .btn-outline:hover:not(:disabled) { background: #f8fafc; }

    .btn-lg { padding: 10px 24px; font-size: 14px; }

    /* ── Step indicator ── */
    .step-indicator {
      display: flex;
      align-items: center;
      gap: 16px;
      margin-bottom: 32px;
      padding: 16px;
      background: #f8fafc;
      border-radius: 10px;
    }

    .step {
      display: flex;
      align-items: center;
      gap: 8px;
      opacity: 0.4;
    }

    .step.active { opacity: 1; }

    .step-number {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 28px;
      height: 28px;
      border-radius: 50%;
      background: #e2e8f0;
      color: #475569;
      font-size: 13px;
      font-weight: 700;
    }

    .step.active .step-number {
      background: #6366f1;
      color: white;
    }

    .step-label {
      font-size: 13px;
      font-weight: 600;
      color: #475569;
    }

    .step.active .step-label { color: #1e293b; }

    .step-divider {
      flex: 1;
      height: 2px;
      background: #e2e8f0;
    }

    /* ── Form sections ── */
    .form-section {
      margin-bottom: 28px;
      padding-bottom: 24px;
      border-bottom: 1px solid #f1f5f9;
    }

    .form-section:last-of-type {
      border-bottom: none;
    }

    .section-title {
      font-size: 16px;
      font-weight: 700;
      color: #1e293b;
      margin: 0 0 4px;
    }

    .section-subtitle {
      font-size: 13px;
      color: #64748b;
      margin: 0 0 16px;
    }

    .form-group {
      margin-bottom: 16px;
      position: relative;
    }

    .form-group:last-child { margin-bottom: 0; }

    .form-group label {
      display: block;
      font-size: 13px;
      font-weight: 600;
      color: #374151;
      margin-bottom: 6px;
    }

    .form-row { display: flex; gap: 12px; }

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

    .form-textarea { resize: vertical; min-height: 80px; }

    .field-error {
      display: block;
      font-size: 12px;
      color: #ef4444;
      margin-top: 4px;
    }

    /* ── Dropdown ── */
    .dropdown {
      position: absolute;
      top: 100%;
      left: 0;
      right: 0;
      background: white;
      border: 1px solid #e2e8f0;
      border-radius: 10px;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
      z-index: 50;
      max-height: 260px;
      overflow-y: auto;
      margin-top: 4px;
    }

    .dropdown-item {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 10px 14px;
      cursor: pointer;
      transition: background 0.1s;
    }

    .dropdown-item:hover { background: #f1f5f9; }

    .dropdown-item:first-child { border-radius: 10px 10px 0 0; }
    .dropdown-item:last-child { border-radius: 0 0 10px 10px; }

    .initial-badge-sm {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 28px;
      height: 28px;
      border-radius: 6px;
      background: #6366f1;
      color: white;
      font-size: 11px;
      font-weight: 700;
      flex-shrink: 0;
    }

    .dropdown-item-info { display: flex; flex-direction: column; }

    .dropdown-item-name {
      font-size: 13px;
      font-weight: 600;
      color: #1e293b;
    }

    .dropdown-item-detail {
      font-size: 12px;
      color: #64748b;
    }

    /* ── Selected supplier card ── */
    .selected-supplier {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 14px;
      background: #f0f0ff;
      border: 1px solid #c7d2fe;
      border-radius: 10px;
      margin-top: 12px;
    }

    .initial-badge {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 40px;
      height: 40px;
      border-radius: 8px;
      background: #6366f1;
      color: white;
      font-size: 14px;
      font-weight: 700;
      flex-shrink: 0;
    }

    .selected-supplier-info { flex: 1; display: flex; flex-direction: column; }

    .selected-supplier-name {
      font-size: 14px;
      font-weight: 700;
      color: #1e293b;
    }

    .selected-supplier-detail {
      font-size: 12px;
      color: #64748b;
    }

    .btn-clear {
      background: none;
      border: none;
      font-size: 22px;
      color: #94a3b8;
      cursor: pointer;
      padding: 0 4px;
      line-height: 1;
    }

    .btn-clear:hover { color: #ef4444; }

    /* ── Counter input ── */
    .counter-input {
      display: flex;
      align-items: center;
      gap: 0;
      border: 1px solid #e2e8f0;
      border-radius: 8px;
      overflow: hidden;
      width: fit-content;
    }

    .counter-btn {
      width: 40px;
      height: 40px;
      background: #f8fafc;
      border: none;
      font-size: 18px;
      font-weight: 600;
      color: #475569;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: background 0.1s;
    }

    .counter-btn:hover:not(:disabled) { background: #e2e8f0; }
    .counter-btn:disabled { opacity: 0.3; cursor: not-allowed; }

    .counter-value {
      width: 80px;
      text-align: center;
      border: none;
      border-left: 1px solid #e2e8f0;
      border-right: 1px solid #e2e8f0;
      border-radius: 0;
      font-size: 18px;
      font-weight: 700;
      padding: 8px;
      -moz-appearance: textfield;
    }

    .counter-value::-webkit-outer-spin-button,
    .counter-value::-webkit-inner-spin-button {
      -webkit-appearance: none;
      margin: 0;
    }

    /* ── Summary ── */
    .summary-section {
      background: #f8fafc;
      border-radius: 10px;
      padding: 20px;
      margin-bottom: 20px;
      border-bottom: none;
    }

    .summary-grid {
      display: flex;
      gap: 32px;
    }

    .summary-item {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .summary-label {
      font-size: 12px;
      font-weight: 600;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .summary-value {
      font-size: 16px;
      font-weight: 600;
      color: #1e293b;
    }

    .summary-highlight {
      font-size: 24px;
      font-weight: 700;
      color: #6366f1;
    }

    /* ── Form actions ── */
    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: 10px;
      padding-top: 16px;
      border-top: 1px solid #e2e8f0;
    }

    /* ── Alert ── */
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

    /* ── Success ── */
    .success-card {
      text-align: center;
      padding: 48px 24px;
    }

    .success-icon {
      width: 64px;
      height: 64px;
      border-radius: 50%;
      background: #dcfce7;
      color: #16a34a;
      font-size: 32px;
      font-weight: 700;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      margin-bottom: 20px;
    }

    .success-title {
      font-size: 22px;
      font-weight: 700;
      color: #1e293b;
      margin: 0 0 8px;
    }

    .success-subtitle {
      font-size: 15px;
      color: #64748b;
      margin: 0 0 28px;
    }

    .success-details {
      display: inline-flex;
      flex-direction: column;
      gap: 8px;
      text-align: left;
      background: #f8fafc;
      border: 1px solid #e2e8f0;
      border-radius: 10px;
      padding: 20px;
      margin-bottom: 28px;
    }

    .detail-row {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .detail-label {
      font-size: 13px;
      font-weight: 600;
      color: #64748b;
      min-width: 100px;
    }

    .detail-value {
      font-size: 14px;
      font-weight: 600;
      color: #1e293b;
    }

    .success-actions {
      display: flex;
      justify-content: center;
      gap: 10px;
      flex-wrap: wrap;
    }

    .badge {
      display: inline-block;
      padding: 3px 10px;
      border-radius: 20px;
      font-size: 11px;
      font-weight: 600;
    }

    .badge-yellow { background: #fef9c3; color: #854d0e; }

    @media (max-width: 768px) {
      .page-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 12px;
      }

      .step-indicator { flex-direction: column; gap: 8px; }
      .step-divider { display: none; }

      .form-row { flex-direction: column; }

      .summary-grid { flex-direction: column; gap: 16px; }

      .form-actions { flex-direction: column; }

      .success-actions { flex-direction: column; align-items: center; }
    }
  `]
})
export class ReceptionReceivePageComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly receptionService = inject(ReceptionService);
  private readonly supplierService = inject(SupplierService);

  // Suppliers
  allSuppliers = signal<SupplierListItem[]>([]);
  filteredSuppliers = signal<SupplierListItem[]>([]);
  selectedSupplier = signal<SupplierListItem | null>(null);
  showSupplierDropdown = signal(false);
  supplierSearch = '';

  // Form fields
  itemCount = 1;
  notes = '';
  formSubmitted = false;

  // State
  saving = signal(false);
  error = signal<string | null>(null);
  createdReception = signal<ReceptionDetail | null>(null);

  ngOnInit(): void {
    this.loadSuppliers();

    // Close dropdown on outside click
    document.addEventListener('click', (e: Event) => {
      const target = e.target as HTMLElement;
      if (!target.closest('.form-group')) {
        this.showSupplierDropdown.set(false);
      }
    });
  }

  loadSuppliers(): void {
    this.supplierService.getAll().subscribe({
      next: (suppliers) => {
        this.allSuppliers.set(suppliers);
      }
    });
  }

  onSupplierSearchChange(): void {
    if (!this.supplierSearch.trim()) {
      this.filteredSuppliers.set([]);
      return;
    }

    const s = this.supplierSearch.toLowerCase();
    const filtered = this.allSuppliers().filter(sup =>
      sup.name.toLowerCase().includes(s) ||
      sup.initial.toLowerCase().includes(s) ||
      sup.email.toLowerCase().includes(s)
    );
    this.filteredSuppliers.set(filtered);
    this.showSupplierDropdown.set(true);
  }

  selectSupplier(supplier: SupplierListItem): void {
    this.selectedSupplier.set(supplier);
    this.supplierSearch = supplier.name;
    this.showSupplierDropdown.set(false);
    this.filteredSuppliers.set([]);
  }

  clearSupplier(): void {
    this.selectedSupplier.set(null);
    this.supplierSearch = '';
    this.filteredSuppliers.set([]);
  }

  incrementCount(): void {
    if (this.itemCount < 500) this.itemCount++;
  }

  decrementCount(): void {
    if (this.itemCount > 1) this.itemCount--;
  }

  formatPhone(phone: string): string {
    if (phone && phone.startsWith('+351') && phone.length === 13) {
      const num = phone.slice(4);
      return `+351 ${num.slice(0, 3)} ${num.slice(3, 6)} ${num.slice(6)}`;
    }
    return phone;
  }

  submit(): void {
    this.formSubmitted = true;
    this.error.set(null);

    if (!this.selectedSupplier()) {
      this.error.set('Selecione um fornecedor.');
      return;
    }

    if (this.itemCount <= 0) {
      this.error.set('A quantidade de peças deve ser maior que zero.');
      return;
    }

    this.saving.set(true);

    this.receptionService.create({
      supplierExternalId: this.selectedSupplier()!.externalId,
      itemCount: this.itemCount,
      notes: this.notes.trim() || undefined,
    }).subscribe({
      next: (reception) => {
        this.saving.set(false);
        this.createdReception.set(reception);
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(
          err.error?.error || 'Erro ao registar recepção. Tente novamente.'
        );
      }
    });
  }

  openReceipt(): void {
    const reception = this.createdReception();
    if (reception) {
      this.receptionService.getReceiptHtml(reception.externalId).subscribe({
        next: (html) => {
          const w = window.open('', '_blank');
          if (w) {
            w.document.write(html);
            w.document.close();
          }
        },
        error: () => {
          this.error.set('Erro ao carregar o recibo.');
        }
      });
    }
  }

  startNew(): void {
    this.createdReception.set(null);
    this.selectedSupplier.set(null);
    this.supplierSearch = '';
    this.itemCount = 1;
    this.notes = '';
    this.formSubmitted = false;
    this.error.set(null);
  }
}
