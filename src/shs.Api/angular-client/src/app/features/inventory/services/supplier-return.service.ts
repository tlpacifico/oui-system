import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ReturnableItem,
  SupplierReturnDetail,
  SupplierReturnPagedResult,
  CreateSupplierReturnRequest,
} from '../../../core/models/supplier-return.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SupplierReturnService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/consignments/returns`;

  getReturnableItems(supplierExternalId: string): Observable<ReturnableItem[]> {
    const params = new HttpParams().set('supplierExternalId', supplierExternalId);
    return this.http.get<ReturnableItem[]>(`${this.apiUrl}/returnable-items`, { params });
  }

  create(req: CreateSupplierReturnRequest): Observable<SupplierReturnDetail> {
    return this.http.post<SupplierReturnDetail>(this.apiUrl, req);
  }

  getAll(page = 1, pageSize = 20, supplierExternalId?: string, search?: string): Observable<SupplierReturnPagedResult> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    if (supplierExternalId) params = params.set('supplierExternalId', supplierExternalId);
    if (search) params = params.set('search', search);
    return this.http.get<SupplierReturnPagedResult>(this.apiUrl, { params });
  }

  getById(externalId: string): Observable<SupplierReturnDetail> {
    return this.http.get<SupplierReturnDetail>(`${this.apiUrl}/${externalId}`);
  }
}
