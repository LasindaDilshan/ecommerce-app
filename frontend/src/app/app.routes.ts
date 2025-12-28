import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';
import { AdminGuard } from './guards/admin.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },

  // Public routes
  {
    path: 'home',
    loadComponent: () => import('./components/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'login',
    loadComponent: () => import('./components/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./components/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'products',
    loadComponent: () => import('./components/products/product-list/product-list.component').then(m => m.ProductListComponent)
  },
  {
    path: 'products/:id',
    loadComponent: () => import('./components/products/product-detail/product-detail.component').then(m => m.ProductDetailComponent)
  },
  {
    path: 'track-order',
    loadComponent: () => import('./components/track-order/track-order.component').then(m => m.TrackOrderComponent)
  },

  // Public cart and checkout (accessible to guests)
  {
    path: 'cart',
    loadComponent: () => import('./components/cart/cart.component').then(m => m.CartComponent)
  },
  {
    path: 'checkout',
    loadComponent: () => import('./components/checkout/checkout.component').then(m => m.CheckoutComponent)
  },
  {
    path: 'comparison',
    loadComponent: () => import('./components/comparison/comparison.component').then(m => m.ComparisonComponent)
  },
  {
    path: 'offline',
    loadComponent: () => import('./components/offline/offline.component').then(m => m.OfflineComponent)
  },

  // Protected routes (require authentication)
  {
    path: 'wishlist',
    loadComponent: () => import('./components/wishlist/wishlist.component').then(m => m.WishlistComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'orders',
    loadComponent: () => import('./components/orders/order-list/order-list.component').then(m => m.OrderListComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'orders/:id',
    loadComponent: () => import('./components/orders/order-detail/order-detail.component').then(m => m.OrderDetailComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'profile',
    loadComponent: () => import('./components/profile/profile.component').then(m => m.ProfileComponent),
    canActivate: [AuthGuard]
  },

  // Admin routes
  {
    path: 'admin',
    canActivate: [AdminGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./components/admin/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'products',
        loadComponent: () => import('./components/admin/product-management/product-management.component').then(m => m.ProductManagementComponent)
      },
      {
        path: 'categories',
        loadComponent: () => import('./components/admin/category-management/category-management.component').then(m => m.CategoryManagementComponent)
      },
      {
        path: 'orders',
        loadComponent: () => import('./components/admin/order-management/order-management.component').then(m => m.OrderManagementComponent)
      },
      {
        path: 'users',
        loadComponent: () => import('./components/admin/user-management/user-management.component').then(m => m.UserManagementComponent)
      },
      {
        path: 'discounts',
        loadComponent: () => import('./components/admin/discount-management/discount-management.component').then(m => m.DiscountManagementComponent)
      },
      {
        path: 'inventory',
        loadComponent: () => import('./components/admin/inventory-management/inventory-management.component').then(m => m.InventoryManagementComponent)
      }
    ]
  },

  // Wildcard route
  { path: '**', redirectTo: '/home' }
];
