import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Role, RoleDetail, CreateRoleRequest, UpdateRoleRequest } from '../../../core/models/role.model';

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/roles`;

  getAll(search?: string): Observable<Role[]> {
    const params: any = {};
    if (search) params.search = search;
    return this.http.get<Role[]>(this.apiUrl, { params });
  }

  getById(externalId: string): Observable<RoleDetail> {
    return this.http.get<RoleDetail>(`${this.apiUrl}/${externalId}`);
  }

  create(request: CreateRoleRequest): Observable<{ externalId: string }> {
    return this.http.post<{ externalId: string }>(this.apiUrl, request);
  }

  update(externalId: string, request: UpdateRoleRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.apiUrl}/${externalId}`, request);
  }

  delete(externalId: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/${externalId}`);
  }
}
