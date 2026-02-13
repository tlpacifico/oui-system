import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Tag, TagListItem, CreateTagRequest, UpdateTagRequest } from '../../../core/models/tag.model';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TagService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/tags`;

  getAll(search?: string): Observable<TagListItem[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get<TagListItem[]>(this.apiUrl, { params });
  }

  getById(externalId: string): Observable<Tag> {
    return this.http.get<Tag>(`${this.apiUrl}/${externalId}`);
  }

  create(data: CreateTagRequest): Observable<Tag> {
    return this.http.post<Tag>(this.apiUrl, data);
  }

  update(externalId: string, data: UpdateTagRequest): Observable<Tag> {
    return this.http.put<Tag>(`${this.apiUrl}/${externalId}`, data);
  }

  delete(externalId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${externalId}`);
  }
}
