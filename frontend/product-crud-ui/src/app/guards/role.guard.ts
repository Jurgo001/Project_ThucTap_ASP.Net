import { inject } from '@angular/core';
import { ToastService } from '../shared/toast.service';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

export const roleGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const toastService = inject(ToastService);

  const allowedRoles = route.data['roles'] as string[] | undefined;

  if (!allowedRoles || authService.hasRole(allowedRoles)) {
    return true;
  }

  toastService.error('Bạn không có quyền truy cập trang này.');

  return router.createUrlTree(['/products']);
};