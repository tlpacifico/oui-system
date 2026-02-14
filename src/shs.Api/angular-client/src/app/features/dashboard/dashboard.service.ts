import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DashboardData {
  salesToday: { count: number; revenue: number; averageTicket: number };
  salesMonth: { count: number; revenue: number; averageTicket: number; growthPercent: number };
  inventory: { totalItems: number; totalValue: number; stagnantCount: number };
  pendingSettlements: { totalAmount: number; suppliersCount: number };
  topSellingItems: { name: string; brand: string; finalPrice: number; soldDate: string }[];
  alerts: {
    expiringConsignments: number;
    stagnantItems30: number;
    stagnantItems45: number;
    stagnantItems60: number;
    openRegisters: { operatorName: string; openedAt: string; salesCount: number }[];
  };
  salesChart: { date: string; revenue: number; count: number }[];
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getDashboard(period: 'today' | 'week' | 'month' = 'today'): Observable<DashboardData> {
    const params = new HttpParams().set('period', period);
    return this.http.get<DashboardData>(`${this.baseUrl}/dashboard`, { params });
  }
}
