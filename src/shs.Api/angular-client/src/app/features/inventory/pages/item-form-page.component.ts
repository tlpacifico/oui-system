import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ItemService } from '../services/item.service';
import { BrandService } from '../services/brand.service';
import { CategoryService } from '../services/category.service';
import { TagService } from '../services/tag.service';
import { SupplierService } from '../services/supplier.service';
import { Item, CreateItemRequest, UpdateItemRequest } from '../../../core/models/item.model';
import { BrandListItem } from '../../../core/models/brand.model';
import { SupplierListItem } from '../../../core/models/supplier.model';
import { forkJoin } from 'rxjs';

interface SelectOption {
  externalId: string;
  name: string;
  color?: string;
}

@Component({
  selector: 'oui-item-form-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="detail-topbar">
      <button class="btn btn-outline" (click)="goBack()">← Voltar</button>
      <h1 class="page-title">{{ isEditing() ? 'Editar Item' : 'Novo Item' }}</h1>
      <div></div>
    </div>

    @if (loadingData()) {
      <div class="state-message">A carregar dados...</div>
    } @else {
      <form (ngSubmit)="onSubmit()" class="form-layout">
        @if (formError()) {
          <div class="alert alert-error">{{ formError() }}</div>
        }

        <!-- Section: Basic Info -->
        <div class="card">
          <div class="card-title">Informações Básicas</div>

          <div class="form-group">
            <label for="itemName">Nome *</label>
            <input
              id="itemName"
              type="text"
              [(ngModel)]="form.name"
              name="name"
              class="form-input"
              placeholder="Ex: Vestido floral manga longa"
              maxlength="500"
              [class.input-error]="submitted && !form.name.trim()"
            />
            @if (submitted && !form.name.trim()) {
              <span class="field-error">O nome é obrigatório.</span>
            }
          </div>

          <div class="form-group">
            <label for="itemDescription">Descrição</label>
            <textarea
              id="itemDescription"
              [(ngModel)]="form.description"
              name="description"
              class="form-input form-textarea"
              placeholder="Descrição opcional do item..."
              rows="3"
              maxlength="2000"
            ></textarea>
          </div>

          <div class="form-row">
            <div class="form-group form-group-grow">
              <label for="itemBrand">Marca *</label>
              <select
                id="itemBrand"
                [(ngModel)]="form.brandExternalId"
                name="brandExternalId"
                class="form-input"
                [class.input-error]="submitted && !form.brandExternalId"
              >
                <option value="">Selecionar marca...</option>
                @for (brand of brands(); track brand.externalId) {
                  <option [value]="brand.externalId">{{ brand.name }}</option>
                }
              </select>
              @if (submitted && !form.brandExternalId) {
                <span class="field-error">A marca é obrigatória.</span>
              }
            </div>

            <div class="form-group form-group-grow">
              <label for="itemCategory">Categoria</label>
              <select
                id="itemCategory"
                [(ngModel)]="form.categoryExternalId"
                name="categoryExternalId"
                class="form-input"
              >
                <option value="">Sem categoria</option>
                @for (cat of categories(); track cat.externalId) {
                  <option [value]="cat.externalId">{{ cat.name }}</option>
                }
              </select>
            </div>
          </div>
        </div>

        <!-- Section: Details -->
        <div class="card">
          <div class="card-title">Detalhes do Item</div>

          <div class="form-row">
            <div class="form-group form-group-grow">
              <label for="itemSize">Tamanho *</label>
              <select
                id="itemSize"
                [(ngModel)]="form.size"
                name="size"
                class="form-input"
                [class.input-error]="submitted && !form.size"
              >
                <option value="">Selecionar...</option>
                <option value="XXS">XXS</option>
                <option value="XS">XS</option>
                <option value="S">S</option>
                <option value="M">M</option>
                <option value="L">L</option>
                <option value="XL">XL</option>
                <option value="XXL">XXL</option>
                <option value="XXXL">XXXL</option>
                <option value="34">34</option>
                <option value="36">36</option>
                <option value="38">38</option>
                <option value="40">40</option>
                <option value="42">42</option>
                <option value="44">44</option>
                <option value="46">46</option>
                <option value="Único">Único</option>
              </select>
              @if (submitted && !form.size) {
                <span class="field-error">O tamanho é obrigatório.</span>
              }
            </div>

            <div class="form-group form-group-grow">
              <label for="itemColor">Cor *</label>
              <input
                id="itemColor"
                type="text"
                [(ngModel)]="form.color"
                name="color"
                class="form-input"
                placeholder="Ex: Azul marinho"
                maxlength="100"
                [class.input-error]="submitted && !form.color.trim()"
              />
              @if (submitted && !form.color.trim()) {
                <span class="field-error">A cor é obrigatória.</span>
              }
            </div>

            <div class="form-group form-group-grow">
              <label for="itemCondition">Condição *</label>
              <select
                id="itemCondition"
                [(ngModel)]="form.condition"
                name="condition"
                class="form-input"
                [class.input-error]="submitted && !form.condition"
              >
                <option value="">Selecionar...</option>
                <option value="Excellent">Excelente</option>
                <option value="VeryGood">Muito Bom</option>
                <option value="Good">Bom</option>
                <option value="Fair">Razoável</option>
                <option value="Poor">Mau</option>
              </select>
              @if (submitted && !form.condition) {
                <span class="field-error">A condição é obrigatória.</span>
              }
            </div>
          </div>

          <div class="form-group">
            <label for="itemComposition">Composição</label>
            <input
              id="itemComposition"
              type="text"
              [(ngModel)]="form.composition"
              name="composition"
              class="form-input"
              placeholder="Ex: 100% algodão, 60% poliéster / 40% algodão"
              maxlength="500"
            />
          </div>
        </div>

        <!-- Section: Pricing & Origin -->
        <div class="card">
          <div class="card-title">Preço e Origem</div>

          @if (!isEditing()) {
            <div class="form-row">
              <div class="form-group form-group-grow">
                <label for="itemAcquisition">Tipo de Aquisição *</label>
                <select
                  id="itemAcquisition"
                  [(ngModel)]="form.acquisitionType"
                  name="acquisitionType"
                  (ngModelChange)="onAcquisitionTypeChange()"
                  class="form-input"
                  [class.input-error]="submitted && !form.acquisitionType"
                >
                  <option value="">Selecionar...</option>
                  <option value="Consignment">Consignação</option>
                  <option value="OwnPurchase">Compra Própria</option>
                </select>
                @if (submitted && !form.acquisitionType) {
                  <span class="field-error">O tipo é obrigatório.</span>
                }
              </div>

              @if (form.acquisitionType === 'OwnPurchase') {
                <div class="form-group form-group-grow">
                  <label for="itemOrigin">Origem *</label>
                  <select
                    id="itemOrigin"
                    [(ngModel)]="form.origin"
                    name="origin"
                    class="form-input"
                    [class.input-error]="submitted && form.acquisitionType === 'OwnPurchase' && !form.origin"
                  >
                    <option value="">Selecionar...</option>
                    <option value="Humana">Humana</option>
                    <option value="Vinted">Vinted</option>
                    <option value="HM">H&M</option>
                    <option value="PersonalCollection">Coleção Pessoal</option>
                    <option value="Other">Outra</option>
                  </select>
                  @if (submitted && form.acquisitionType === 'OwnPurchase' && !form.origin) {
                    <span class="field-error">A origem é obrigatória.</span>
                  }
                </div>
              }
            </div>
          }

          @if (form.acquisitionType === 'Consignment' && !isEditing()) {
            <div class="form-row">
              <div class="form-group form-group-grow">
                <label for="itemSupplier">Fornecedor *</label>
                <select
                  id="itemSupplier"
                  [(ngModel)]="form.supplierExternalId"
                  name="supplierExternalId"
                  class="form-input"
                  [class.input-error]="submitted && form.acquisitionType === 'Consignment' && !form.supplierExternalId"
                >
                  <option value="">Selecionar fornecedor...</option>
                  @for (sup of suppliers(); track sup.externalId) {
                    <option [value]="sup.externalId">{{ sup.initial }} - {{ sup.name }}</option>
                  }
                </select>
                @if (submitted && form.acquisitionType === 'Consignment' && !form.supplierExternalId) {
                  <span class="field-error">O fornecedor é obrigatório.</span>
                }
              </div>

              <div class="form-group form-group-small-md">
                <label for="itemCommission">Comissão %</label>
                <input
                  id="itemCommission"
                  type="number"
                  [(ngModel)]="form.commissionPercentage"
                  name="commissionPercentage"
                  class="form-input"
                  placeholder="50"
                  min="0"
                  max="100"
                  step="1"
                />
              </div>
            </div>
          }

          <div class="form-row">
            <div class="form-group form-group-grow">
              <label for="itemPrice">Preço de Venda (€) *</label>
              <input
                id="itemPrice"
                type="number"
                [(ngModel)]="form.evaluatedPrice"
                name="evaluatedPrice"
                class="form-input"
                placeholder="0.00"
                min="0.01"
                step="0.01"
                [class.input-error]="submitted && (!form.evaluatedPrice || form.evaluatedPrice <= 0)"
              />
              @if (submitted && (!form.evaluatedPrice || form.evaluatedPrice <= 0)) {
                <span class="field-error">O preço deve ser maior que zero.</span>
              }
            </div>

            @if (form.acquisitionType === 'OwnPurchase' || (isEditing() && editingItem()?.acquisitionType === 'OwnPurchase')) {
              <div class="form-group form-group-grow">
                <label for="itemCost">Preço de Custo (€)</label>
                <input
                  id="itemCost"
                  type="number"
                  [(ngModel)]="form.costPrice"
                  name="costPrice"
                  class="form-input"
                  placeholder="0.00"
                  min="0"
                  step="0.01"
                />
              </div>
            }
          </div>
        </div>

        <!-- Section: Tags -->
        <div class="card">
          <div class="card-title">Tags</div>
          <div class="tags-picker">
            @for (tag of tags(); track tag.externalId) {
              <label
                class="tag-chip"
                [class.tag-selected]="isTagSelected(tag.externalId)"
                [style.--tag-color]="tag.color || '#6366f1'"
              >
                <input
                  type="checkbox"
                  [checked]="isTagSelected(tag.externalId)"
                  (change)="toggleTag(tag.externalId)"
                />
                {{ tag.name }}
              </label>
            }
            @if (tags().length === 0) {
              <span class="text-muted">Nenhuma tag disponível.</span>
            }
          </div>
        </div>

        <!-- Actions -->
        <div class="form-actions">
          <button type="button" class="btn btn-outline" (click)="goBack()" [disabled]="saving()">Cancelar</button>
          <button type="submit" class="btn btn-primary" [disabled]="saving()">
            {{ saving() ? 'A guardar...' : (isEditing() ? 'Guardar Alterações' : 'Criar Item') }}
          </button>
        </div>
      </form>
    }
  `,
  styles: [`
    :host { display: block; }

    /* ── Topbar ── */
    .detail-topbar {
      display: flex;
      align-items: center;
      gap: 16px;
      margin-bottom: 24px;
    }

    .page-title {
      font-size: 22px;
      font-weight: 700;
      color: #1e293b;
      margin: 0;
      flex: 1;
    }

    /* ── Form Layout ── */
    .form-layout {
      display: flex;
      flex-direction: column;
      gap: 20px;
      max-width: 800px;
    }

    /* ── Cards ── */
    .card {
      background: #ffffff;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      padding: 24px;
    }

    .card-title {
      font-size: 13px;
      font-weight: 600;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-bottom: 20px;
    }

    /* ── Form ── */
    .form-group {
      margin-bottom: 16px;
    }

    .form-group:last-child {
      margin-bottom: 0;
    }

    .form-group label {
      display: block;
      font-size: 13px;
      font-weight: 600;
      color: #374151;
      margin-bottom: 6px;
    }

    .form-row {
      display: flex;
      gap: 16px;
    }

    .form-group-grow {
      flex: 1;
    }

    .form-group-small-md {
      width: 140px;
      flex-shrink: 0;
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
      background: white;
    }

    .form-input:focus {
      border-color: #6366f1;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
    }

    .form-input.input-error {
      border-color: #ef4444;
    }

    .form-textarea {
      resize: vertical;
      min-height: 80px;
    }

    .field-error {
      display: block;
      font-size: 12px;
      color: #ef4444;
      margin-top: 4px;
    }

    /* ── Tags Picker ── */
    .tags-picker {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    }

    .tag-chip {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 6px 14px;
      border-radius: 20px;
      font-size: 13px;
      font-weight: 500;
      cursor: pointer;
      border: 1.5px solid #e2e8f0;
      background: white;
      color: #475569;
      transition: all 0.15s;
      user-select: none;
    }

    .tag-chip input {
      display: none;
    }

    .tag-chip:hover {
      border-color: var(--tag-color, #6366f1);
      background: color-mix(in srgb, var(--tag-color, #6366f1) 8%, white);
    }

    .tag-chip.tag-selected {
      border-color: var(--tag-color, #6366f1);
      background: color-mix(in srgb, var(--tag-color, #6366f1) 12%, white);
      color: var(--tag-color, #6366f1);
      font-weight: 600;
    }

    /* ── Buttons ── */
    .btn {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 10px 20px;
      border-radius: 8px;
      font-size: 14px;
      font-weight: 600;
      cursor: pointer;
      border: 1px solid transparent;
      transition: all 0.15s;
    }

    .btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .btn-primary {
      background: #6366f1;
      color: white;
    }

    .btn-primary:hover:not(:disabled) {
      background: #4f46e5;
    }

    .btn-outline {
      background: white;
      color: #1e293b;
      border-color: #e2e8f0;
    }

    .btn-outline:hover:not(:disabled) {
      background: #f8fafc;
    }

    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: 12px;
      padding-top: 8px;
    }

    /* ── Alert ── */
    .alert {
      padding: 12px 16px;
      border-radius: 8px;
      font-size: 13px;
    }

    .alert-error {
      background: #fef2f2;
      color: #991b1b;
      border: 1px solid #fecaca;
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

    .text-muted {
      font-size: 13px;
      color: #94a3b8;
    }

    /* ── Responsive ── */
    @media (max-width: 768px) {
      .form-row {
        flex-direction: column;
        gap: 0;
      }

      .form-group-small-md {
        width: 100%;
      }

      .detail-topbar {
        flex-wrap: wrap;
      }
    }
  `]
})
export class ItemFormPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly itemService = inject(ItemService);
  private readonly brandService = inject(BrandService);
  private readonly categoryService = inject(CategoryService);
  private readonly tagService = inject(TagService);
  private readonly supplierService = inject(SupplierService);

  isEditing = signal(false);
  editingItem = signal<Item | null>(null);
  loadingData = signal(true);
  saving = signal(false);
  formError = signal<string | null>(null);
  submitted = false;

  brands = signal<SelectOption[]>([]);
  categories = signal<SelectOption[]>([]);
  tags = signal<SelectOption[]>([]);
  suppliers = signal<SupplierListItem[]>([]);

  selectedTagIds = new Set<string>();

  form = {
    name: '',
    description: '',
    brandExternalId: '',
    categoryExternalId: '',
    size: '',
    color: '',
    composition: '',
    condition: '',
    evaluatedPrice: null as number | null,
    costPrice: null as number | null,
    acquisitionType: '',
    origin: '',
    supplierExternalId: '',
    commissionPercentage: 50 as number | null,
  };

  ngOnInit(): void {
    const paramId = this.route.snapshot.paramMap.get('id');
    this.isEditing.set(paramId !== null && paramId !== 'new');

    // Load reference data
    forkJoin({
      brands: this.brandService.getAll(),
      categories: this.categoryService.getAll(),
      tags: this.tagService.getAll(),
      suppliers: this.supplierService.getAll(),
    }).subscribe({
      next: ({ brands, categories, tags, suppliers }) => {
        this.brands.set(brands.map(b => ({ externalId: b.externalId, name: b.name })));
        this.categories.set(categories.map((c: any) => ({ externalId: c.externalId, name: c.name })));
        this.tags.set(tags.map((t: any) => ({ externalId: t.externalId, name: t.name, color: t.color })));
        this.suppliers.set(suppliers);

        if (this.isEditing()) {
          this.loadItem(paramId!);
        } else {
          this.loadingData.set(false);
        }
      },
      error: () => {
        this.formError.set('Erro ao carregar dados de referência.');
        this.loadingData.set(false);
      }
    });
  }

  private loadItem(externalId: string): void {
    this.itemService.getItemById(externalId).subscribe({
      next: (item) => {
        this.editingItem.set(item);
        this.populateForm(item);
        this.loadingData.set(false);
      },
      error: () => {
        this.formError.set('Item não encontrado.');
        this.loadingData.set(false);
      }
    });
  }

  private populateForm(item: Item): void {
    // Find brand externalId from brands list
    const brand = this.brands().find(b => b.name === item.brand.name);
    const category = item.category
      ? this.categories().find(c => c.name === item.category!.name)
      : null;

    this.form.name = item.name;
    this.form.description = item.description || '';
    this.form.brandExternalId = brand?.externalId || '';
    this.form.categoryExternalId = category?.externalId || '';
    this.form.size = item.size;
    this.form.color = item.color;
    this.form.composition = item.composition || '';
    this.form.condition = item.condition;
    this.form.evaluatedPrice = item.evaluatedPrice;
    this.form.costPrice = item.costPrice || null;
    this.form.acquisitionType = item.acquisitionType;
    this.form.origin = item.origin;
    this.form.commissionPercentage = item.commissionPercentage;

    // Set supplier
    if (item.supplier) {
      const sup = this.suppliers().find(s => s.name === item.supplier!.name);
      this.form.supplierExternalId = sup?.externalId || '';
    }

    // Set tags
    this.selectedTagIds.clear();
    for (const tag of item.tags) {
      const t = this.tags().find(x => x.name === tag.name);
      if (t) this.selectedTagIds.add(t.externalId);
    }
  }

  isTagSelected(externalId: string): boolean {
    return this.selectedTagIds.has(externalId);
  }

  toggleTag(externalId: string): void {
    if (this.selectedTagIds.has(externalId)) {
      this.selectedTagIds.delete(externalId);
    } else {
      this.selectedTagIds.add(externalId);
    }
  }

  onAcquisitionTypeChange(): void {
    if (this.form.acquisitionType === 'Consignment') {
      this.form.origin = 'Consignment';
    } else {
      this.form.origin = '';
      this.form.supplierExternalId = '';
      this.form.commissionPercentage = null;
    }
  }

  goBack(): void {
    if (this.isEditing() && this.editingItem()) {
      this.router.navigate(['/inventory/items', this.editingItem()!.externalId]);
    } else {
      this.router.navigate(['/inventory/items']);
    }
  }

  onSubmit(): void {
    this.submitted = true;
    this.formError.set(null);

    if (!this.validateForm()) return;

    this.saving.set(true);

    if (this.isEditing()) {
      this.submitUpdate();
    } else {
      this.submitCreate();
    }
  }

  private validateForm(): boolean {
    if (!this.form.name.trim()) return false;
    if (!this.form.brandExternalId) return false;
    if (!this.form.size) return false;
    if (!this.form.color.trim()) return false;
    if (!this.form.condition) return false;
    if (!this.form.evaluatedPrice || this.form.evaluatedPrice <= 0) return false;

    if (!this.isEditing()) {
      if (!this.form.acquisitionType) return false;
      if (this.form.acquisitionType === 'Consignment' && !this.form.supplierExternalId) return false;
      if (this.form.acquisitionType === 'OwnPurchase' && !this.form.origin) return false;
    }

    return true;
  }

  private submitCreate(): void {
    const data: CreateItemRequest = {
      name: this.form.name.trim(),
      description: this.form.description.trim() || undefined,
      brandExternalId: this.form.brandExternalId,
      categoryExternalId: this.form.categoryExternalId || undefined,
      size: this.form.size,
      color: this.form.color.trim(),
      composition: this.form.composition.trim() || undefined,
      condition: this.form.condition,
      evaluatedPrice: this.form.evaluatedPrice!,
      costPrice: this.form.costPrice || undefined,
      acquisitionType: this.form.acquisitionType,
      origin: this.form.acquisitionType === 'Consignment' ? 'Consignment' : this.form.origin,
      supplierExternalId: this.form.supplierExternalId || undefined,
      commissionPercentage: this.form.commissionPercentage || undefined,
      tagExternalIds: this.selectedTagIds.size > 0 ? Array.from(this.selectedTagIds) : undefined,
    };

    this.itemService.createItem(data).subscribe({
      next: (result) => {
        this.saving.set(false);
        this.router.navigate(['/inventory/items', result.externalId]);
      },
      error: (err) => {
        this.saving.set(false);
        this.formError.set(err.error?.error || 'Erro ao criar item.');
      }
    });
  }

  private submitUpdate(): void {
    const data: UpdateItemRequest = {
      name: this.form.name.trim(),
      description: this.form.description.trim() || undefined,
      brandExternalId: this.form.brandExternalId,
      categoryExternalId: this.form.categoryExternalId || undefined,
      size: this.form.size,
      color: this.form.color.trim(),
      composition: this.form.composition.trim() || undefined,
      condition: this.form.condition,
      evaluatedPrice: this.form.evaluatedPrice!,
      costPrice: this.form.costPrice || undefined,
      commissionPercentage: this.form.commissionPercentage || undefined,
      tagExternalIds: Array.from(this.selectedTagIds),
    };

    this.itemService.updateItem(this.editingItem()!.externalId, data).subscribe({
      next: () => {
        this.saving.set(false);
        this.router.navigate(['/inventory/items', this.editingItem()!.externalId]);
      },
      error: (err) => {
        this.saving.set(false);
        this.formError.set(err.error?.error || 'Erro ao atualizar item.');
      }
    });
  }
}
