import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { User, UserDetail } from '../../../core/models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/users`;

  getAll(search?: string): Observable<User[]> {
    const params: any = {};
    if (search) params.search = search;
    return this.http.get<User[]>(this.apiUrl, { params });
  }

  getById(externalId: string): Observable<UserDetail> {
    return this.http.get<UserDetail>(`${this.apiUrl}/${externalId}`);
  }
}
