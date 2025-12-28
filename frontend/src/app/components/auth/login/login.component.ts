import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { LoginRequest } from '../../../models/auth.models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="container">
      <div class="login-container">
        <h2>Login</h2>

        <form (ngSubmit)="onSubmit()" #loginForm="ngForm">
          <div class="form-group">
            <label class="form-label">Email</label>
            <input
              type="email"
              class="form-control"
              [(ngModel)]="credentials.email"
              name="email"
              required
              email
            />
          </div>

          <div class="form-group">
            <label class="form-label">Password</label>
            <input
              type="password"
              class="form-control"
              [(ngModel)]="credentials.password"
              name="password"
              required
            />
          </div>

          <div class="alert alert-error" *ngIf="errorMessage">
            {{ errorMessage }}
          </div>

          <button
            type="submit"
            class="btn btn-primary btn-block"
            [disabled]="!loginForm.valid || loading"
          >
            {{ loading ? 'Loading...' : 'Login' }}
          </button>
        </form>

        <p class="register-link">
          Don't have an account? <a routerLink="/register">Register here</a>
        </p>
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      max-width: 400px;
      margin: 50px auto;
      padding: 30px;
      background: var(--bg-card);
      color: var(--text-primary);
      border-radius: 8px;
      box-shadow: var(--shadow-md);
      border: 1px solid var(--border-color);
    }

    h2 {
      text-align: center;
      margin-bottom: 30px;
      color: var(--text-primary);
    }

    .btn-block {
      width: 100%;
    }

    .register-link {
      text-align: center;
      margin-top: 20px;
      color: var(--text-secondary);
    }

    .register-link a {
      color: var(--primary);
      text-decoration: none;
      font-weight: 500;
    }

    .register-link a:hover {
      color: var(--primary-light);
      text-decoration: underline;
    }
  `]
})
export class LoginComponent {
  credentials: LoginRequest = {
    email: '',
    password: ''
  };

  loading = false;
  errorMessage = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit(): void {
    this.loading = true;
    this.errorMessage = '';

    this.authService.login(this.credentials).subscribe({
      next: () => {
        this.router.navigate(['/home']);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Login failed';
        this.loading = false;
      }
    });
  }
}
