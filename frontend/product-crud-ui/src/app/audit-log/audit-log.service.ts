import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ResultModel } from '../product.model';
import {
  AuditLogDTO,
  AuditLogFilterDTO
} from './audit-log.model';

@Injectable({ providedIn: 'root' })
export class AuditLogService {
  private readonly apiUrl = '/api/AuditLogs';

  constructor(private http: HttpClient) {}

  getAll(
    filter: AuditLogFilterDTO
  ): Observable<ResultModel<AuditLogDTO[]>> {
    let params = new HttpParams()
      .set('pageIndex', filter.pageIndex)
      .set('pageSize', filter.pageSize);

    if (filter.keyword?.trim()) {
      params = params.set('keyword', filter.keyword.trim());
    }

    if (filter.action) {
      params = params.set('action', filter.action);
    }

    return this.http.get<ResultModel<AuditLogDTO[]>>(
      `${this.apiUrl}/GetAll`,
      { params }
    );
  }
}
