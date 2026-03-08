import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface ApprovalDetails {
  supplierName: string;
  receptionDate: string;
  receptionRef: string;
  items: ApprovalItem[];
  totalValue: number;
  expiresAt: string;
}

export interface ApprovalItem {
  identificationNumber: string;
  name: string;
  brand: string;
  size: string;
  color: string;
  condition: string;
  evaluatedPrice: number;
  commissionPercentage: number;
}

@Injectable({ providedIn: 'root' })
export class ApprovalService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/consignments`;

  getApprovalDetails(token: string): Observable<ApprovalDetails> {
    return this.http.get<ApprovalDetails>(`${this.apiUrl}/approval/${token}`);
  }

  approve(token: string): Observable<{ message: string; itemsApproved: number }> {
    return this.http.post<{ message: string; itemsApproved: number }>(
      `${this.apiUrl}/approval/${token}/approve`, {}
    );
  }

  reject(token: string, message?: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(
      `${this.apiUrl}/approval/${token}/reject`, { message }
    );
  }

  staffApprove(receptionExternalId: string): Observable<{ message: string; itemsApproved: number }> {
    return this.http.put<{ message: string; itemsApproved: number }>(
      `${this.apiUrl}/receptions/${receptionExternalId}/approve`, {}
    );
  }
}
