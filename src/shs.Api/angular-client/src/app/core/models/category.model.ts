export interface Category {
  externalId: string;
  name: string;
  description?: string;
  parentCategory?: CategoryParentInfo;
  subCategories: CategoryChildInfo[];
  itemCount: number;
  createdOn: Date;
  createdBy?: string;
  updatedOn?: Date;
  updatedBy?: string;
}

export interface CategoryListItem {
  externalId: string;
  name: string;
  description?: string;
  parentCategory?: CategoryParentInfo;
  subCategoryCount: number;
  itemCount: number;
  createdOn: Date;
}

export interface CategoryParentInfo {
  externalId: string;
  name: string;
}

export interface CategoryChildInfo {
  externalId: string;
  name: string;
}

export interface CreateCategoryRequest {
  name: string;
  description?: string;
  parentCategoryExternalId?: string;
}

export interface UpdateCategoryRequest {
  name: string;
  description?: string;
  parentCategoryExternalId?: string;
}
