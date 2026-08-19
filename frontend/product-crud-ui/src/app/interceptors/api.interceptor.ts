import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import {
  Observable,
  catchError,
  retry,
  throwError,
  timer
} from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { ToastService } from '../shared/toast.service';

@Injectable()
export class ApiInterceptor implements HttpInterceptor {
  private readonly retryableStatusCodes = [
    408,
    500,
    502,
    503,
    504,
    522,
    524
  ];

  private readonly retryDelays = [
    300,
    600,
    1200
  ];

  constructor(
    private authService: AuthService,
    private router: Router,
    private toastService: ToastService
  ) {}

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    const isLoginRequest = request.url.includes('/api/Authorization/Login');
    const token = isLoginRequest
      ? null
      : this.authService.token;

    const authenticatedRequest = token
      ? request.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`
          }
        })
      : request;

    let requestStream = next.handle(authenticatedRequest);

    if (request.method === 'GET') {
      requestStream = requestStream.pipe(
        retry({
          count: 3,
          delay: (error: HttpErrorResponse, retryCount: number) => {
            if (!this.retryableStatusCodes.includes(error.status)) {
              return throwError(() => error);
            }

            return timer(this.retryDelays[retryCount - 1]);
          }
        })
      );
    }

    return requestStream.pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401 && !isLoginRequest) {
          this.authService.logout();
          this.toastService.error('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
          this.router.navigate(['/login']);
        } else if (error.status === 403) {
          this.toastService.error('Bạn không có quyền thực hiện chức năng này.');
        } else {
          const message =
            error.error?.message ??
            'Có lỗi xảy ra khi xử lý yêu cầu.';

          this.toastService.error(message);
        }

        return throwError(() => error);
      })
    );
  }
}
