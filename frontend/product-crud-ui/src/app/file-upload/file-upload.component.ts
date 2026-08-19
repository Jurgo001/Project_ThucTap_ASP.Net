import {
  HttpEventType,
  HttpResponse
} from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FileUploadService } from './file-upload.service';
import { FileUploadResultDTO } from './file-upload.model';
import { ResultModel } from '../product.model';
import { ToastService } from '../shared/toast.service';

@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.css']
})
export class FileUploadComponent {
  selectedFile: File | null = null;
  progress = 0;
  isUploading = false;
  uploadedFile: FileUploadResultDTO | null = null;

  constructor(
    private fileUploadService: FileUploadService,
    private toastService: ToastService
  ) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile = input.files?.[0] ?? null;
    this.progress = 0;
    this.uploadedFile = null;
  }

  upload(): void {
    if (!this.selectedFile) {
      this.toastService.error('Vui lòng chọn file trước khi tải lên.');
      return;
    }

    this.isUploading = true;
    this.progress = 0;

    this.fileUploadService.upload(this.selectedFile).subscribe({
      next: (event) => {
        if (
          event.type === HttpEventType.UploadProgress &&
          event.total
        ) {
          this.progress = Math.round(
            100 * event.loaded / event.total
          );
        }

        if (event instanceof HttpResponse) {
          const body =
            event.body as ResultModel<FileUploadResultDTO> | null;

          if (body) {
            this.uploadedFile = body.data;
            this.progress = 100;
            this.toastService.success(body.message);
          }

          this.isUploading = false;
        }
      },
      error: () => {
        this.isUploading = false;
      }
    });
  }
}
