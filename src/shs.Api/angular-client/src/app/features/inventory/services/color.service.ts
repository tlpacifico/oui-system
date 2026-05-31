import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Color, ColorListItem, CreateColorRequest, UpdateColorRequest } from '../../../core/models/color.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ColorService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/colors`;

  getAll(search?: string): Observable<ColorListItem[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get<ColorListItem[]>(this.apiUrl, { params });
  }

  getById(externalId: string): Observable<Color> {
    return this.http.get<Color>(`${this.apiUrl}/${externalId}`);
  }

  create(data: CreateColorRequest): Observable<Color> {
    return this.http.post<Color>(this.apiUrl, data);
  }

  update(externalId: string, data: UpdateColorRequest): Observable<Color> {
    return this.http.put<Color>(`${this.apiUrl}/${externalId}`, data);
  }

  delete(externalId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${externalId}`);
  }
}
