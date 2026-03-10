import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { Permission, PermissionsByCategory, CreatePermissionRequest, UpdatePermissionRequest } from '../../../core/models/permission.model';

@Injectable({
  providedIn: 'root'
})
export class PermissionService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/permissions`;

  getAll(category?: string, search?: string): Observable<Permission[]> {
    const params: any = {};
    if (category) params.category = category;
    if (search) params.search = search;
    return this.http.get<Permission[]>(this.apiUrl, { params });
  }

  getCategories(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/categories`);
  }

  getAllGroupedByCategory(): Observable<PermissionsByCategory> {
    return this.getAll().pipe(
      map(permissions => {
        const grouped: PermissionsByCategory = {};
        permissions.forEach(permission => {
          if (!grouped[permission.category]) {
            grouped[permission.category] = [];
          }
          grouped[permission.category].push(permission);
        });
        return grouped;
      })
    );
  }

  create(request: CreatePermissionRequest): Observable<{ externalId: string }> {
    return this.http.post<{ externalId: string }>(this.apiUrl, request);
  }

  update(externalId: string, request: UpdatePermissionRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.apiUrl}/${externalId}`, request);
  }

  delete(externalId: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/${externalId}`);
  }
}
