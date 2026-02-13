export interface Tag {
  externalId: string;
  name: string;
  color?: string;
  itemCount: number;
  createdOn: Date;
  createdBy?: string;
  updatedOn?: Date;
  updatedBy?: string;
}

export interface TagListItem {
  externalId: string;
  name: string;
  color?: string;
  itemCount: number;
  createdOn: Date;
}

export interface CreateTagRequest {
  name: string;
  color?: string;
}

export interface UpdateTagRequest {
  name: string;
  color?: string;
}
