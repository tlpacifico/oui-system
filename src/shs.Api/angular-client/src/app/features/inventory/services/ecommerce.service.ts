import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

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

  getProducts(params?: { status?: string; search?: string; page?: number; pageSize?: number }): Observable<any> {
    return this.http.get(`${this.baseUrl}/products`, { params: params as any });
  }

  unpublishProduct(externalId: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/products/${externalId}`);
  }
}
