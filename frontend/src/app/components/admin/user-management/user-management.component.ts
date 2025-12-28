import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { UserService } from '../../../services/user.service';
import { ConfirmationModalService } from '../../../services/confirmation-modal.service';
import { ToastService } from '../../../services/toast.service';
import { User } from '../../../models/auth.models';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container">
      <div class="header-section">
        <h1>User Management</h1>
        <div class="stats">
          <span class="stat-item">Total Users: {{ users.length }}</span>
          <span class="stat-item">Active: {{ getActiveCount() }}</span>
          <span class="stat-item">Admins: {{ getAdminCount() }}</span>
        </div>
      </div>

      <!-- Users Table -->
      <div class="table-container">
        <table class="data-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Name</th>
              <th>Email</th>
              <th>Phone</th>
              <th>Role</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let user of users">
              <td><strong>{{ user.id }}</strong></td>
              <td>
                <div class="user-info">
                  <strong>{{ user.firstName }} {{ user.lastName }}</strong>
                </div>
              </td>
              <td>{{ user.email }}</td>
              <td>{{ user.phoneNumber || 'N/A' }}</td>
              <td>
                <span [class]="getRoleBadgeClass(user.role)" class="badge">
                  {{ user.role }}
                </span>
              </td>
              <td>
                <span [class.badge-success]="user.isActive" [class.badge-danger]="!user.isActive" class="badge">
                  {{ user.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>
              <td class="actions-cell">
                <button
                  (click)="toggleUserStatus(user)"
                  [class.btn-success]="!user.isActive"
                  [class.btn-danger]="user.isActive"
                  class="btn btn-sm">
                  {{ user.isActive ? 'Deactivate' : 'Activate' }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>

        <div class="no-data" *ngIf="users.length === 0 && !loading">
          No users found.
        </div>

        <div class="loading" *ngIf="loading">Loading users...</div>
      </div>

      <!-- Success/Error Messages -->
      <div class="alert alert-success" *ngIf="successMessage">{{ successMessage }}</div>
      <div class="alert alert-error" *ngIf="errorMessage">{{ errorMessage }}</div>
    </div>
  `,
  styles: [`
    .container {
      padding: 20px;
    }

    .header-section {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 30px;
      flex-wrap: wrap;
      gap: 20px;
    }

    h1 {
      color: var(--text-primary);
      margin: 0;
    }

    .stats {
      display: flex;
      gap: 20px;
    }

    .stat-item {
      padding: 8px 16px;
      background: var(--bg-secondary);
      color: var(--text-primary);
      border-radius: 6px;
      font-weight: 600;
      border: 1px solid var(--border-color);
    }

    .table-container {
      background: var(--bg-card);
      border-radius: 8px;
      overflow: hidden;
      border: 1px solid var(--border-color);
      box-shadow: var(--shadow-md);
    }

    .data-table {
      width: 100%;
      border-collapse: collapse;
      background: var(--bg-card);
      color: var(--text-primary);
    }

    .data-table th {
      background: var(--bg-secondary);
      color: var(--text-primary);
      padding: 15px 10px;
      text-align: left;
      font-weight: 600;
      border-bottom: 2px solid var(--border-color);
      white-space: nowrap;
    }

    .data-table td {
      padding: 12px 10px;
      border-bottom: 1px solid var(--border-color);
      color: var(--text-primary);
    }

    .data-table tbody tr:hover {
      background: var(--bg-hover);
    }

    .user-info {
      display: flex;
      flex-direction: column;
    }

    .user-info strong {
      color: var(--text-primary);
      margin-bottom: 4px;
    }

    .actions-cell {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }

    .btn-sm {
      padding: 6px 12px;
      font-size: 14px;
      white-space: nowrap;
    }

    .badge {
      padding: 4px 8px;
      border-radius: 4px;
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

    .badge-secondary {
      background: rgba(108, 117, 125, 0.1);
      color: var(--text-secondary);
      border: 1px solid var(--border-color);
    }

    .badge-admin {
      background: rgba(99, 102, 241, 0.1);
      color: var(--primary);
      border: 1px solid var(--primary);
    }

    .badge-user {
      background: rgba(6, 182, 212, 0.1);
      color: var(--secondary);
      border: 1px solid var(--secondary);
    }

    .no-data {
      padding: 40px;
      text-align: center;
      color: var(--text-secondary);
    }

    .loading {
      padding: 40px;
      text-align: center;
      color: var(--text-secondary);
    }

    @media (max-width: 1200px) {
      .table-container {
        overflow-x: auto;
      }

      .data-table {
        min-width: 1000px;
      }
    }

    @media (max-width: 768px) {
      .header-section {
        flex-direction: column;
        align-items: flex-start;
      }

      .stats {
        flex-wrap: wrap;
      }

      .actions-cell {
        flex-direction: column;
      }
    }
  `]
})
export class UserManagementComponent implements OnInit, OnDestroy {
  users: User[] = [];
  loading = false;
  errorMessage = '';
  successMessage = '';
  private destroy$ = new Subject<void>();
  private timeoutIds: any[] = [];

  constructor(
    private userService: UserService,
    private confirmationService: ConfirmationModalService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.errorMessage = '';

    this.userService.getAllUsers()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.users = result.items || [];
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = 'Failed to load users';
          this.loading = false;
          console.error('Error loading users:', error);
        }
      });
  }

  async toggleUserStatus(user: User): Promise<void> {
    const action = user.isActive ? 'deactivate' : 'activate';
    const confirmed = await this.confirmationService.confirmAction(
      `${action.charAt(0).toUpperCase() + action.slice(1)} User`,
      `Are you sure you want to ${action} user "${user.firstName} ${user.lastName}"?`
    );

    if (confirmed) {
      user.isActive = !user.isActive;
      this.toastService.success('Success', `User ${action}d successfully!`);
    }
  }

  getActiveCount(): number {
    return this.users.filter(u => u.isActive).length;
  }

  getAdminCount(): number {
    return this.users.filter(u => u.role === 'Admin').length;
  }

  getRoleBadgeClass(role: string): string {
    return role === 'Admin' ? 'badge badge-admin' : 'badge badge-user';
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.timeoutIds.forEach(id => clearTimeout(id));
  }
}
