import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ItemService } from '../services/item.service';
import { Item, ItemPhoto, ItemStatus, ItemCondition } from '../../../core/models/item.model';

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
          <div class="card-title">Fotos</div>
          @if (item()!.photos.length > 0) {
            <div class="main-photo">
              <img [src]="selectedPhoto()?.filePath" [alt]="item()!.name" />
            </div>
            <div class="photo-thumbs">
              @for (photo of item()!.photos; track photo.externalId) {
                <img
                  [src]="photo.thumbnailPath || photo.filePath"
                  [alt]="'Foto ' + photo.displayOrder"
                  (click)="selectPhoto(photo)"
                  [class.active]="selectedPhoto()?.externalId === photo.externalId"
                />
              }
            </div>
          } @else {
            <div class="no-photos">Sem fotos disponíveis</div>
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

    .photo-thumbs img {
      width: 64px;
      height: 64px;
      object-fit: cover;
      border-radius: 8px;
      cursor: pointer;
      border: 2px solid transparent;
      transition: all 0.15s;
      flex-shrink: 0;
    }

    .photo-thumbs img:hover {
      border-color: #a5b4fc;
    }

    .photo-thumbs img.active {
      border-color: #6366f1;
    }

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

  item = signal<Item | null>(null);
  loading = signal(false);
  selectedPhoto = signal<ItemPhoto | null>(null);

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
}
