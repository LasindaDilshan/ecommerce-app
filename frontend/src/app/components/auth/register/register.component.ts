import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { RegisterRequest } from '../../../models/auth.models';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="container">
      <div class="register-container">
        <h2>Register</h2>

        <form (ngSubmit)="onSubmit()" #registerForm="ngForm">
          <div class="form-group">
            <label class="form-label">First Name</label>
            <input
              type="text"
              class="form-control"
              [(ngModel)]="formData.firstName"
              name="firstName"
              required
            />
          </div>

          <div class="form-group">
            <label class="form-label">Last Name</label>
            <input
              type="text"
              class="form-control"
              [(ngModel)]="formData.lastName"
              name="lastName"
              required
            />
          </div>

          <div class="form-group">
            <label class="form-label">Email</label>
            <input
              type="email"
              class="form-control"
              [(ngModel)]="formData.email"
              name="email"
              required
              email
            />
          </div>

          <div class="form-group">
            <label class="form-label">Phone Number</label>
            <input
              type="tel"
              class="form-control"
              [(ngModel)]="formData.phoneNumber"
              name="phoneNumber"
            />
          </div>

          <div class="form-group">
            <label class="form-label">Password</label>
            <input
              type="password"
              class="form-control"
              [(ngModel)]="formData.password"
              name="password"
              required
              minlength="6"
            />
          </div>

          <div class="alert alert-error" *ngIf="errorMessage">
            {{ errorMessage }}
          </div>

          <button
            type="submit"
            class="btn btn-primary btn-block"
            [disabled]="!registerForm.valid || loading"
          >
            {{ loading ? 'Loading...' : 'Register' }}
          </button>
        </form>

        <p class="login-link">
          Already have an account? <a routerLink="/login">Login here</a>
        </p>
      </div>
    </div>
  `,
  styles: [`
    .register-container {
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

    .login-link {
      text-align: center;
      margin-top: 20px;
      color: var(--text-secondary);
    }

    .login-link a {
      color: var(--primary);
      text-decoration: none;
      font-weight: 500;
    }

    .login-link a:hover {
      color: var(--primary-light);
      text-decoration: underline;
    }
  `]
})
export class RegisterComponent {
  formData: RegisterRequest = {
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    phoneNumber: ''
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

    this.authService.register(this.formData).subscribe({
      next: () => {
        this.router.navigate(['/home']);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Registration failed';
        this.loading = false;
      }
    });
  }
}
