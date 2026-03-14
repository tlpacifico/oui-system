export enum EcommerceProductStatus {
  Draft = 'Draft',
  Published = 'Published',
  Reserved = 'Reserved',
  Sold = 'Sold',
  Unpublished = 'Unpublished'
}

export enum EcommerceOrderStatus {
  Pending = 'Pending',
  Confirmed = 'Confirmed',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export interface EcommerceProduct {
  externalId: string;
  slug: string;
  title: string;
  price: number;
  brandName: string;
  categoryName: string;
  size: string;
  color: string;
  status: EcommerceProductStatus;
  publishedAt?: string;
  unpublishedAt?: string;
  primaryPhotoUrl?: string;
}

export interface EcommerceProductPhoto {
  externalId: string;
  filePath: string;
  thumbnailPath?: string;
  displayOrder: number;
  isPrimary: boolean;
}

export interface EcommerceProductDetail extends EcommerceProduct {
  description?: string;
  condition?: string;
  composition?: string;
  photos: EcommerceProductPhoto[];
}

export interface EcommerceOrder {
  externalId: string;
  orderNumber: string;
  customerName: string;
  customerEmail: string;
  customerPhone?: string;
  status: EcommerceOrderStatus;
  totalAmount: number;
  reservedAt: string;
  expiresAt?: string;
  confirmedAt?: string;
  itemCount: number;
}

export interface EcommerceOrderItem {
  externalId: string;
  productTitle: string;
  price: number;
}

export interface EcommerceOrderDetail extends EcommerceOrder {
  notes?: string;
  completedAt?: string;
  cancelledAt?: string;
  cancellationReason?: string;
  items: EcommerceOrderItem[];
}

export interface UpdateProductRequest {
  title?: string;
  description?: string;
  price?: number;
  brandName?: string;
  categoryName?: string;
  size?: string;
  color?: string;
  condition?: string;
  composition?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
