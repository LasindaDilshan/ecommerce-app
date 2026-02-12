import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface TwoFactorSetupResponse {
  qrCodeUri: string;
  manualEntryKey: string;
  recoveryCodes: string[];
}

export interface TwoFactorStatusResponse {
  isEnabled: boolean;
  hasRecoveryCodes: boolean;
}

export interface VerifyTwoFactorRequest {
  code: string;
}

export interface TwoFactorLoginRequest {
  email: string;
  code: string;
}

export interface RecoveryCodeLoginRequest {
  email: string;
  recoveryCode: string;
}

@Injectable({
  providedIn: 'root'
})
export class TwoFactorService {
  private apiUrl = `${environment.apiUrl}/twofactor`;

  constructor(private http: HttpClient) {}

  getStatus(): Observable<TwoFactorStatusResponse> {
    return this.http.get<TwoFactorStatusResponse>(`${this.apiUrl}/status`);
  }

  enable(): Observable<TwoFactorSetupResponse> {
    return this.http.post<TwoFactorSetupResponse>(`${this.apiUrl}/enable`, {});
  }

  verifySetup(code: string): Observable<{ message: string; recoveryCodes: string[] }> {
    return this.http.post<{ message: string; recoveryCodes: string[] }>(`${this.apiUrl}/verify-setup`, { code });
  }

  disable(code: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/disable`, { code });
  }

  regenerateRecoveryCodes(code: string): Observable<{ recoveryCodes: string[] }> {
    return this.http.post<{ recoveryCodes: string[] }>(`${this.apiUrl}/regenerate-recovery-codes`, { code });
  }

  loginWithCode(request: TwoFactorLoginRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, request);
  }

  loginWithRecoveryCode(request: RecoveryCodeLoginRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login-recovery`, request);
  }
}
