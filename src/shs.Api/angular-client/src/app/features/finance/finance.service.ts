import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

// ── Pending Items (grouped by supplier) ──

export interface PendingSettlementItem {
  itemId: number;
  externalId: string;
  identificationNumber: string;
  name: string;
  brandName: string;
  evaluatedPrice: number;
  finalSalePrice: number;
  commissionPercentage: number | null;
  commissionAmount: number | null;
  supplierId: number;
  supplierName: string;
  supplierInitial: string;
  updatedOn: Date;
  isSettled: boolean;
}

export interface PendingSettlementGroup {
  supplierId: number;
  supplierName: string;
  supplierInitial: string;
  itemCount: number;
  totalSalesAmount: number;
  items: PendingSettlementItem[];
}

// ── Calculate Settlement ──

export interface CalculateSettlementRequest {
  supplierId: number;
  periodStart: string; // ISO date
  periodEnd: string;
}

export interface CalculateSettlementResponse {
  supplierId: number;
  supplierName: string;
  periodStart: string;
  periodEnd: string;
  itemCount: number;
  totalSalesAmount: number;
  creditPercentageInStore: number;
  cashRedemptionPercentage: number;
  storeCreditAmount: number;
  cashRedemptionAmount: number;
  netAmountToSupplier: number;
  storeCommissionAmount: number;
}

// ── Create Settlement ──

export interface CreateSettlementRequest {
  supplierId: number;
  periodStart: string;
  periodEnd: string;
  notes?: string;
}

export interface CreateSettlementResponse {
  externalId: string;
  supplierId: number;
  supplierName: string;
  periodStart: string;
  periodEnd: string;
  totalSalesAmount: number;
  creditPercentageInStore: number;
  cashRedemptionPercentage: number;
  storeCreditAmount: number;
  cashRedemptionAmount: number;
  storeCommissionAmount: number;
  netAmountToSupplier: number;
  status: number;
  itemCount: number;
  notes: string | null;
  createdOn: string;
  createdBy: string;
}

// ── Settlement List ──

export type SettlementStatus = 1 | 2 | 3; // Pending=1, Paid=2, Cancelled=3

export interface SettlementListItem {
  externalId: string;
  supplierId: number;
  supplierName: string;
  supplierInitial: string;
  periodStart: string;
  periodEnd: string;
  totalSalesAmount: number;
  creditPercentageInStore: number;
  cashRedemptionPercentage: number;
  storeCreditAmount: number;
  cashRedemptionAmount: number;
  storeCommissionAmount: number;
  netAmountToSupplier: number;
  status: SettlementStatus;
  itemCount: number;
  paidOn: string | null;
  paidBy: string | null;
  createdOn: string;
  createdBy: string;
}

export interface SettlementsPagedResult {
  total: number;
  page: number;
  pageSize: number;
  data: SettlementListItem[];
}

// ── Settlement Detail ──

export interface SettlementItemDetail {
  externalId: string;
  identificationNumber: string;
  name: string;
  brandName: string;
  evaluatedPrice: number;
  finalPrice: number;
  saleDate: string;
}

export interface SettlementDetail {
  externalId: string;
  supplierId: number;
  supplierName: string;
  supplierEmail: string;
  supplierPhone: string;
  periodStart: string;
  periodEnd: string;
  totalSalesAmount: number;
  creditPercentageInStore: number;
  cashRedemptionPercentage: number;
  storeCreditAmount: number;
  cashRedemptionAmount: number;
  storeCommissionAmount: number;
  netAmountToSupplier: number;
  status: SettlementStatus;
  paidOn: string | null;
  paidBy: string | null;
  notes: string | null;
  createdOn: string;
  createdBy: string;
  storeCredit: {
    externalId: string;
    originalAmount: number;
    currentBalance: number;
    status: number;
    issuedOn: string;
  } | null;
  items: SettlementItemDetail[];
}

// ── Process Payment ──

export interface ProcessPaymentResponse {
  externalId: string;
  status: number;
  paidOn: string;
  paidBy: string;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class FinanceService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  // ── Pending Items ──

