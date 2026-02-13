import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ItemService } from '../services/item.service';
import { Item, ItemPhoto, ItemStatus, ItemCondition } from '../../../core/models/item.model';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'oui-item-detail-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    @if (loading()) {
      <div class="state-message">A carregar...</div>
    } @else if (item()) {
      <!-- Top bar: back + actions -->
      <div class="detail-topbar">
        <button class="btn btn-outline" (click)="goBack()">← Voltar</button>
        <div class="detail-topbar-right">
          <button class="btn btn-outline" [routerLink]="['/inventory/items', item()!.externalId, 'edit']">Editar</button>
        </div>
      </div>

      <!-- Item header: name, ID, status -->
      <div class="item-header">
        <div class="item-header-info">
          <span class="item-id-label">{{ item()!.identificationNumber }}</span>
          <h1 class="item-name">{{ item()!.name }}</h1>
          @if (item()!.description) {
            <p class="item-description">{{ item()!.description }}</p>
          }
        </div>
        <span class="badge badge-lg" [ngClass]="'badge-' + getStatusBadgeClass(item()!.status)">
          {{ getStatusLabel(item()!.status) }}
        </span>
      </div>

      <!-- KPI Stats -->
      <div class="stat-grid">
        <div class="card stat-card">
          <div class="stat-label">Preço</div>
          <div class="stat-value stat-price">€{{ item()!.evaluatedPrice.toFixed(2) }}</div>
        </div>
        <div class="card stat-card">
          <div class="stat-label">Condição</div>
          <div class="stat-value">{{ getConditionLabel(item()!.condition) }}</div>
        </div>
        <div class="card stat-card">
          <div class="stat-label">Dias em Stock</div>
          <div class="stat-value" [class]="getDaysClass(item()!.daysInStock)">{{ item()!.daysInStock }}</div>
        </div>
        <div class="card stat-card">
          <div class="stat-label">Origem</div>
          <div class="stat-value">{{ getAcquisitionLabel(item()!.acquisitionType) }}</div>
        </div>
      </div>

      <!-- Two-column: Photos + Info -->
      <div class="detail-grid">
        <!-- Photos Card -->
        <div class="card">
          <div class="card-title-row">
            <span class="card-title">Fotos</span>
            <span class="photo-count">{{ item()!.photos.length }}/10</span>
          </div>
          @if (item()!.photos.length > 0) {
            <div class="main-photo">
              <img [src]="getPhotoUrl(selectedPhoto()?.filePath)" [alt]="item()!.name" />
              <button
                class="photo-delete-btn"
                (click)="deletePhoto(selectedPhoto()!)"
                [disabled]="deletingPhoto()"
                title="Eliminar foto"
              >{{ deletingPhoto() ? '...' : '✕' }}</button>
            </div>
            <div class="photo-thumbs">
              @for (photo of item()!.photos; track photo.externalId) {
                <div class="thumb-wrapper" [class.active]="selectedPhoto()?.externalId === photo.externalId">
                  <img
                    [src]="getPhotoUrl(photo.thumbnailPath || photo.filePath)"
                    [alt]="'Foto ' + photo.displayOrder"
                    (click)="selectPhoto(photo)"
                  />
                  @if (photo.isPrimary) {
                    <span class="primary-badge">★</span>
                  }
                </div>
              }
            </div>
          } @else {
            <div class="no-photos">Sem fotos disponíveis</div>
          }

          <!-- Upload zone -->
          @if (item()!.photos.length < 10) {
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
                <span class="upload-icon">📷</span>
                <span class="upload-text">Arrastar fotos ou clicar para selecionar</span>
                <span class="upload-hint">JPEG, PNG ou WebP · Máx. 10 MB</span>
              }
            </div>
          }

          @if (uploadError()) {
            <div class="photo-error">{{ uploadError() }}</div>
          }
        </div>

        <!-- Info Card -->
        <div class="card">
          <div class="card-title">Informações</div>
          <div class="info-grid">
            <div class="info-row">
              <label>Marca</label>
              <span>{{ item()!.brand.name }}</span>
            </div>
            @if (item()!.category) {
              <div class="info-row">
                <label>Categoria</label>
                <span>{{ item()!.category?.name }}</span>
              </div>
            }
            <div class="info-row">
              <label>Tamanho</label>
              <span>{{ item()!.size }}</span>
            </div>
            <div class="info-row">
              <label>Cor</label>
              <span>{{ item()!.color }}</span>
            </div>
            @if (item()!.composition) {
              <div class="info-row full-width">
                <label>Composição</label>
                <span>{{ item()!.composition }}</span>
              </div>
            }
            @if (item()!.acquisitionType === 'Consignment' && item()!.supplier) {
              <div class="info-row">
                <label>Fornecedor</label>
                <span>{{ item()!.supplier?.name }}</span>
              </div>
              <div class="info-row">
                <label>Comissão</label>
                <span>{{ item()!.commissionPercentage }}%</span>
              </div>
            }
            <div class="info-row">
              <label>Criado em</label>
              <span>{{ item()!.createdOn | date: 'dd/MM/yyyy HH:mm' }}</span>
            </div>
          </div>

          @if (item()!.tags.length > 0) {
            <div class="tags-section">
              <label>Tags</label>
              <div class="tags-list">
                @for (tag of item()!.tags; track tag.id) {
                  <span
                    class="tag"
                    [style.background-color]="tag.color ? tag.color + '22' : '#f1f5f9'"
                    [style.color]="tag.color || '#475569'"
                    [style.border-color]="tag.color ? tag.color + '44' : '#e2e8f0'"
                  >
                    {{ tag.name }}
                  </span>
                }
              </div>
            </div>
          }
        </div>
      </div>

      <!-- Rejection notice -->
      @if (item()!.isRejected && item()!.rejectionReason) {
        <div class="rejection-card">
          <strong>Rejeitado:</strong> {{ item()!.rejectionReason }}
        </div>
      }
    }
  `,
  styles: [`
    :host { display: block; }

    /* ── Detail Topbar ── */
    .detail-topbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
    }

    .detail-topbar-right {
      display: flex;
      gap: 8px;
    }

    /* ── Item Header ── */
    .item-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 24px;
      gap: 20px;
    }

    .item-header-info {
      flex: 1;
      min-width: 0;
    }

    .item-id-label {
      font-family: monospace;
      font-size: 12px;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      display: block;
      margin-bottom: 4px;
    }

    .item-name {
      font-size: 24px;
      font-weight: 700;
      margin: 0 0 8px;
      color: #1e293b;
    }

    .item-description {
      font-size: 14px;
      color: #64748b;
      margin: 0;
      line-height: 1.6;
    }

    /* ── Stats Grid ── */
    .stat-grid {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 16px;
      margin-bottom: 24px;
    }

    .stat-card {
      text-align: center;
      padding: 20px;
    }

    .stat-label {
      font-size: 12px;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      font-weight: 600;
      margin-bottom: 8px;
    }

    .stat-value {
      font-size: 22px;
      font-weight: 800;
      color: #1e293b;
    }

    .stat-price          { color: #6366f1; }
    .stat-days-warning   { color: #f59e0b; }
    .stat-days-danger    { color: #ef4444; }

    /* ── Detail Grid ── */
    .detail-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 20px;
      margin-bottom: 24px;
    }

    /* ── Cards ── */
    .card {
      background: #ffffff;
      border-radius: 12px;
      border: 1px solid #e2e8f0;
      padding: 20px;
    }

    .card-title {
      font-size: 13px;
      font-weight: 600;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-bottom: 16px;
    }

    /* ── Photo Section ── */
    .main-photo {
      width: 100%;
      height: 360px;
      background: #f1f5f9;
      border-radius: 8px;
      overflow: hidden;
      margin-bottom: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .main-photo img {
      width: 100%;
      height: 100%;
      object-fit: contain;
    }

    .photo-thumbs {
      display: flex;
      gap: 8px;
      overflow-x: auto;
      padding: 4px 0;
    }

    /* thumbnails handled by .thumb-wrapper */

    .no-photos {
      display: flex;
      align-items: center;
      justify-content: center;
      height: 200px;
      color: #94a3b8;
      font-size: 14px;
      background: #f1f5f9;
      border-radius: 8px;
    }

    .card-title-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 16px;
    }

    .photo-count {
      font-size: 12px;
      color: #94a3b8;
      font-weight: 600;
    }

    .main-photo {
      position: relative;
    }

    .photo-delete-btn {
      position: absolute;
      top: 8px;
      right: 8px;
      width: 32px;
      height: 32px;
      border-radius: 50%;
      background: rgba(0,0,0,0.6);
      color: white;
      border: none;
      font-size: 14px;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      opacity: 0;
      transition: opacity 0.15s;
    }

    .main-photo:hover .photo-delete-btn {
      opacity: 1;
    }

    .photo-delete-btn:hover {
      background: #ef4444;
    }

    .photo-delete-btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .thumb-wrapper {
      position: relative;
      flex-shrink: 0;
    }

    .thumb-wrapper img {
      width: 64px;
      height: 64px;
      object-fit: cover;
      border-radius: 8px;
      cursor: pointer;
      border: 2px solid transparent;
      transition: all 0.15s;
    }

    .thumb-wrapper:hover img {
      border-color: #a5b4fc;
    }

    .thumb-wrapper.active img {
      border-color: #6366f1;
    }

    .primary-badge {
      position: absolute;
      bottom: 2px;
      right: 2px;
      font-size: 10px;
      background: #6366f1;
      color: white;
      border-radius: 50%;
      width: 16px;
      height: 16px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    /* ── Upload zone ── */
    .upload-zone {
      margin-top: 16px;
      padding: 24px;
      border: 2px dashed #d1d5db;
      border-radius: 10px;
      text-align: center;
      cursor: pointer;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 4px;
      transition: all 0.15s;
      background: #fafafa;
    }

    .upload-zone:hover, .upload-zone.dragging {
      border-color: #6366f1;
      background: #eef2ff;
    }

    .upload-icon { font-size: 24px; }

    .upload-text {
      font-size: 13px;
      color: #475569;
      font-weight: 600;
    }

    .upload-hint {
      font-size: 11px;
      color: #94a3b8;
    }

    .photo-error {
      margin-top: 8px;
      padding: 8px 12px;
      border-radius: 8px;
      font-size: 12px;
      background: #fef2f2;
      color: #991b1b;
      border: 1px solid #fecaca;
    }

    /* ── Info Section ── */
    .info-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .info-row {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .info-row.full-width {
      grid-column: 1 / -1;
    }

    .info-row label {
      font-size: 12px;
      color: #64748b;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .info-row span {
      font-size: 14px;
      color: #1e293b;
      font-weight: 500;
    }

    /* ── Tags ── */
    .tags-section {
      margin-top: 20px;
      padding-top: 16px;
      border-top: 1px solid #e2e8f0;
    }

    .tags-section label {
      display: block;
      font-size: 12px;
      color: #64748b;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-bottom: 8px;
    }

    .tags-list {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }

    .tag {
      padding: 4px 12px;
      border-radius: 20px;
      font-size: 12px;
      font-weight: 600;
      border: 1px solid;
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

    .badge-lg {
      padding: 6px 16px;
      font-size: 13px;
    }

    .badge-success { background: #dcfce7; color: #166534; }
    .badge-warning { background: #fef3c7; color: #92400e; }
    .badge-danger  { background: #fee2e2; color: #991b1b; }
    .badge-info    { background: #dbeafe; color: #1e40af; }
    .badge-gray    { background: #f1f5f9; color: #475569; }

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
      text-decoration: none;
    }

    .btn-outline {
      background: white;
      color: #1e293b;
      border-color: #e2e8f0;
    }

    .btn-outline:hover {
      background: #f8fafc;
    }

    /* ── Rejection ── */
    .rejection-card {
      padding: 16px 20px;
      background: #fee2e2;
      border: 1px solid #fecaca;
      border-left: 4px solid #ef4444;
      border-radius: 8px;
      color: #991b1b;
      font-size: 14px;
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
    @media (max-width: 1024px) {
      .stat-grid {
        grid-template-columns: repeat(2, 1fr);
      }
    }

    @media (max-width: 768px) {
      .detail-grid {
        grid-template-columns: 1fr;
      }

      .stat-grid {
        grid-template-columns: 1fr 1fr;
      }

      .item-header {
        flex-direction: column;
      }
    }
  `]
})
export class ItemDetailPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly itemService = inject(ItemService);

  private readonly baseUrl = environment.apiUrl.replace('/api', '');

  item = signal<Item | null>(null);
  loading = signal(false);
  selectedPhoto = signal<ItemPhoto | null>(null);
  uploading = signal(false);
  deletingPhoto = signal(false);
  uploadError = signal<string | null>(null);
  isDragging = signal(false);

  ngOnInit(): void {
    const externalId = this.route.snapshot.paramMap.get('id');
    if (externalId) {
      this.loadItem(externalId);
    }
  }

  loadItem(externalId: string): void {
    this.loading.set(true);
    this.itemService.getItemById(externalId).subscribe({
      next: (item) => {
        this.item.set(item);
        if (item.photos.length > 0) {
          this.selectedPhoto.set(item.photos[0]);
        }
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.router.navigate(['/inventory/items']);
      }
    });
  }

  selectPhoto(photo: ItemPhoto): void {
    this.selectedPhoto.set(photo);
  }

  goBack(): void {
    this.router.navigate(['/inventory/items']);
  }

  getStatusLabel(status: ItemStatus): string {
    const labels: Record<string, string> = {
      'Received': 'Recebido',
      'Evaluated': 'Avaliado',
      'AwaitingAcceptance': 'Aguardando',
      'ToSell': 'À Venda',
      'Sold': 'Vendido',
      'Returned': 'Devolvido',
      'Paid': 'Pago',
      'Rejected': 'Rejeitado'
    };
    return labels[status] || status;
  }

  getStatusBadgeClass(status: ItemStatus): string {
    const classes: Record<string, string> = {
      'Received': 'gray',
      'Evaluated': 'info',
      'AwaitingAcceptance': 'warning',
      'ToSell': 'success',
      'Sold': 'info',
      'Returned': 'gray',
      'Paid': 'success',
      'Rejected': 'danger'
    };
    return classes[status] || 'gray';
  }

  getConditionLabel(condition: ItemCondition): string {
    const labels: Record<string, string> = {
      'Excellent': 'Excelente',
      'VeryGood': 'Muito Bom',
      'Good': 'Bom',
      'Fair': 'Razoável',
      'Poor': 'Mau'
    };
    return labels[condition] || condition;
  }

  getAcquisitionLabel(type: string): string {
    const labels: Record<string, string> = {
      'Consignment': 'Consignação',
      'OwnPurchase': 'Compra Própria'
    };
    return labels[type] || type;
  }

  getDaysClass(days: number): string {
    if (days >= 60) return 'stat-days-danger';
    if (days >= 30) return 'stat-days-warning';
    return '';
  }

  getPhotoUrl(path?: string): string {
    if (!path) return '';
    return `${this.baseUrl}${path}`;
  }

  // ── Photo upload ──

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
      input.value = ''; // Reset so same file can be re-selected
    }
  }

  private uploadFiles(files: File[]): void {
    this.uploadError.set(null);

    // Client-side validation
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

    const currentCount = this.item()?.photos.length ?? 0;
    if (currentCount + files.length > 10) {
      this.uploadError.set(`Máximo 10 fotos. Tem ${currentCount}, está a enviar ${files.length}.`);
      return;
    }

    this.uploading.set(true);

    this.itemService.uploadPhotos(this.item()!.externalId, files).subscribe({
      next: (newPhotos) => {
        this.uploading.set(false);
        // Reload item to get updated photos
        this.loadItem(this.item()!.externalId);
      },
      error: (err) => {
        this.uploading.set(false);
        this.uploadError.set(err.error?.error || 'Erro ao carregar fotos.');
      }
    });
  }

  deletePhoto(photo: ItemPhoto): void {
    if (!confirm('Eliminar esta foto?')) return;

    this.deletingPhoto.set(true);

    this.itemService.deletePhoto(this.item()!.externalId, photo.externalId).subscribe({
      next: () => {
        this.deletingPhoto.set(false);
        // Reload item to get updated photos
        this.loadItem(this.item()!.externalId);
      },
      error: () => {
        this.deletingPhoto.set(false);
        this.uploadError.set('Erro ao eliminar foto.');
      }
    });
  }
}
