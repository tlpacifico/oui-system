import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { Permission, PermissionsByCategory } from '../../../core/models/permission.model';

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
}
