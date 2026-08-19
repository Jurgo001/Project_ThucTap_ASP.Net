import { ResultModel } from '../product.model';

export interface LoginModel {
  username: string;
  password: string;
}

export interface LoginResponseDTO {
  userId: number;
  username: string;
  role: string;
  token: string;
  expiresAtUtc: string;
}

export interface AuthUser {
  userId: number;
  username: string;
  role: string;
  token: string;
  expiresAtUtc: string;
}

export type LoginResult = ResultModel<LoginResponseDTO>;
