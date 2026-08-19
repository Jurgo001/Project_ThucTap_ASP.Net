export interface ProductDTO {
  id: number;
  productCode: string;
  productName: string;
  price: number;
  quantity: number;
  isActive: boolean;
  createdDate: string;
  modifiedDate?: string | null;
}

export interface ProductModel {
  id: number;
  productCode: string;
  productName: string;
  price: number;
  quantity: number;
  isActive: boolean;
}

export interface ProductFilterDTO {
  keyword?: string;
  isActive?: boolean | null;
  pageIndex: number;
  pageSize: number;
  sortField: string;
  sortDirection: 'asc' | 'desc';
}

export interface ResultModel<T> {
  success: boolean;
  message: string;
  data: T;
  totalRecords?: number | null;
}
