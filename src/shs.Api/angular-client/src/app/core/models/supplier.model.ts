export interface Supplier {
  externalId: string;
  name: string;
  email: string;
  phoneNumber: string;
  taxNumber?: string;
  initial: string;
  notes?: string;
  itemCount: number;
  createdOn: Date;
  createdBy?: string;
  updatedOn?: Date;
  updatedBy?: string;
}

export interface SupplierListItem {
  externalId: string;
  name: string;
  email: string;
  phoneNumber: string;
  taxNumber?: string;
  initial: string;
  itemCount: number;
  createdOn: Date;
}

export interface CreateSupplierRequest {
  name: string;
  email: string;
  phoneNumber: string;
  taxNumber?: string;
  initial: string;
  notes?: string;
}

export interface UpdateSupplierRequest {
  name: string;
  email: string;
  phoneNumber: string;
  taxNumber?: string;
  initial: string;
  notes?: string;
}

export interface SupplierItemListItem {
  externalId: string;
  identificationNumber: string;
  name: string;
  brand: string;
  size: string;
  evaluatedPrice: number;
  status: string;
  condition: string;
  daysInStock: number;
  createdOn: Date;
}

export interface SupplierReception {
  externalId: string;
  receptionDate: Date;
  itemCount: number;
  status: string;
  evaluatedCount: number;
  acceptedCount: number;
  rejectedCount: number;
  notes?: string;
  createdOn: Date;
}
