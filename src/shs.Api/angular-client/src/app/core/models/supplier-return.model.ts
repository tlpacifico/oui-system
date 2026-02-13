export interface ReturnSupplierInfo {
  externalId: string;
  name: string;
  initial: string;
}

export interface ReturnableItem {
  externalId: string;
  identificationNumber: string;
  name: string;
  brand: string;
  size: string;
  color: string;
  condition: string;
  evaluatedPrice: number;
  status: string;
  isRejected: boolean;
  daysInStock: number;
  primaryPhotoUrl?: string;
  createdOn: Date;
}

export interface ReturnItem {
  externalId: string;
  identificationNumber: string;
  name: string;
  brand: string;
  size: string;
  color: string;
  condition: string;
  evaluatedPrice: number;
  isRejected: boolean;
}

export interface SupplierReturnListItem {
  externalId: string;
  supplier: ReturnSupplierInfo;
  returnDate: Date;
  itemCount: number;
  notes?: string;
  createdOn: Date;
}

export interface SupplierReturnDetail {
  externalId: string;
  supplier: ReturnSupplierInfo;
  returnDate: Date;
  itemCount: number;
  notes?: string;
  createdOn: Date;
  createdBy?: string;
  items: ReturnItem[];
}

export interface CreateSupplierReturnRequest {
  supplierExternalId: string;
  itemExternalIds: string[];
  notes?: string;
}

export interface SupplierReturnPagedResult {
  data: SupplierReturnListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
