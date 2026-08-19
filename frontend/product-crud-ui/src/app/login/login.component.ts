import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { LoginModel } from '../auth/auth.model';
import { ToastService } from '../shared/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  model: LoginModel = {
    username: 'admin',
    password: 'Admin@123'
  };

  isSubmitting = false;

  constructor(
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private toastService: ToastService
  ) {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/products']);
    }
  }

  login(): void {
    if (!this.model.username.trim() || !this.model.password) {
      this.toastService.error('Vui lòng nhập tên đăng nhập và mật khẩu.');
      return;
    }

    this.isSubmitting = true;

    this.authService
      .login(this.model)
      .pipe(finalize(() => this.isSubmitting = false))
      .subscribe({
        next: (result) => {
          this.toastService.success(result.message);

          const returnUrl =
            this.route.snapshot.queryParamMap.get('returnUrl') ??
            '/products';

          this.router.navigateByUrl(returnUrl);
        },
        error: () => {
          // ApiInterceptor đã hiển thị lỗi.
        }
      });
  }
}
