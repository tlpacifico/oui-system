import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ReceptionDetail,
  ReceptionPagedResult,
  CreateReceptionRequest,
  AddEvaluationItemRequest,
  EvaluationItemResponse,
} from '../../../core/models/reception.model';
import { environment } from '../../../../environments/environment';

export interface ReceptionFilters {
  status?: string;
  supplierExternalId?: string;
  search?: string;
  page?: number;
  pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class ReceptionService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/consignments/receptions`;

  getReceptions(filters: ReceptionFilters = {}): Observable<ReceptionPagedResult> {
    let params = new HttpParams();

    if (filters.status) params = params.set('status', filters.status);
    if (filters.supplierExternalId) params = params.set('supplierExternalId', filters.supplierExternalId);
    if (filters.search) params = params.set('search', filters.search);
    params = params.set('page', (filters.page || 1).toString());
    params = params.set('pageSize', (filters.pageSize || 20).toString());

    return this.http.get<ReceptionPagedResult>(this.apiUrl, { params });
  }

  getById(externalId: string): Observable<ReceptionDetail> {
    return this.http.get<ReceptionDetail>(`${this.apiUrl}/${externalId}`);
  }

  create(data: CreateReceptionRequest): Observable<ReceptionDetail> {
    return this.http.post<ReceptionDetail>(this.apiUrl, data);
  }

  getReceiptUrl(externalId: string): string {
    return `${this.apiUrl}/${externalId}/receipt`;
  }

  getReceiptHtml(externalId: string): Observable<string> {
    return this.http.get(`${this.apiUrl}/${externalId}/receipt`, { responseType: 'text' });
  }

  // ── Evaluation methods ──

  getReceptionItems(externalId: string): Observable<EvaluationItemResponse[]> {
    return this.http.get<EvaluationItemResponse[]>(`${this.apiUrl}/${externalId}/items`);
  }

  addEvaluationItem(receptionExternalId: string, data: AddEvaluationItemRequest): Observable<EvaluationItemResponse> {
    return this.http.post<EvaluationItemResponse>(`${this.apiUrl}/${receptionExternalId}/items`, data);
  }

  removeEvaluationItem(receptionExternalId: string, itemExternalId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${receptionExternalId}/items/${itemExternalId}`);
  }

  completeEvaluation(receptionExternalId: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${receptionExternalId}/complete-evaluation`, {});
  }

  // ── Email methods (CU-10) ──

  sendEvaluationEmail(receptionExternalId: string): Observable<{ message: string; sentTo: string }> {
    return this.http.post<{ message: string; sentTo: string }>(
      `${this.apiUrl}/${receptionExternalId}/send-evaluation-email`, {}
    );
  }
}
