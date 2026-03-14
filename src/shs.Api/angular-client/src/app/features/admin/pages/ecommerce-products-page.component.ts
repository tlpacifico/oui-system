import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EcommerceService } from '../../inventory/services/ecommerce.service';
import { EcommerceProduct, EcommerceProductDetail, EcommerceProductPhoto, EcommerceProductStatus, UpdateProductRequest } from '../../../core/models/ecommerce.model';
import { HasPermissionDirective } from '../../../core/auth/directives/has-permission.directive';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'oui-ecommerce-products-page',
  standalone: true,
  imports: [CommonModule, FormsModule, HasPermissionDirective],
  template: `
    <div class="page-header">
      <div>
        <h1 class="page-title">Produtos E-commerce</h1>
        <p class="page-subtitle">{{ totalCount() }} produtos no total</p>
      </div>
    </div>

    <!-- Filters -->
    <div class="card filters-card">
      <div class="filters-bar">
        <input
          type="text"
          placeholder="Pesquisar por título..."
          [ngModel]="searchText()"
          (ngModelChange)="searchText.set($event)"
          (keyup.enter)="onSearch()"
          class="filter-input filter-search"
        />
        <select
          class="filter-input filter-select"
          [ngModel]="statusFilter()"
          (ngModelChange)="statusFilter.set($event); onSearch()"
        >
          <option value="">Todos os estados</option>
          <option value="Published">Publicado</option>
          <option value="Draft">Rascunho</option>
          <option value="Reserved">Reservado</option>
          <option value="Sold">Vendido</option>
          <option value="Unpublished">Despublicado</option>
        </select>
        <button class="btn btn-outline btn-sm" (click)="onSearch()">Pesquisar</button>
        @if (searchText() || statusFilter()) {
          <button class="btn btn-outline btn-sm" (click)="clearFilters()">Limpar</button>
        }
      </div>
    </div>

    @if (loading()) {
      <div class="state-message">A carregar...</div>
    } @else if (products().length === 0) {
      <div class="state-message">
        @if (searchText() || statusFilter()) {
          Nenhum produto encontrado com os filtros aplicados.
        } @else {
          Nenhum produto no e-commerce.
        }
      </div>
    } @else {
      <div class="card table-card">
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th style="width: 50px"></th>
                <th>Título</th>
                <th>Marca</th>
                <th>Categoria</th>
                <th>Tamanho</th>
                <th>Preço</th>
                <th>Estado</th>
                <th>Publicado em</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              @for (product of products(); track product.externalId) {
                <tr>
                  <td>
                    @if (product.primaryPhotoUrl) {
                      <img [src]="getPhotoUrl(product.primaryPhotoUrl)" class="product-thumb" alt="" />
                    } @else {
                      <div class="product-thumb-placeholder">—</div>
                    }
                  </td>
                  <td><b>{{ product.title }}</b></td>
                  <td>{{ product.brandName }}</td>
                  <td>{{ product.categoryName || '—' }}</td>
                  <td>{{ product.size }}</td>
                  <td>{{ product.price | currency: 'EUR' : 'symbol' : '1.2-2' }}</td>
                  <td>
                    <span class="badge" [ngClass]="getStatusBadgeClass(product.status)">
                      {{ getStatusLabel(product.status) }}
                    </span>
                  </td>
                  <td>{{ product.publishedAt ? (product.publishedAt | date: 'dd/MM/yyyy HH:mm') : '—' }}</td>
                  <td class="cell-actions">
                    <button
                      class="btn btn-outline btn-sm"
                      (click)="openEdit(product)"
                      *hasPermission="'ecommerce.products.update'"
                    >
                      Editar
                    </button>
                    @if (product.status === 'Published' || product.status === 'Draft') {
                      <button
                        class="btn btn-outline btn-sm btn-danger-outline"
                        (click)="confirmUnpublish(product)"
                        *hasPermission="'ecommerce.products.unpublish'"
                      >
                        Despublicar
                      </button>
                    }
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>

      <!-- Pagination -->
      @if (totalPages() > 1) {
        <div class="pagination">
          <button class="btn btn-outline btn-sm" [disabled]="currentPage() <= 1" (click)="goToPage(currentPage() - 1)">
            ← Anterior
          </button>
          <span class="pagination-info">Página {{ currentPage() }} de {{ totalPages() }}</span>
          <button class="btn btn-outline btn-sm" [disabled]="currentPage() >= totalPages()" (click)="goToPage(currentPage() + 1)">
            Seguinte →
          </button>
        </div>
      }
    }

    <!-- Edit Modal -->
    @if (showEditModal()) {
      <div class="modal-overlay" (click)="closeEditModal()"></div>
      <div class="modal modal-lg">
        <div class="modal-header">
          <h2>Editar Produto</h2>
          <button class="modal-close" (click)="closeEditModal()">&times;</button>
        </div>
        <div class="modal-body">
          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Título</label>
              <input
                type="text"
                class="form-input"
                [ngModel]="editForm().title"
                (ngModelChange)="updateEditForm('title', $event)"
              />
            </div>
            <div class="form-group">
              <label class="form-label">Preço (€)</label>
              <input
                type="number"
                class="form-input"
                [ngModel]="editForm().price"
                (ngModelChange)="updateEditForm('price', $event)"
                min="0"
                step="0.01"
              />
            </div>
            <div class="form-group">
              <label class="form-label">Marca</label>
              <input
                type="text"
                class="form-input"
                [ngModel]="editForm().brandName"
                (ngModelChange)="updateEditForm('brandName', $event)"
              />
            </div>
            <div class="form-group">
              <label class="form-label">Categoria</label>
              <input
                type="text"
                class="form-input"
                [ngModel]="editForm().categoryName"
                (ngModelChange)="updateEditForm('categoryName', $event)"
              />
            </div>
            <div class="form-group">
              <label class="form-label">Tamanho</label>
              <input
                type="text"
                class="form-input"
                [ngModel]="editForm().size"
                (ngModelChange)="updateEditForm('size', $event)"
              />
            </div>
            <div class="form-group">
              <label class="form-label">Cor</label>
              <input
                type="text"
                class="form-input"
                [ngModel]="editForm().color"
                (ngModelChange)="updateEditForm('color', $event)"
              />
            </div>
            <div class="form-group">
              <label class="form-label">Condição</label>
              <select
                class="form-input"
                [ngModel]="editForm().condition"
                (ngModelChange)="updateEditForm('condition', $event)"
              >
                <option value="">— Selecionar —</option>
                <option value="Excellent">Excelente</option>
                <option value="VeryGood">Muito Bom</option>
                <option value="Good">Bom</option>
                <option value="Fair">Razoável</option>
                <option value="Poor">Fraco</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Composição</label>
              <input
                type="text"
                class="form-input"
                [ngModel]="editForm().composition"
                (ngModelChange)="updateEditForm('composition', $event)"
              />
            </div>
          </div>
          <div class="form-group" style="margin-top: 0.5rem">
            <label class="form-label">Descrição</label>
            <textarea
              class="form-input"
              [ngModel]="editForm().description"
              (ngModelChange)="updateEditForm('description', $event)"
              rows="3"
            ></textarea>
          </div>

          <!-- Photos Section -->
          <div class="photos-section">
            <div class="photos-header">
              <span class="form-label">Fotos</span>
              <span class="photo-count">{{ editPhotos().length }}/10</span>
            </div>
            @if (editPhotos().length > 0) {
              <div class="photo-grid">
                @for (photo of editPhotos(); track photo.externalId) {
                  <div class="photo-item">
                    <img [src]="getPhotoUrl(photo.filePath)" alt="" />
                    <button
                      class="photo-remove-btn"
                      (click)="removePhoto(photo)"
                      [disabled]="deletingPhotoId() === photo.externalId"
                      title="Eliminar foto"
                    >{{ deletingPhotoId() === photo.externalId ? '...' : '✕' }}</button>
                    @if (photo.isPrimary) {
                      <span class="photo-primary-badge">Principal</span>
                    }
                  </div>
                }
              </div>
            }
            @if (editPhotos().length < 10) {
              <div
                class="upload-zone"
                [class.dragging]="isDragging()"
                (dragover)="onDragOver($event)"
                (dragleave)="onDragLeave($event)"
                (drop)="onDrop($event)"
                (click)="fileInput.click()"
              >
                <input
                  #fileInput
                  type="file"
                  accept="image/jpeg,image/png,image/webp"
                  multiple
                  style="display:none"
                  (change)="onFilesSelected($event)"
                />
                @if (uploading()) {
                  <span class="upload-text">A carregar...</span>
                } @else {
                  <span class="upload-text">Arrastar fotos ou clicar para selecionar</span>
                  <span class="upload-hint">JPEG, PNG ou WebP — Máx. 10 MB</span>
                }
              </div>
            }
            @if (uploadError()) {
              <div class="alert alert-danger" style="margin-top: 0.5rem">{{ uploadError() }}</div>
            }
          </div>

          @if (editError()) {
            <div class="alert alert-danger">{{ editError() }}</div>
          }
        </div>
        <div class="modal-footer">
          <button class="btn btn-outline" (click)="closeEditModal()">Cancelar</button>
          <button class="btn btn-primary" (click)="saveProduct()" [disabled]="saving()">
            {{ saving() ? 'A guardar...' : 'Guardar' }}
          </button>
        </div>
      </div>
    }

    <!-- Unpublish Confirmation -->
    @if (showUnpublishConfirm()) {
      <div class="modal-overlay" (click)="cancelUnpublish()"></div>
      <div class="modal modal-sm">
        <div class="modal-header">
          <h2>Confirmar Despublicação</h2>
          <button class="modal-close" (click)="cancelUnpublish()">&times;</button>
        </div>
        <div class="modal-body">
          <p>Tem a certeza que deseja despublicar <strong>{{ productToUnpublish()?.title }}</strong>?</p>
          <p style="color: #64748b; font-size: 0.875rem;">O produto deixará de estar visível na loja online.</p>
        </div>
        <div class="modal-footer">
          <button class="btn btn-outline" (click)="cancelUnpublish()">Cancelar</button>
          <button class="btn btn-danger" (click)="unpublishProduct()" [disabled]="unpublishing()">
            {{ unpublishing() ? 'A despublicar...' : 'Despublicar' }}
          </button>
        </div>
      </div>
    }
  `,
  styles: [`
    :host { display: block; }

    .product-thumb {
      width: 40px;
      height: 40px;
      object-fit: cover;
      border-radius: 6px;
    }

    .product-thumb-placeholder {
      width: 40px;
      height: 40px;
      background: #f1f5f9;
      border-radius: 6px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: #94a3b8;
      font-size: 0.75rem;
    }

    .pagination {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 1rem;
      padding: 1rem 0;
    }

    .pagination-info {
      font-size: 0.875rem;
      color: #64748b;
    }

    .form-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 0.75rem 1rem;
    }

    .photos-section {
      margin-top: 1rem;
      padding-top: 1rem;
      border-top: 1px solid #e2e8f0;
    }

    .photos-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 0.75rem;
    }

    .photo-count {
      font-size: 0.8rem;
      color: #64748b;
    }

    .photo-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
      gap: 0.5rem;
      margin-bottom: 0.75rem;
    }

    .photo-item {
      position: relative;
      aspect-ratio: 1;
      border-radius: 8px;
      overflow: hidden;
      border: 1px solid #e2e8f0;
    }

    .photo-item img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .photo-remove-btn {
      position: absolute;
      top: 4px;
      right: 4px;
      width: 22px;
      height: 22px;
      border-radius: 50%;
      background: rgba(0,0,0,0.6);
      color: #fff;
      border: none;
      cursor: pointer;
      font-size: 0.7rem;
      display: flex;
      align-items: center;
      justify-content: center;
      opacity: 0;
      transition: opacity 0.15s;
    }

    .photo-item:hover .photo-remove-btn {
      opacity: 1;
    }

    .photo-primary-badge {
      position: absolute;
      bottom: 4px;
      left: 4px;
      background: rgba(0,0,0,0.6);
      color: #fff;
      font-size: 0.65rem;
      padding: 1px 5px;
      border-radius: 4px;
    }

    .upload-zone {
      border: 2px dashed #cbd5e1;
      border-radius: 8px;
      padding: 1.25rem;
      text-align: center;
      cursor: pointer;
      transition: border-color 0.15s, background 0.15s;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.25rem;
    }

    .upload-zone:hover, .upload-zone.dragging {
      border-color: #6366f1;
      background: #f5f3ff;
    }

    .upload-text {
      font-size: 0.85rem;
      color: #475569;
    }

    .upload-hint {
      font-size: 0.75rem;
      color: #94a3b8;
    }
  `]
})
export class EcommerceProductsPageComponent implements OnInit {
  private readonly ecommerceService = inject(EcommerceService);
  private readonly baseUrl = environment.apiUrl.replace('/api', '');

