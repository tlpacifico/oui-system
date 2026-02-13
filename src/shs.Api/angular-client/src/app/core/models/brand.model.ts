export interface Brand {
  externalId: string;
  name: string;
  description?: string;
  logoUrl?: string;
  itemCount: number;
  createdOn: Date;
  createdBy?: string;
  updatedOn?: Date;
  updatedBy?: string;
}

export interface BrandListItem {
  externalId: string;
  name: string;
  description?: string;
  logoUrl?: string;
  itemCount: number;
  createdOn: Date;
}

export interface CreateBrandRequest {
  name: string;
  description?: string;
  logoUrl?: string;
}

export interface UpdateBrandRequest {
  name: string;
  description?: string;
  logoUrl?: string;
}
