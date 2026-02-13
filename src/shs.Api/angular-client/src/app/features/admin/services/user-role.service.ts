import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { UserRole, AssignRoleRequest, AssignBulkRolesRequest } from '../../../core/models/user-role.model';

@Injectable({
  providedIn: 'root'
})
export class UserRoleService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/users`;

  getUserRoles(userExternalId: string): Observable<UserRole[]> {
    return this.http.get<UserRole[]>(`${this.baseUrl}/${userExternalId}/roles`);
  }

  assignRole(userExternalId: string, request: AssignRoleRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/${userExternalId}/roles`, request);
  }

  assignBulkRoles(userExternalId: string, request: AssignBulkRolesRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/${userExternalId}/roles/bulk`, request);
  }

  revokeRole(userExternalId: string, roleExternalId: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.baseUrl}/${userExternalId}/roles/${roleExternalId}`);
  }
}
