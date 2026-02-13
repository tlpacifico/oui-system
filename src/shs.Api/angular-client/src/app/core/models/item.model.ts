export interface Item {
  externalId: string;
  identificationNumber: string;
  name: string;
  description?: string;
  brand: { id: number; name: string };
  category?: { id: number; name: string };
  size: string;
  color: string;
  composition?: string;
  condition: ItemCondition;
  evaluatedPrice: number;
  costPrice?: number;
  finalSalePrice?: number;
  status: ItemStatus;
  acquisitionType: AcquisitionType;
  origin: ItemOrigin;
  supplier?: { id: number; name: string };
  commissionPercentage: number;
  commissionAmount?: number;
  isRejected: boolean;
  rejectionReason?: string;
  soldAt?: Date;
  daysInStock: number;
  tags: { id: number; name: string; color?: string }[];
  photos: ItemPhoto[];
  createdOn: Date;
  createdBy?: string;
  updatedOn?: Date;
  updatedBy?: string;
}

export interface ItemPhoto {
  externalId: string;
  filePath: string;
  thumbnailPath?: string;
  displayOrder: number;
  isPrimary: boolean;
}

export enum ItemStatus {
  Received = 'Received',
  Evaluated = 'Evaluated',
  AwaitingAcceptance = 'AwaitingAcceptance',
  ToSell = 'ToSell',
  Sold = 'Sold',
  Returned = 'Returned',
  Paid = 'Paid',
  Rejected = 'Rejected'
}

export enum ItemCondition {
  Excellent = 'Excellent',
  VeryGood = 'VeryGood',
  Good = 'Good',
  Fair = 'Fair',
  Poor = 'Poor'
}

export enum AcquisitionType {
  Consignment = 'Consignment',
  OwnPurchase = 'OwnPurchase'
}

export enum ItemOrigin {
  Consignment = 'Consignment',
  Humana = 'Humana',
  Vinted = 'Vinted',
  HM = 'HM',
  PersonalCollection = 'PersonalCollection',
  Other = 'Other'
}

export interface ItemListItem {
  externalId: string;
  identificationNumber: string;
  name: string;
  brand: string;
  size: string;
  color: string;
  evaluatedPrice: number;
  status: ItemStatus;
  primaryPhotoUrl?: string;
  daysInStock?: number;
  supplier?: string;
  condition?: string;
  createdOn: Date;
}

export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateItemRequest {
  name: string;
  description?: string;
  brandExternalId: string;
  categoryExternalId?: string;
  size: string;
  color: string;
  composition?: string;
  condition: string;
  evaluatedPrice: number;
  costPrice?: number;
  acquisitionType: string;
  origin: string;
  supplierExternalId?: string;
  commissionPercentage?: number;
  tagExternalIds?: string[];
}

export interface UpdateItemRequest {
  name: string;
  description?: string;
  brandExternalId: string;
  categoryExternalId?: string;
  size: string;
  color: string;
  composition?: string;
  condition: string;
  evaluatedPrice: number;
  costPrice?: number;
  commissionPercentage?: number;
  tagExternalIds?: string[];
}
