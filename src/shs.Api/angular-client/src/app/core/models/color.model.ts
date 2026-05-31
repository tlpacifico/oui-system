export interface Color {
  externalId: string;
  name: string;
  hexCode?: string;
  itemCount: number;
  createdOn: Date;
  createdBy?: string;
  updatedOn?: Date;
  updatedBy?: string;
}

export interface ColorListItem {
  externalId: string;
  name: string;
  hexCode?: string;
  itemCount: number;
  createdOn: Date;
}

export interface CreateColorRequest {
  name: string;
  hexCode?: string;
}

export interface UpdateColorRequest {
  name: string;
  hexCode?: string;
}
