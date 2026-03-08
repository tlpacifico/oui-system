export interface Product {
  slug: string
  title: string
  price: number
  brandName: string
  categoryName?: string
  size?: string
  color?: string
  condition: string
  primaryPhotoUrl?: string
}

export interface ProductDetail extends Product {
  description?: string
  composition?: string
  photos: ProductPhoto[]
}

export interface ProductPhoto {
  url: string
  thumbnailUrl?: string
  isPrimary: boolean
}

export interface ProductsResponse {
  items: Product[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface OrderItem {
  productTitle: string
  price: number
}

export interface OrderResponse {
  externalId: string
  orderNumber: string
  customerName: string
  totalAmount: number
  reservedAt: string
  expiresAt: string
  items: OrderItem[]
  unavailableProducts: string[]
}

export interface OrderStatus {
  orderNumber: string
  status: string
  customerName: string
  totalAmount: number
  reservedAt: string
  expiresAt: string
  confirmedAt?: string
  completedAt?: string
  cancelledAt?: string
  cancellationReason?: string
  items: OrderItem[]
}
