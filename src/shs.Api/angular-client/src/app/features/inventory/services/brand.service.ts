import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Brand, BrandListItem, CreateBrandRequest, UpdateBrandRequest } from '../../../core/models/brand.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class BrandService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/brands`;

  getAll(search?: string): Observable<BrandListItem[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get<BrandListItem[]>(this.apiUrl, { params });
  }

  getById(externalId: string): Observable<Brand> {
    return this.http.get<Brand>(`${this.apiUrl}/${externalId}`);
  }

  create(data: CreateBrandRequest): Observable<Brand> {
    return this.http.post<Brand>(this.apiUrl, data);
  }

  update(externalId: string, data: UpdateBrandRequest): Observable<Brand> {
    return this.http.put<Brand>(`${this.apiUrl}/${externalId}`, data);
  }

  delete(externalId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${externalId}`);
  }
}
