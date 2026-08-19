import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import {
  AuthUser,
  LoginModel,
  LoginResult
} from './auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = '/api/Authorization';
  private readonly storageKey = 'productCrudAuth';

  constructor(private http: HttpClient) {}

  login(model: LoginModel): Observable<LoginResult> {
    return this.http
      .post<LoginResult>(`${this.apiUrl}/Login`, model)
      .pipe(
        tap((result) => {
          const authUser: AuthUser = {
            userId: result.data.userId,
            username: result.data.username,
            role: result.data.role,
            token: result.data.token,
            expiresAtUtc: result.data.expiresAtUtc
          };

          localStorage.setItem(
            this.storageKey,
            JSON.stringify(authUser)
          );
        })
      );
  }

  logout(): void {
    localStorage.removeItem(this.storageKey);
  }

  get currentUser(): AuthUser | null {
    const storedValue = localStorage.getItem(this.storageKey);

    if (!storedValue) {
      return null;
    }

    try {
      return JSON.parse(storedValue) as AuthUser;
    } catch {
      this.logout();
      return null;
    }
  }

  get token(): string | null {
    return this.currentUser?.token ?? null;
  }

  isAuthenticated(): boolean {
    const user = this.currentUser;

    if (!user || !user.token) {
      return false;
    }

    if (new Date(user.expiresAtUtc).getTime() <= Date.now()) {
      this.logout();
      return false;
    }

    return true;
  }

  hasRole(roles: string[]): boolean {
    const role = this.currentUser?.role;
    return !!role && roles.includes(role);
  }
}
