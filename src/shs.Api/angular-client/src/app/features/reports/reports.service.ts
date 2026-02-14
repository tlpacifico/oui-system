import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SalesReport {
  revenue: number;
  salesCount: number;
  avgTicket: number;
  topBrands: { brandName: string; revenue: number; count: number }[];
  topCategories: { categoryName: string; revenue: number; count: number }[];
  paymentBreakdown: Record<string, { count: number; total: number }>;
  dailySalesChart: { date: string; revenue: number; count: number }[];
  previousPeriodComparison: { percentChange: number; previousRevenue: number };
}

export interface InventoryReport {
  totalItems: number;
  totalValue: number;
  agingDistribution: {
    days0_15: number;
    days15_30: number;
    days30_45: number;
    days45_60: number;
    days60Plus: number;
  };
  sellThroughRate: number;
  sellThroughByBrand: { brandName: string; inStock: number; sold: number; sellThroughRate: number }[];
  stagnantItemsList: { id: number; externalId: string; brandName: string; categoryName: string | null; evaluatedPrice: number; daysInStock: number }[];
}

export interface SuppliersReport {
  period: { start: string; end: string };
  ranking: {
    id: number;
    externalId: string;
    name: string;
    initial: string;
    revenue: number;
    soldCount: number;
    returnedCount: number;
    returnRate: number;
    pendingAmount: number;
    avgDaysToSell: number;
  }[];
}

export interface FinanceReport {
  period: { start: string; end: string };
  grossRevenue: number;
  commissionRevenue: number;
  pendingSettlements: number;
  paidSettlements: number;
  projectedCashflow: number;
}

@Injectable({ providedIn: 'root' })
export class ReportsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getSalesReport(startDate?: string, endDate?: string, brandId?: number, categoryId?: number): Observable<SalesReport> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    if (brandId != null) params = params.set('brandId', brandId.toString());
    if (categoryId != null) params = params.set('categoryId', categoryId.toString());
    return this.http.get<SalesReport>(`${this.baseUrl}/reports/sales`, { params });
  }

  getInventoryReport(): Observable<InventoryReport> {
    return this.http.get<InventoryReport>(`${this.baseUrl}/reports/inventory`);
  }

  getSuppliersReport(startDate?: string, endDate?: string): Observable<SuppliersReport> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    return this.http.get<SuppliersReport>(`${this.baseUrl}/reports/suppliers`, { params });
  }

  getFinanceReport(startDate?: string, endDate?: string): Observable<FinanceReport> {
    let params = new HttpParams();
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    return this.http.get<FinanceReport>(`${this.baseUrl}/reports/finance`, { params });
  }
}
