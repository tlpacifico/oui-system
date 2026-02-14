import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// ── Register types ──

export interface RegisterResponse {
  externalId: string;
  registerNumber: number;
  operatorName: string;
  openedAt: Date;
  closedAt?: Date;
  openingAmount: number;
  closingAmount?: number;
  expectedAmount?: number;
  discrepancy?: number;
  status: string;
  salesCount: number;
  salesTotal: number;
}

export interface CurrentRegisterResponse {
  open: boolean;
  register?: RegisterResponse;
}

export interface CloseRegisterResponse {
  externalId: string;
  registerNumber: number;
  operatorName: string;
  openedAt: Date;
  closedAt: Date;
  salesCount: number;
  totalRevenue: number;
  salesByPaymentMethod: Record<string, number>;
  expectedCash: number;
  countedCash: number;
  discrepancy: number;
}

// ── Sale types ──

export interface ProcessSaleRequest {
  cashRegisterId: string;
  items: { itemExternalId: string; discountAmount: number }[];
  payments: { method: string; amount: number; reference?: string; supplierId?: number }[];
  discountPercentage?: number;
  discountReason?: string;
  notes?: string;
}

export interface ProcessSaleResponse {
  externalId: string;
  saleNumber: string;
  saleDate: Date;
  subtotal: number;
  discountPercentage: number;
  discountAmount: number;
  totalAmount: number;
  change: number;
  itemCount: number;
  cashierName: string;
}

export interface SaleDetail {
  externalId: string;
  saleNumber: string;
  saleDate: Date;
  subtotal: number;
  discountPercentage: number;
  discountAmount: number;
  totalAmount: number;
  discountReason?: string;
  status: string;
  notes?: string;
  cashierName: string;
  registerNumber: number;
  items: SaleItemDetail[];
  payments: SalePaymentDetail[];
  createdOn: Date;
}

export interface SaleItemDetail {
  itemExternalId: string;
  identificationNumber: string;
  name: string;
  brand: string;
  size: string;
  color: string;
  supplierName?: string;
  unitPrice: number;
  discountAmount: number;
  finalPrice: number;
}

export interface SalePaymentDetail {
  method: string;
  amount: number;
  reference?: string;
}

export interface TodaySalesResponse {
  salesCount: number;
  totalRevenue: number;
  averageTicket: number;
  totalItems: number;
  byPaymentMethod: Record<string, { count: number; total: number }>;
  recentSales: SaleListItem[];
}

export interface SaleListItem {
  externalId: string;
  saleNumber: string;
  saleDate: Date;
  totalAmount: number;
  itemCount: number;
  status: string;
  paymentMethods: string;
}

export interface SalesPagedResult {
  data: SaleListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class PosService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  // ── Register ──

  openRegister(openingAmount: number): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.baseUrl}/pos/register/open`, { openingAmount });
  }

  closeRegister(registerExternalId: string, closingAmount: number, notes?: string): Observable<CloseRegisterResponse> {
    return this.http.post<CloseRegisterResponse>(`${this.baseUrl}/pos/register/close`, {
      registerExternalId, closingAmount, notes
    });
  }

  getCurrentRegister(): Observable<CurrentRegisterResponse> {
    return this.http.get<CurrentRegisterResponse>(`${this.baseUrl}/pos/register/current`);
  }

  // ── Sales ──

  processSale(req: ProcessSaleRequest): Observable<ProcessSaleResponse> {
    return this.http.post<ProcessSaleResponse>(`${this.baseUrl}/pos/sales`, req);
  }

  getSaleById(externalId: string): Observable<SaleDetail> {
    return this.http.get<SaleDetail>(`${this.baseUrl}/pos/sales/${externalId}`);
  }

  getTodaySales(): Observable<TodaySalesResponse> {
    return this.http.get<TodaySalesResponse>(`${this.baseUrl}/pos/sales/today`);
  }

  searchSales(page = 1, pageSize = 20, dateFrom?: string, dateTo?: string, search?: string): Observable<SalesPagedResult> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    if (dateFrom) params = params.set('dateFrom', dateFrom);
    if (dateTo) params = params.set('dateTo', dateTo);
    if (search) params = params.set('search', search);
    return this.http.get<SalesPagedResult>(`${this.baseUrl}/pos/sales`, { params });
  }
}
