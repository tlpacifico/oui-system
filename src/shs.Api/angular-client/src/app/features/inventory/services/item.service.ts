import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Item, ItemListItem, PagedResult, CreateItemRequest, UpdateItemRequest } from '../../../core/models/item.model';
import { environment } from '../../../../environments/environment';

export interface ItemFilters {
  search?: string;
  brandId?: number;
  status?: string;
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class ItemService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/inventory/items`;

  getItems(filters: ItemFilters = {}): Observable<PagedResult<ItemListItem>> {
    let params = new HttpParams();

    if (filters.search) params = params.set('search', filters.search);
    if (filters.brandId) params = params.set('brandId', filters.brandId.toString());
    if (filters.status) params = params.set('status', filters.status);
    params = params.set('page', (filters.page || 1).toString());
    params = params.set('pageSize', (filters.pageSize || 20).toString());

    return this.http.get<PagedResult<ItemListItem>>(this.apiUrl, { params });
  }

  getItemById(externalId: string): Observable<Item> {
    return this.http.get<Item>(`${this.apiUrl}/${externalId}`);
  }

  createItem(data: CreateItemRequest): Observable<any> {
    return this.http.post(this.apiUrl, data);
  }

  createConsignmentItem(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/consignment`, data);
  }

  updateItem(externalId: string, data: UpdateItemRequest): Observable<Item> {
    return this.http.put<Item>(`${this.apiUrl}/${externalId}`, data);
  }

  deleteItem(externalId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${externalId}`);
  }

  // ── Photo methods ──

  uploadPhotos(itemExternalId: string, files: File[]): Observable<any[]> {
    const formData = new FormData();
    files.forEach(f => formData.append('files', f));
    return this.http.post<any[]>(`${this.apiUrl}/${itemExternalId}/photos`, formData);
  }

  deletePhoto(itemExternalId: string, photoExternalId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${itemExternalId}/photos/${photoExternalId}`);
  }

  reorderPhotos(itemExternalId: string, photoExternalIds: string[]): Observable<any[]> {
    return this.http.put<any[]>(`${this.apiUrl}/${itemExternalId}/photos/reorder`, {
      photoExternalIds
    });
  }
}
