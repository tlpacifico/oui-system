import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { SystemSettingGroup, UpdateSystemSettingRequest } from '../../../core/models/system-setting.model';

@Injectable({ providedIn: 'root' })
export class SystemSettingService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/system-settings`;

  getAll(): Observable<SystemSettingGroup[]> {
    return this.http.get<SystemSettingGroup[]>(this.apiUrl);
  }

  update(key: string, request: UpdateSystemSettingRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/${key}`, request);
  }
}
