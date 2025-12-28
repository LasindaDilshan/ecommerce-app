import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { UserService } from '../../services/user.service';
import { User } from '../../models/auth.models';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container animate-fadeIn">
      <div class="page-header">
        <h1>My Profile</h1>
        <p class="subtitle">Manage your account information</p>
      </div>

      <div class="profile-content">
        <!-- Profile Card -->
        <div class="profile-card animate-scaleIn" *ngIf="user">
          <div class="profile-avatar">
            <div class="avatar-circle">
              {{ getInitials() }}
            </div>
            <button class="btn btn-secondary btn-sm">Change Photo</button>
          </div>

          <div class="profile-info">
            <h2>{{ user.firstName }} {{ user.lastName }}</h2>
            <p class="email">{{ user.email }}</p>
            <span [class.badge-success]="user.isActive" [class.badge-danger]="!user.isActive" class="badge">
              {{ user.isActive ? 'Active' : 'Inactive' }}
            </span>
          </div>
        </div>

        <!-- Edit Form -->
        <div class="form-card animate-slideInRight" *ngIf="user">
          <h3>Personal Information</h3>

          <form (ngSubmit)="updateProfile()" #profileForm="ngForm">
            <div class="form-row">
              <div class="form-group">
                <label class="form-label">First Name *</label>
                <input
                  type="text"
                  class="form-control"
                  [(ngModel)]="user.firstName"
                  name="firstName"
                  required
                />
              </div>

              <div class="form-group">
                <label class="form-label">Last Name *</label>
                <input
                  type="text"
                  class="form-control"
                  [(ngModel)]="user.lastName"
                  name="lastName"
                  required
                />
              </div>
            </div>

            <div class="form-group">
              <label class="form-label">Email Address</label>
              <input
                type="email"
                class="form-control"
                [(ngModel)]="user.email"
                name="email"
                disabled
              />
              <small class="form-text">Email address cannot be changed</small>
            </div>

            <div class="form-group">
              <label class="form-label">Phone Number</label>
              <input
                type="tel"
                class="form-control"
                [(ngModel)]="user.phoneNumber"
                name="phoneNumber"
                placeholder="+1 (555) 123-4567"
              />
            </div>

            <div class="alert alert-success" *ngIf="successMessage">
              {{ successMessage }}
            </div>

            <div class="alert alert-error" *ngIf="errorMessage">
              {{ errorMessage }}
            </div>

            <div class="form-actions">
              <button type="submit" class="btn btn-primary" [disabled]="!profileForm.valid || saving">
                {{ saving ? 'Saving...' : 'Save Changes' }}
              </button>
              <button type="button" class="btn btn-secondary" (click)="cancelEdit()">
                Cancel
              </button>
            </div>
          </form>
        </div>

        <!-- Account Actions -->
        <div class="actions-card">
          <h3>Account Actions</h3>

          <div class="action-list">
            <button class="action-btn">
              <span class="icon">🔒</span>
              <div class="action-content">
                <strong>Change Password</strong>
                <small>Update your password regularly for security</small>
              </div>
            </button>

            <button class="action-btn">
              <span class="icon">📍</span>
              <div class="action-content">
                <strong>Manage Addresses</strong>
                <small>Add or edit your delivery addresses</small>
              </div>
            </button>

            <button class="action-btn">
              <span class="icon">🔐</span>
              <div class="action-content">
                <strong>Two-Factor Authentication</strong>
                <small>Add an extra layer of security</small>
              </div>
            </button>

            <button class="action-btn danger">
              <span class="icon">🗑️</span>
              <div class="action-content">
                <strong>Delete Account</strong>
                <small>Permanently delete your account</small>
              </div>
            </button>
          </div>
        </div>
      </div>

      <div class="loading" *ngIf="loading">
        <div class="spinner"></div>
        <p>Loading profile...</p>
      </div>
    </div>
  `,
  styles: [`
    .container {
      padding: 20px;
      max-width: 1200px;
      margin: 0 auto;
    }

    .page-header {
      margin-bottom: 30px;
      padding-bottom: 20px;
      border-bottom: 2px solid var(--border-color);
    }

    .page-header h1 {
      color: var(--text-primary);
      font-size: 32px;
      font-weight: 700;
      margin: 0 0 8px 0;
    }

    .subtitle {
      color: var(--text-secondary);
      font-size: 16px;
      margin: 0;
    }

    .profile-content {
      display: grid;
      grid-template-columns: 300px 1fr;
      gap: 24px;
    }

    .profile-card {
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: 12px;
      padding: 24px;
      box-shadow: var(--shadow-md);
      text-align: center;
      height: fit-content;
      position: sticky;
      top: 20px;
    }

    .profile-avatar {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 16px;
      margin-bottom: 20px;
    }

    .avatar-circle {
      width: 120px;
      height: 120px;
      border-radius: 50%;
      background: var(--gradient-primary);
      color: white;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 48px;
      font-weight: 700;
      box-shadow: var(--shadow-lg);
    }

    .profile-info h2 {
      color: var(--text-primary);
      font-size: 24px;
      margin: 0 0 8px 0;
    }

    .profile-info .email {
      color: var(--text-secondary);
      font-size: 14px;
      margin-bottom: 12px;
    }

    .badge {
      padding: 4px 12px;
      border-radius: 12px;
      font-size: 12px;
      font-weight: 600;
      display: inline-block;
    }

    .badge-success {
      background: rgba(16, 185, 129, 0.1);
      color: var(--success);
      border: 1px solid var(--success);
    }

    .badge-danger {
      background: rgba(239, 68, 68, 0.1);
      color: var(--danger);
      border: 1px solid var(--danger);
    }

    .form-card, .actions-card {
      background: var(--bg-card);
      border: 1px solid var(--border-color);
      border-radius: 12px;
      padding: 24px;
      box-shadow: var(--shadow-md);
    }

    .form-card h3, .actions-card h3 {
      color: var(--text-primary);
      font-size: 20px;
      margin: 0 0 24px 0;
      padding-bottom: 12px;
      border-bottom: 1px solid var(--border-color);
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 20px;
    }

    .form-text {
      display: block;
      margin-top: 6px;
      color: var(--text-tertiary);
      font-size: 13px;
    }

    .form-actions {
      display: flex;
      gap: 12px;
      margin-top: 24px;
    }

    .action-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .action-btn {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 16px;
      background: var(--bg-secondary);
      border: 1px solid var(--border-color);
      border-radius: 8px;
      cursor: pointer;
      transition: all 0.3s ease;
      text-align: left;
      width: 100%;
    }

    .action-btn:hover {
      background: var(--bg-hover);
      transform: translateX(4px);
      box-shadow: var(--shadow-sm);
    }

    .action-btn .icon {
      font-size: 24px;
      width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--bg-card);
      border-radius: 8px;
    }

    .action-content strong {
      display: block;
      color: var(--text-primary);
      font-size: 15px;
      margin-bottom: 4px;
    }

    .action-content small {
      display: block;
      color: var(--text-secondary);
      font-size: 13px;
    }

    .action-btn.danger:hover {
      border-color: var(--danger);
      background: rgba(239, 68, 68, 0.05);
    }

    .action-btn.danger .action-content strong {
      color: var(--danger);
    }

    .loading {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 60px 20px;
      color: var(--text-secondary);
    }

    .spinner {
      border: 4px solid var(--border-color);
      border-top: 4px solid var(--primary);
      border-radius: 50%;
      width: 50px;
      height: 50px;
      animation: spin 1s linear infinite;
      margin-bottom: 20px;
    }

    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }

    .animate-fadeIn {
      animation: fadeIn 0.6s ease-out;
    }

    .animate-scaleIn {
      animation: scaleIn 0.6s ease-out;
    }

    .animate-slideInRight {
      animation: slideInRight 0.6s ease-out;
    }

    @keyframes fadeIn {
      from {
        opacity: 0;
        transform: translateY(20px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    @keyframes scaleIn {
      from {
        opacity: 0;
        transform: scale(0.9);
      }
      to {
        opacity: 1;
        transform: scale(1);
      }
    }

    @keyframes slideInRight {
      from {
        opacity: 0;
        transform: translateX(50px);
      }
      to {
        opacity: 1;
        transform: translateX(0);
      }
    }

    @media (max-width: 1024px) {
      .profile-content {
        grid-template-columns: 1fr;
      }

      .profile-card {
        position: relative;
        top: 0;
      }
    }

    @media (max-width: 768px) {
      .container {
        padding: 15px;
      }

      .page-header h1 {
        font-size: 24px;
      }

      .form-row {
        grid-template-columns: 1fr;
      }

      .form-actions {
        flex-direction: column;
      }

      .form-actions .btn {
        width: 100%;
      }

      .action-btn {
        flex-direction: column;
        text-align: center;
      }
    }
  `]
})
export class ProfileComponent implements OnInit, OnDestroy {
  user: User | null = null;
  loading = false;
  saving = false;
  successMessage = '';
  errorMessage = '';
  private destroy$ = new Subject<void>();
  private timeoutIds: any[] = [];

  constructor(private userService: UserService) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  private loadProfile(): void {
    this.loading = true;
    this.userService.getProfile()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (user) => {
          this.user = user;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading profile:', error);
          this.errorMessage = 'Failed to load profile';
          this.loading = false;
        }
      });
  }

  updateProfile(): void {
    if (this.user) {
      this.saving = true;
      this.successMessage = '';
      this.errorMessage = '';

      this.userService.updateProfile({
        firstName: this.user.firstName,
        lastName: this.user.lastName,
        phoneNumber: this.user.phoneNumber
      })
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.successMessage = 'Profile updated successfully!';
            this.saving = false;
            this.timeoutIds.push(setTimeout(() => this.successMessage = '', 3000));
          },
          error: (error) => {
            this.errorMessage = 'Failed to update profile';
            this.saving = false;
            this.timeoutIds.push(setTimeout(() => this.errorMessage = '', 3000));
          }
        });
    }
  }

  cancelEdit(): void {
    this.loadProfile(); // Reload profile data
    this.successMessage = '';
    this.errorMessage = '';
  }

  getInitials(): string {
    if (!this.user) return '?';
    const first = this.user.firstName?.charAt(0) || '';
    const last = this.user.lastName?.charAt(0) || '';
    return (first + last).toUpperCase() || '?';
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.timeoutIds.forEach(id => clearTimeout(id));
  }
}
