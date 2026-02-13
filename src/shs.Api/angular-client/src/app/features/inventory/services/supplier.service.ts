import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Supplier, SupplierListItem, CreateSupplierRequest, UpdateSupplierRequest, SupplierItemListItem, SupplierReception } from '../../../core/models/supplier.model';
import { PagedResult } from '../../../core/models/item.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SupplierService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/suppliers`;

  getAll(search?: string): Observable<SupplierListItem[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get<SupplierListItem[]>(this.apiUrl, { params });
  }

  getById(externalId: string): Observable<Supplier> {
    return this.http.get<Supplier>(`${this.apiUrl}/${externalId}`);
  }

  getItems(externalId: string, page = 1, pageSize = 20, status?: string): Observable<PagedResult<SupplierItemListItem>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    if (status) params = params.set('status', status);
    return this.http.get<PagedResult<SupplierItemListItem>>(`${this.apiUrl}/${externalId}/items`, { params });
  }

  getReceptions(externalId: string): Observable<SupplierReception[]> {
    return this.http.get<SupplierReception[]>(`${this.apiUrl}/${externalId}/receptions`);
  }

  create(data: CreateSupplierRequest): Observable<Supplier> {
    return this.http.post<Supplier>(this.apiUrl, data);
  }

  update(externalId: string, data: UpdateSupplierRequest): Observable<Supplier> {
    return this.http.put<Supplier>(`${this.apiUrl}/${externalId}`, data);
  }

  delete(externalId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${externalId}`);
  }
}
