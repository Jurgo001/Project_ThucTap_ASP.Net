import {
  HttpClient,
  HttpEvent
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ResultModel } from '../product.model';
import { FileUploadResultDTO } from './file-upload.model';

@Injectable({ providedIn: 'root' })
export class FileUploadService {
  private readonly apiUrl = '/api/Files';

  constructor(private http: HttpClient) {}

  upload(
    file: File
  ): Observable<HttpEvent<ResultModel<FileUploadResultDTO>>> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<ResultModel<FileUploadResultDTO>>(
      `${this.apiUrl}/Upload`,
      formData,
      {
        observe: 'events',
        reportProgress: true
      }
    );
  }
}
