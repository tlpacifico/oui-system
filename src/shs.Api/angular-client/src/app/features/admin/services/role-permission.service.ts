import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class RolePermissionService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/roles`;

  assignPermission(roleExternalId: string, permissionExternalId: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(
      `${this.baseUrl}/${roleExternalId}/permissions`,
      { permissionExternalId }
    );
  }

  assignBulkPermissions(roleExternalId: string, permissionExternalIds: string[]): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(
      `${this.baseUrl}/${roleExternalId}/permissions/bulk`,
      { permissionExternalIds }
    );
  }

  revokePermission(roleExternalId: string, permissionExternalId: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(
      `${this.baseUrl}/${roleExternalId}/permissions/${permissionExternalId}`
    );
  }
}