  readonly products = signal<EcommerceProduct[]>([]);
  readonly loading = signal(false);
  readonly searchText = signal('');
  readonly statusFilter = signal('');
  readonly currentPage = signal(1);
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);

  // Edit modal
  readonly showEditModal = signal(false);
  readonly editingProductId = signal<string | null>(null);
  readonly editForm = signal<UpdateProductRequest>({});
  readonly editError = signal('');
  readonly saving = signal(false);

  // Photo management
  readonly editPhotos = signal<EcommerceProductPhoto[]>([]);
  readonly uploading = signal(false);
  readonly uploadError = signal('');
  readonly deletingPhotoId = signal<string | null>(null);
  readonly isDragging = signal(false);

  // Unpublish
  readonly showUnpublishConfirm = signal(false);
  readonly productToUnpublish = signal<EcommerceProduct | null>(null);
  readonly unpublishing = signal(false);

  ngOnInit() {
    this.loadProducts();
  }

  loadProducts() {
    this.loading.set(true);
    const params: any = {
      page: this.currentPage(),
      pageSize: 20
    };
    if (this.statusFilter()) params.status = this.statusFilter();
    if (this.searchText()) params.search = this.searchText();

    this.ecommerceService.getProducts(params).subscribe({
      next: (res) => {
        this.products.set(res.items);
        this.totalCount.set(res.totalCount);
        this.totalPages.set(res.totalPages);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  onSearch() {
    this.currentPage.set(1);
    this.loadProducts();
  }

  clearFilters() {
    this.searchText.set('');
    this.statusFilter.set('');
    this.currentPage.set(1);
    this.loadProducts();
  }

  goToPage(page: number) {
    this.currentPage.set(page);
    this.loadProducts();
  }

  getStatusLabel(status: string): string {
    const labels: Record<string, string> = {
      Draft: 'Rascunho',
      Published: 'Publicado',
      Reserved: 'Reservado',
      Sold: 'Vendido',
      Unpublished: 'Despublicado'
    };
    return labels[status] || status;
  }

  getStatusBadgeClass(status: string): string {
    const classes: Record<string, string> = {
      Draft: 'badge-gray',
      Published: 'badge-green',
      Reserved: 'badge-blue',
      Sold: 'badge-purple',
      Unpublished: 'badge-red'
    };
    return classes[status] || 'badge-gray';
  }

  openEdit(product: EcommerceProduct) {
    this.editingProductId.set(product.externalId);
    this.editError.set('');

    this.ecommerceService.getProductById(product.externalId).subscribe({
      next: (detail) => {
        this.editForm.set({
          title: detail.title,
          description: detail.description || '',
          price: detail.price,
          brandName: detail.brandName || '',
          categoryName: detail.categoryName || '',
          size: detail.size || '',
          color: detail.color || '',
          condition: detail.condition || '',
          composition: detail.composition || ''
        });
        this.editPhotos.set(detail.photos || []);
        this.uploadError.set('');
        this.showEditModal.set(true);
      },
      error: () => {
        this.editError.set('Erro ao carregar detalhes do produto.');
      }
    });
  }

  closeEditModal() {
    this.showEditModal.set(false);
    this.editingProductId.set(null);
    this.editPhotos.set([]);
    this.uploadError.set('');
  }

  updateEditForm(field: string, value: any) {
    this.editForm.update(f => ({ ...f, [field]: value }));
  }

  saveProduct() {
    const id = this.editingProductId();
    if (!id) return;

    this.saving.set(true);
    this.ecommerceService.updateProduct(id, this.editForm()).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeEditModal();
        this.loadProducts();
      },
      error: (err: { error?: { error?: string } }) => {
        this.saving.set(false);
        this.editError.set(err.error?.error || 'Erro ao guardar produto.');
      }
    });
  }

  getPhotoUrl(path?: string): string {
    if (!path) return '';
    return `${this.baseUrl}${path}`;
  }

  // ── Photo management ──

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.uploadFiles(Array.from(files));
    }
  }

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.uploadFiles(Array.from(input.files));
      input.value = '';
    }
  }

  private uploadFiles(files: File[]): void {
    this.uploadError.set('');
    const id = this.editingProductId();
    if (!id) return;

    const allowed = ['image/jpeg', 'image/png', 'image/webp'];
    const maxSize = 10 * 1024 * 1024;

    for (const file of files) {
      if (!allowed.includes(file.type)) {
        this.uploadError.set(`Tipo não suportado: ${file.name}. Use JPEG, PNG ou WebP.`);
        return;
      }
      if (file.size > maxSize) {
        this.uploadError.set(`Ficheiro demasiado grande: ${file.name}. Máximo 10 MB.`);
        return;
      }
    }

    const currentCount = this.editPhotos().length;
    if (currentCount + files.length > 10) {
      this.uploadError.set(`Máximo 10 fotos. Tem ${currentCount}, está a enviar ${files.length}.`);
      return;
    }

    this.uploading.set(true);

    this.ecommerceService.uploadPhotos(id, files).subscribe({
      next: () => {
        this.uploading.set(false);
        this.reloadEditPhotos(id);
      },
      error: (err: { error?: { error?: string } }) => {
        this.uploading.set(false);
        this.uploadError.set(err.error?.error || 'Erro ao carregar fotos.');
      }
    });
  }

  removePhoto(photo: EcommerceProductPhoto): void {
    if (!confirm('Eliminar esta foto?')) return;
    const id = this.editingProductId();
    if (!id) return;

    this.deletingPhotoId.set(photo.externalId);

    this.ecommerceService.deletePhoto(id, photo.externalId).subscribe({
      next: () => {
        this.deletingPhotoId.set(null);
        this.reloadEditPhotos(id);
      },
      error: () => {
        this.deletingPhotoId.set(null);
        this.uploadError.set('Erro ao eliminar foto.');
      }
    });
  }

  private reloadEditPhotos(productExternalId: string): void {
    this.ecommerceService.getProductById(productExternalId).subscribe({
      next: (detail) => {
        this.editPhotos.set(detail.photos || []);
      }
    });
  }

  confirmUnpublish(product: EcommerceProduct) {
    this.productToUnpublish.set(product);
    this.showUnpublishConfirm.set(true);
  }

  cancelUnpublish() {
    this.showUnpublishConfirm.set(false);
    this.productToUnpublish.set(null);
  }

  unpublishProduct() {
    const product = this.productToUnpublish();
    if (!product) return;

    this.unpublishing.set(true);
    this.ecommerceService.unpublishProduct(product.externalId).subscribe({
      next: () => {
        this.unpublishing.set(false);
        this.cancelUnpublish();
        this.loadProducts();
      },
      error: (err: { error?: { error?: string } }) => {
        this.unpublishing.set(false);
        alert(err.error?.error || 'Erro ao despublicar produto.');
      }
    });
  }
}
