import { Injectable, Injector } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest, User } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router,
    private injector: Injector
  ) {
    this.loadUserFromStorage();
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request).pipe(
      tap(response => this.handleAuthResponse(response))
    );
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(response => {
        this.handleAuthResponse(response);
        // Merge guest cart after login (lazy injection to avoid circular dependency)
        this.mergeGuestCartAfterLogin();
      })
    );
  }

  private mergeGuestCartAfterLogin(): void {
    // Use setTimeout to break circular dependency and lazy load services
    setTimeout(() => {
      import('./guest-session.service').then(({ GuestSessionService }) => {
        const guestSessionService = this.injector.get(GuestSessionService);

        if (guestSessionService.hasSession()) {
          import('./cart.service').then(({ CartService }) => {
            const cartService = this.injector.get(CartService);

            cartService.mergeGuestCart().subscribe({
              error: (error) => {
                console.error('Failed to merge guest cart:', error);
              }
            });
          });
        }
      });
    }, 0);
  }

  logout(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = localStorage.getItem('refreshToken');
    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh`, { refreshToken }).pipe(
      tap(response => this.handleAuthResponse(response))
    );
  }

  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  isLoggedIn(): boolean {
    return !!this.getAccessToken();
  }

  isAdmin(): boolean {
    const user = this.getCurrentUser();
    return user?.role === 'Admin';
  }

  private handleAuthResponse(response: AuthResponse): void {
    localStorage.setItem('accessToken', response.accessToken);
    localStorage.setItem('refreshToken', response.refreshToken);

    const user: User = {
      id: response.userId,
      email: response.email,
      firstName: response.firstName,
      lastName: response.lastName,
      role: response.role,
      isActive: true,
      createdAt: new Date()
    };

    localStorage.setItem('currentUser', JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  private loadUserFromStorage(): void {
    const userJson = localStorage.getItem('currentUser');
    if (userJson) {
      try {
        const user = JSON.parse(userJson);
        // Validate user object has required properties
        if (this.isValidUser(user)) {
          this.currentUserSubject.next(user);
        } else {
          // Invalid user data - clear storage
          console.warn('Invalid user data in storage, clearing...');
          this.clearStoredAuth();
        }
      } catch (e) {
        // JSON parse error - clear corrupted data
        console.error('Failed to parse user from storage:', e);
        this.clearStoredAuth();
      }
    }
  }

  /**
   * Type guard to validate user object structure
   */
  private isValidUser(obj: any): obj is User {
    return (
      obj !== null &&
      typeof obj === 'object' &&
      typeof obj.id === 'number' &&
      typeof obj.email === 'string' &&
      typeof obj.firstName === 'string' &&
      typeof obj.lastName === 'string' &&
      typeof obj.role === 'string'
    );
  }

  /**
   * Clear stored authentication data
   */
  private clearStoredAuth(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
  }
}