  getPendingSettlementItems(
    supplierId?: number,
    startDate?: string,
    endDate?: string
  ): Observable<PendingSettlementGroup[]> {
    let params = new HttpParams();
    if (supplierId != null) params = params.set('supplierId', supplierId.toString());
    if (startDate) params = params.set('startDate', startDate);
    if (endDate) params = params.set('endDate', endDate);
    return this.http.get<PendingSettlementGroup[]>(
      `${this.baseUrl}/settlements/pending-items`,
      { params }
    );
  }

  // ── Calculate (preview) ──

  calculateSettlement(req: CalculateSettlementRequest): Observable<CalculateSettlementResponse> {
    return this.http.post<CalculateSettlementResponse>(
      `${this.baseUrl}/settlements/calculate`,
      req
    );
  }

  // ── Create ──

  createSettlement(req: CreateSettlementRequest): Observable<CreateSettlementResponse> {
    return this.http.post<CreateSettlementResponse>(
      `${this.baseUrl}/settlements`,
      req
    );
  }

  // ── List ──

  getSettlements(
    supplierId?: number,
    status?: SettlementStatus,
    page = 1,
    pageSize = 20
  ): Observable<SettlementsPagedResult> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    if (supplierId != null) params = params.set('supplierId', supplierId.toString());
    if (status != null) params = params.set('status', status.toString());
    return this.http.get<SettlementsPagedResult>(`${this.baseUrl}/settlements`, { params });
  }

  // ── Detail ──

  getSettlementById(externalId: string): Observable<SettlementDetail> {
    return this.http.get<SettlementDetail>(`${this.baseUrl}/settlements/${externalId}`);
  }

  // ── Process Payment ──

  processPayment(externalId: string): Observable<ProcessPaymentResponse> {
    return this.http.post<ProcessPaymentResponse>(
      `${this.baseUrl}/settlements/${externalId}/process-payment`,
      {}
    );
  }

  // ── Cancel ──

  cancelSettlement(externalId: string): Observable<{ externalId: string; status: number; message: string }> {
    return this.http.delete<{ externalId: string; status: number; message: string }>(
      `${this.baseUrl}/settlements/${externalId}`
    );
  }

  // ── Store Credits ──

  getSupplierStoreCredits(
    supplierId: number,
    status?: number
  ): Observable<SupplierStoreCreditsResponse> {
    let params = new HttpParams();
    if (status != null) params = params.set('status', status.toString());
    return this.http.get<SupplierStoreCreditsResponse>(
      `${this.baseUrl}/store-credits/supplier/${supplierId}`,
      { params }
    );
  }

  getStoreCreditById(externalId: string): Observable<StoreCreditDetail> {
    return this.http.get<StoreCreditDetail>(
      `${this.baseUrl}/store-credits/${externalId}`
    );
  }

  getStoreCreditTransactions(externalId: string): Observable<StoreCreditTransactionsResponse> {
    return this.http.get<StoreCreditTransactionsResponse>(
      `${this.baseUrl}/store-credits/${externalId}/transactions`
    );
  }

  issueStoreCredit(req: IssueStoreCreditRequest): Observable<StoreCreditIssuedResponse> {
    return this.http.post<StoreCreditIssuedResponse>(
      `${this.baseUrl}/store-credits`,
      req
    );
  }

  adjustStoreCredit(externalId: string, req: AdjustStoreCreditRequest): Observable<StoreCreditAdjustedResponse> {
    return this.http.post<StoreCreditAdjustedResponse>(
      `${this.baseUrl}/store-credits/${externalId}/adjust`,
      req
    );
  }

  cancelStoreCredit(externalId: string, reason?: string): Observable<StoreCreditCancelledResponse> {
    let params = new HttpParams();
    if (reason) params = params.set('reason', reason);
    return this.http.delete<StoreCreditCancelledResponse>(
      `${this.baseUrl}/store-credits/${externalId}`,
      { params }
    );
  }

  // ── Cash Redemptions ──

  getSupplierCashBalance(supplierId: number): Observable<SupplierCashBalanceResponse> {
    return this.http.get<SupplierCashBalanceResponse>(
      `${this.baseUrl}/cash-redemptions/supplier/${supplierId}/balance`
    );
  }

  getSupplierCashHistory(
    supplierId: number,
    page = 1,
    pageSize = 20
  ): Observable<SupplierCashHistoryResponse> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<SupplierCashHistoryResponse>(
      `${this.baseUrl}/cash-redemptions/supplier/${supplierId}/history`,
      { params }
    );
  }

  processCashRedemption(req: ProcessCashRedemptionRequest): Observable<CashRedemptionProcessedResponse> {
    return this.http.post<CashRedemptionProcessedResponse>(
      `${this.baseUrl}/cash-redemptions`,
      req
    );
  }
}

