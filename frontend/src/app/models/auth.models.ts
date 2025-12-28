export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  userId: number;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  accessToken: string;
  refreshToken: string;
  expiresAt: Date;
  requiresTwoFactor: boolean;
  emailVerified: boolean;
}

export interface User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  role: string;
  isActive: boolean;
  createdAt: Date;
}
