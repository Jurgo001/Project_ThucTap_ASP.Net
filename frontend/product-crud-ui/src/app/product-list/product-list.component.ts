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
import { AuthService } from '../auth/auth.service';
import { FileUploadComponent } from '../file-upload/file-upload.component';
import {
  ProductDTO,
  ProductFilterDTO,
  ProductModel
} from '../product.model';
import { ProductService } from '../product.service';
import { ToastService } from '../shared/toast.service';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    FileUploadComponent
  ],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.css']
})
export class ProductListComponent implements OnInit, OnDestroy {
  products: ProductDTO[] = [];
  keyword = '';
  isActive: boolean | null = null;
  pageIndex = 1;
  pageSize = 5;
  totalRecords = 0;
  sortField = 'Id';
  sortDirection: 'asc' | 'desc' = 'desc';
  isLoading = false;

  showPopup = false;
  isEditMode = false;
  model: ProductModel = this.emptyModel();

  private readonly searchSubject = new Subject<string>();
  private readonly destroySubject = new Subject<void>();

  constructor(
    private productService: ProductService,
    public authService: AuthService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.searchSubject
      .pipe(
        debounceTime(350),
        distinctUntilChanged(),
        takeUntil(this.destroySubject)
      )
      .subscribe(() => {
        this.pageIndex = 1;
        this.loadProducts();
      });

    this.loadProducts();
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

  get canCreateOrEdit(): boolean {
    return this.authService.hasRole(['Admin', 'Editor']);
  }

  get canDelete(): boolean {
    return this.authService.hasRole(['Admin']);
  }

  loadProducts(): void {
    const filter: ProductFilterDTO = {
      keyword: this.keyword,
      isActive: this.isActive,
      pageIndex: this.pageIndex,
      pageSize: this.pageSize,
      sortField: this.sortField,
      sortDirection: this.sortDirection
    };

    this.isLoading = true;

    this.productService
      .getAll(filter)
      .pipe(finalize(() => this.isLoading = false))
      .subscribe({
        next: (result) => {
          this.products = result.data ?? [];
          this.totalRecords = result.totalRecords ?? 0;
        },
        error: () => {
          // ApiInterceptor đã hiển thị lỗi.
        }
      });
  }

  onKeywordChange(value: string): void {
    this.searchSubject.next(value);
  }

  onActiveFilterChange(): void {
    this.pageIndex = 1;
    this.loadProducts();
  }

  onPageSizeChange(): void {
    this.pageIndex = 1;
    this.loadProducts();
  }

  changePage(pageIndex: number): void {
    if (
      pageIndex < 1 ||
      pageIndex > this.totalPages
    ) {
      return;
    }

    this.pageIndex = pageIndex;
    this.loadProducts();
  }

  sortBy(field: string): void {
    if (this.sortField === field) {
      this.sortDirection =
        this.sortDirection === 'asc'
          ? 'desc'
          : 'asc';
    } else {
      this.sortField = field;
      this.sortDirection = 'asc';
    }

    this.pageIndex = 1;
    this.loadProducts();
  }

  getSortIcon(field: string): string {
    if (this.sortField !== field) {
      return '↕';
    }

    return this.sortDirection === 'asc'
      ? '↑'
      : '↓';
  }

  openCreate(): void {
    if (!this.canCreateOrEdit) {
      return;
    }

    this.isEditMode = false;
    this.model = this.emptyModel();
    this.showPopup = true;
  }

  openEdit(id: number): void {
    if (!this.canCreateOrEdit) {
      return;
    }

    this.productService.getById(id).subscribe({
      next: (result) => {
        this.model = {
          id: result.data.id,
          productCode: result.data.productCode,
          productName: result.data.productName,
          price: result.data.price,
          quantity: result.data.quantity,
          isActive: result.data.isActive
        };

        this.isEditMode = true;
        this.showPopup = true;
      },
      error: () => {
        // ApiInterceptor đã hiển thị lỗi.
      }
    });
  }

  closePopup(): void {
    this.showPopup = false;
  }

  save(): void {
    if (
      !this.model.productCode.trim() ||
      !this.model.productName.trim()
    ) {
      this.toastService.error(
        'Vui lòng nhập mã và tên sản phẩm.'
      );
      return;
    }

    if (this.isEditMode) {
      this.productService.update(this.model).subscribe({
        next: (result) => {
          this.toastService.success(result.message);
          this.showPopup = false;
          this.loadProducts();
        },
        error: () => {
          // ApiInterceptor đã hiển thị lỗi.
        }
      });

      return;
    }

    this.productService.create(this.model).subscribe({
      next: (result) => {
        this.toastService.success(result.message);
        this.showPopup = false;
        this.loadProducts();
      },
      error: () => {
        // ApiInterceptor đã hiển thị lỗi.
      }
    });
  }

  deleteProduct(item: ProductDTO): void {
    if (!this.canDelete) {
      return;
    }

    const isConfirmed = confirm(
      `Bạn có chắc muốn xóa "${item.productName}"?`
    );

    if (!isConfirmed) {
      return;
    }

    this.productService.delete(item.id).subscribe({
      next: (result) => {
        this.toastService.success(result.message);

        if (
          this.products.length === 1 &&
          this.pageIndex > 1
        ) {
          this.pageIndex--;
        }

        this.loadProducts();
      },
      error: () => {
        // ApiInterceptor đã hiển thị lỗi.
      }
    });
  }

  private emptyModel(): ProductModel {
    return {
      id: 0,
      productCode: '',
      productName: '',
      price: 0,
      quantity: 0,
      isActive: true
    };
  }
}
