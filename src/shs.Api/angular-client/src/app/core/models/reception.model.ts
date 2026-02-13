export interface ReceptionSupplierInfo {
  externalId: string;
  name: string;
  initial: string;
}

export interface ReceptionListItem {
  externalId: string;
  supplier: ReceptionSupplierInfo;
  receptionDate: Date;
  itemCount: number;
  status: ReceptionStatus;
  evaluatedCount: number;
  acceptedCount: number;
  rejectedCount: number;
  notes?: string;
  createdOn: Date;
}

export interface ReceptionDetail {
  externalId: string;
  supplier: ReceptionSupplierInfo;
  receptionDate: Date;
  itemCount: number;
  status: ReceptionStatus;
  notes?: string;
  evaluatedCount: number;
  acceptedCount: number;
  rejectedCount: number;
  evaluatedAt?: Date;
  evaluatedBy?: string;
  createdOn: Date;
  createdBy?: string;
}

export interface CreateReceptionRequest {
  supplierExternalId: string;
  itemCount: number;
  notes?: string;
}

export interface ReceptionPagedResult {
  data: ReceptionListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export enum ReceptionStatus {
  PendingEvaluation = 'PendingEvaluation',
  Evaluated = 'Evaluated',
  ConsignmentCreated = 'ConsignmentCreated'
}

// ── Evaluation models ──

export interface AddEvaluationItemRequest {
  name: string;
  description?: string;
  brandExternalId: string;
  categoryExternalId?: string;
  size: string;
  color: string;
  composition?: string;
  condition: string;
  evaluatedPrice: number;
  commissionPercentage?: number;
  isRejected: boolean;
  rejectionReason?: string;
  tagExternalIds?: string[];
}

export interface EvaluationItemResponse {
  externalId: string;
  identificationNumber: string;
  name: string;
  brand: string;
  size: string;
  color: string;
  condition: string;
  evaluatedPrice: number;
  commissionPercentage: number;
  status: string;
  isRejected: boolean;
  rejectionReason?: string;
  createdOn: Date;
}
