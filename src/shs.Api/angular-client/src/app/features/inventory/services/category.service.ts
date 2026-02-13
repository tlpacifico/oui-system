import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Category, CategoryListItem, CreateCategoryRequest, UpdateCategoryRequest } from '../../../core/models/category.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/categories`;

  getAll(search?: string): Observable<CategoryListItem[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get<CategoryListItem[]>(this.apiUrl, { params });
  }

  getById(externalId: string): Observable<Category> {
    return this.http.get<Category>(`${this.apiUrl}/${externalId}`);
  }

  create(data: CreateCategoryRequest): Observable<Category> {
    return this.http.post<Category>(this.apiUrl, data);
  }

  update(externalId: string, data: UpdateCategoryRequest): Observable<Category> {
    return this.http.put<Category>(`${this.apiUrl}/${externalId}`, data);
  }

  delete(externalId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${externalId}`);
  }
}
