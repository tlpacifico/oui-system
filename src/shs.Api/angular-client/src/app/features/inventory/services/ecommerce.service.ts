import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  EcommerceProductDetail,
  EcommerceOrderDetail,
  UpdateProductRequest,
  PaginatedResponse,
  EcommerceProduct,
  EcommerceOrder
} from '../../../core/models/ecommerce.model';

@Injectable({ providedIn: 'root' })
export class EcommerceService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/ecommerce/admin`;

  publishItem(itemExternalId: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/publish`, { itemExternalId });
  }

  publishBatch(itemExternalIds: string[]): Observable<any> {
    return this.http.post(`${this.baseUrl}/publish-batch`, { itemExternalIds });
  }

  getProducts(params?: { status?: string; search?: string; page?: number; pageSize?: number }): Observable<PaginatedResponse<EcommerceProduct>> {
    return this.http.get<PaginatedResponse<EcommerceProduct>>(`${this.baseUrl}/products`, { params: params as any });
  }

  getProductById(externalId: string): Observable<EcommerceProductDetail> {
    return this.http.get<EcommerceProductDetail>(`${this.baseUrl}/products/${externalId}`);
  }

  updateProduct(externalId: string, request: UpdateProductRequest): Observable<any> {
    return this.http.put(`${this.baseUrl}/products/${externalId}`, request);
  }

  uploadPhotos(productExternalId: string, files: File[]): Observable<any[]> {
    const formData = new FormData();
    files.forEach(f => formData.append('files', f));
    return this.http.post<any[]>(`${this.baseUrl}/products/${productExternalId}/photos`, formData);
  }

  deletePhoto(productExternalId: string, photoExternalId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/products/${productExternalId}/photos/${photoExternalId}`);
  }

  unpublishProduct(externalId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/products/${externalId}`);
  }

  getOrders(params?: { status?: string; page?: number; pageSize?: number }): Observable<PaginatedResponse<EcommerceOrder>> {
    return this.http.get<PaginatedResponse<EcommerceOrder>>(`${this.baseUrl}/orders`, { params: params as any });
  }

  getOrderById(externalId: string): Observable<EcommerceOrderDetail> {
    return this.http.get<EcommerceOrderDetail>(`${this.baseUrl}/orders/${externalId}`);
  }

  confirmOrder(externalId: string): Observable<any> {
    return this.http.put(`${this.baseUrl}/orders/${externalId}/confirm`, {});
  }

  cancelOrder(externalId: string, reason?: string): Observable<any> {
    return this.http.put(`${this.baseUrl}/orders/${externalId}/cancel`, { reason });
  }
}