// ── Store Credit types ──

export interface StoreCreditItem {
  externalId: string;
  originalAmount: number;
  currentBalance: number;
  status: number;
  issuedOn: string;
  issuedBy: string;
  expiresOn: string | null;
  notes: string | null;
  sourceSettlement: { externalId: string; periodStart: string; periodEnd: string } | null;
}

export interface SupplierStoreCreditsResponse {
  supplierId: number;
  totalActiveBalance: number;
  credits: StoreCreditItem[];
}

export interface StoreCreditDetail {
  externalId: string;
  supplierId: number;
  supplierName: string;
  originalAmount: number;
  currentBalance: number;
  status: number;
  issuedOn: string;
  issuedBy: string;
  expiresOn: string | null;
  notes: string | null;
  sourceSettlement: { externalId: string; totalSalesAmount: number; periodStart: string; periodEnd: string } | null;
  transactionCount: number;
  lastTransaction: { transactionDate: string; amount: number; transactionType: number } | null;
}

export interface StoreCreditTransactionsResponse {
  storeCreditId: string;
  currentBalance: number;
  transactions: StoreCreditTransaction[];
}

export interface StoreCreditTransaction {
  externalId: string;
  amount: number;
  balanceAfter: number;
  transactionType: number;
  transactionDate: string;
  processedBy: string;
  notes: string | null;
  sale: { externalId: string; saleNumber: string; totalAmount: number } | null;
}

export interface IssueStoreCreditRequest {
  supplierId: number;
  amount: number;
  expiresOn?: string;
  notes?: string;
}

export interface StoreCreditIssuedResponse {
  externalId: string;
  supplierId: number;
  supplierName: string;
  originalAmount: number;
  currentBalance: number;
  status: number;
  issuedOn: string;
  issuedBy: string;
}

export interface AdjustStoreCreditRequest {
  adjustmentAmount: number;
  reason?: string;
}

export interface StoreCreditAdjustedResponse {
  externalId: string;
  oldBalance: number;
  adjustmentAmount: number;
  newBalance: number;
  status: number;
  reason: string | null;
}

export interface StoreCreditCancelledResponse {
  externalId: string;
  status: number;
  cancelledBalance: number;
  message: string;
}

// ── Cash Redemption types ──

export interface SupplierCashBalanceResponse {
  supplierId: number;
  supplierName: string;
  availableBalance: number;
  creditPercentageInStore: number;
  cashRedemptionPercentage: number;
}

export interface CashTransaction {
  externalId: string;
  amount: number;
  transactionType: number;
  transactionDate: string;
  processedBy: string;
  notes: string | null;
  settlementPeriod: string | null;
}

export interface SupplierCashHistoryResponse {
  supplierId: number;
  supplierName: string;
  currentBalance: number;
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
  transactions: CashTransaction[];
}

export interface ProcessCashRedemptionRequest {
  supplierId: number;
  amount: number;
  notes?: string;
}

export interface CashRedemptionProcessedResponse {
  externalId: string;
  supplierId: number;
  supplierName: string;
  amountRedeemed: number;
  previousBalance: number;
  newBalance: number;
  transactionDate: string;
  processedBy: string;
  message: string;
}
