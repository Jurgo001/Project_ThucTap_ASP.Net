import { CommonModule } from '@angular/common';
import {
  Component,
  OnDestroy,
  OnInit
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  Subject,
  debounceTime,
  distinctUntilChanged,
  finalize,
  takeUntil
} from 'rxjs';
import {
  AuditLogDTO,
  AuditLogFilterDTO
} from './audit-log.model';
import { AuditLogService } from './audit-log.service';

@Component({
  selector: 'app-audit-log',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './audit-log.component.html',
  styleUrls: ['./audit-log.component.css']
})
export class AuditLogComponent implements OnInit, OnDestroy {
  auditLogs: AuditLogDTO[] = [];
  keyword = '';
  action = '';
  pageIndex = 1;
  pageSize = 10;
  totalRecords = 0;
  isLoading = false;

  private readonly searchSubject = new Subject<string>();
  private readonly destroySubject = new Subject<void>();

  constructor(private auditLogService: AuditLogService) {}

  ngOnInit(): void {
    this.searchSubject
      .pipe(
        debounceTime(350),
        distinctUntilChanged(),
        takeUntil(this.destroySubject)
      )
      .subscribe(() => {
        this.pageIndex = 1;
        this.loadAuditLogs();
      });

    this.loadAuditLogs();
  }

  ngOnDestroy(): void {
    this.destroySubject.next();
    this.destroySubject.complete();
  }

  get totalPages(): number {
    return Math.max(
      1,
      Math.ceil(this.totalRecords / this.pageSize)
    );
  }

  onKeywordChange(value: string): void {
    this.searchSubject.next(value);
  }

  onActionChange(): void {
    this.pageIndex = 1;
    this.loadAuditLogs();
  }

  changePage(pageIndex: number): void {
    if (
      pageIndex < 1 ||
      pageIndex > this.totalPages
    ) {
      return;
    }

    this.pageIndex = pageIndex;
    this.loadAuditLogs();
  }

  loadAuditLogs(): void {
    const filter: AuditLogFilterDTO = {
      keyword: this.keyword,
      action: this.action,
      pageIndex: this.pageIndex,
      pageSize: this.pageSize
    };

    this.isLoading = true;

    this.auditLogService
      .getAll(filter)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (result) => {
          this.auditLogs = result.data ?? [];
          this.totalRecords = result.totalRecords ?? 0;
        },
        error: () => {
          // ApiInterceptor đã hiển thị lỗi.
        }
      });
  }
}
