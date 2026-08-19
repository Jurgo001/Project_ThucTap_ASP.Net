import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  ProductDTO,
  ProductFilterDTO,
  ProductModel,
  ResultModel
} from './product.model';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly apiUrl = '/api/ProductManagement';

  constructor(private http: HttpClient) {}

  getAll(filter: ProductFilterDTO): Observable<ResultModel<ProductDTO[]>> {
    let params = new HttpParams()
      .set('pageIndex', filter.pageIndex)
      .set('pageSize', filter.pageSize)
      .set('sortField', filter.sortField)
      .set('sortDirection', filter.sortDirection);

    if (filter.keyword?.trim()) {
      params = params.set('keyword', filter.keyword.trim());
    }

    if (filter.isActive !== null && filter.isActive !== undefined) {
      params = params.set('isActive', filter.isActive);
    }

    return this.http.get<ResultModel<ProductDTO[]>>(
      `${this.apiUrl}/GetAll`,
      { params }
    );
  }

  getById(id: number): Observable<ResultModel<ProductDTO>> {
    return this.http.get<ResultModel<ProductDTO>>(
      `${this.apiUrl}/GetById/${id}`
    );
  }

  create(model: ProductModel): Observable<ResultModel<number>> {
    return this.http.post<ResultModel<number>>(
      `${this.apiUrl}/Create`,
      model
    );
  }

  update(model: ProductModel): Observable<ResultModel<boolean>> {
    return this.http.put<ResultModel<boolean>>(
      `${this.apiUrl}/Update`,
      model
    );
  }

  delete(id: number): Observable<ResultModel<boolean>> {
    return this.http.delete<ResultModel<boolean>>(
      `${this.apiUrl}/Delete/${id}`
    );
  }
}
