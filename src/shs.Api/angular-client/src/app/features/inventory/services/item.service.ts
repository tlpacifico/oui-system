import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Item, ItemListItem, PagedResult } from '../../../core/models/item.model';
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

  createConsignmentItem(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/consignment`, data);
  }
}
