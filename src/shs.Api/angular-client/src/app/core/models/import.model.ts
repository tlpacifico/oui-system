export interface ImportResult {
  rowsRead: number;
  brandsCreated: number;
  suppliersCreated: number;
  itemsImported: number;
  errors: number;
  errorDetails: string[];
}
