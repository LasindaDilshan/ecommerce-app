# E-Commerce Backend API - Testing Guide

## ✅ System Status

### Database
- **PostgreSQL**: ✅ Running (Docker container)
  - Host: localhost
  - Port: 5432
  - Database: ecommerce_db
  - Status: Healthy

### Backend API
- **Status**: ✅ Running
- **URL**: http://localhost:5000
- **Swagger Documentation**: http://localhost:5000/swagger
- **Environment**: Development

---

## 🔐 Test Credentials

### Admin User
```
Email: admin@ecommerce.com
Password: Admin123!
```

### Regular User
```
Email: user@ecommerce.com
Password: User123!
```

---

## 📋 Available Endpoints

### Authentication (`/api/auth`)
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Logout user
- `GET /api/auth/profile` - Get user profile (requires auth)
- `PUT /api/auth/profile` - Update user profile (requires auth)
- `POST /api/auth/change-password` - Change password (requires auth)
- `POST /api/auth/forgot-password` - Request password reset
- `GET /api/auth/verify-email` - Verify email address

### Products (`/api/products`)
- `GET /api/products` - Get all products (with pagination)
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create product (Admin only)
- `PUT /api/products/{id}` - Update product (Admin only)
- `DELETE /api/products/{id}` - Delete product (Admin only)
- `GET /api/products/category/{categoryId}` - Get products by category
- `GET /api/products/featured` - Get featured products
- `GET /api/products/search` - Search products

### Categories (`/api/categories`)
- `GET /api/categories` - Get all categories
- `GET /api/categories/{id}` - Get category by ID
- `POST /api/categories` - Create category (Admin only)
- `PUT /api/categories/{id}` - Update category (Admin only)
- `DELETE /api/categories/{id}` - Delete category (Admin only)

### Cart (`/api/cart`)
- `GET /api/cart` - Get user cart (requires auth)
- `POST /api/cart/items` - Add item to cart (requires auth)
- `PUT /api/cart/items/{id}` - Update cart item (requires auth)
- `DELETE /api/cart/items/{id}` - Remove item from cart (requires auth)
- `DELETE /api/cart` - Clear cart (requires auth)
- `POST /api/cart/coupon` - Apply coupon code (requires auth)
- `DELETE /api/cart/coupon` - Remove coupon (requires auth)

### Orders (`/api/orders`)
- `GET /api/orders` - Get user orders (requires auth)
- `GET /api/orders/{id}` - Get order by ID (requires auth)
- `POST /api/orders` - Create order from cart (requires auth)
- `PUT /api/orders/{id}/cancel` - Cancel order (requires auth)
- `GET /api/admin/orders` - Get all orders (Admin only)
- `PUT /api/admin/orders/{id}/status` - Update order status (Admin only)

### Reviews (`/api/reviews`)
- `GET /api/reviews/product/{productId}` - Get product reviews
- `POST /api/reviews` - Create review (requires auth)
- `PUT /api/reviews/{id}` - Update review (requires auth)
- `DELETE /api/reviews/{id}` - Delete review (requires auth)

### Wishlist (`/api/wishlist`)
- `GET /api/wishlist` - Get user wishlist (requires auth)
- `POST /api/wishlist` - Add item to wishlist (requires auth)
- `DELETE /api/wishlist/{productId}` - Remove from wishlist (requires auth)

### Subscriptions (`/api/subscriptions`)
- `GET /api/subscriptions` - Get user subscriptions (requires auth)
- `POST /api/subscriptions` - Create subscription (requires auth)
- `PUT /api/subscriptions/{id}/cancel` - Cancel subscription (requires auth)

### Discount Codes (`/api/discountcodes`)
- `POST /api/discountcodes/validate` - Validate discount code
- `GET /api/admin/discountcodes` - Get all discount codes (Admin only)
- `POST /api/admin/discountcodes` - Create discount code (Admin only)

### Dashboard (Admin) (`/api/dashboard`)
- `GET /api/dashboard/stats` - Get dashboard statistics (Admin only)
- `GET /api/dashboard/sales` - Get sales data (Admin only)

---

## 🧪 Sample Data in Database

### Categories (4 total)
1. Electronics
2. Clothing
3. Books
4. Home & Garden

### Products (5 total)
1. Laptop Pro 15 - $1,299.99 (Electronics)
2. Wireless Mouse - $29.99 (Electronics)
3. Cotton T-Shirt - $19.99 (Clothing)
4. Programming Guide - $49.99 (Books)
5. LED Desk Lamp - $39.99 (Home & Garden)

---

## 🔧 Testing from Frontend

### 1. Configure Your Frontend
Update your Angular/React frontend API base URL to:
```typescript
const API_BASE_URL = 'http://localhost:5000';
```

### 2. Test Authentication Flow
```typescript
// Login
POST http://localhost:5000/api/auth/login
Body: {
  "email": "admin@ecommerce.com",
  "password": "Admin123!"
}

// Response will include:
{
  "userId": 1,
  "email": "admin@ecommerce.com",
  "firstName": "Admin",
  "lastName": "User",
  "role": "Admin",
  "accessToken": "eyJhbGc...",
  "refreshToken": "...",
  "expiresAt": "2025-11-08T...",
  "emailVerified": true
}
```

### 3. Use Access Token
Include the access token in all authenticated requests:
```typescript
Headers: {
  "Authorization": "Bearer YOUR_ACCESS_TOKEN"
}
```

### 4. Test Shopping Flow
1. Browse products: `GET /api/products`
2. Add to cart: `POST /api/cart/items`
3. View cart: `GET /api/cart`
4. Create order: `POST /api/orders`

---

## 📊 Features Implemented & Tested

### Core Features
- ✅ User Authentication (JWT)
- ✅ Product Management (CRUD)
- ✅ Shopping Cart
- ✅ Order Management
- ✅ Categories
- ✅ Product Reviews
- ✅ Wishlist
- ✅ Discount Codes
- ✅ User Subscriptions

### Advanced Features
- ✅ Role-based Authorization (Admin/User)
- ✅ Email Verification
- ✅ Password Reset
- ✅ Refresh Tokens
- ✅ Rate Limiting
- ✅ CORS Configuration
- ✅ Global Exception Handling
- ✅ Logging (Serilog)
- ✅ API Documentation (Swagger)

### Testing
- ✅ Unit Tests (9/9 passing)
- ✅ Database Migrations
- ✅ Seed Data

---

## 🌐 CORS Configuration

Frontend origins allowed:
- http://localhost:4200 (Angular default)
- http://localhost:5173 (Vite/React default)

---

## 🛠️ Useful Commands

### Check if API is running
```bash
curl http://localhost:5000/api/products
```

### View Swagger Documentation
Open in browser: http://localhost:5000/swagger

### Check Database
```bash
docker exec ecommerce-postgres psql -U ecommerce -d ecommerce_db -c "SELECT * FROM \"Products\" LIMIT 5;"
```

### Stop Backend
```bash
# Find the dotnet process
tasklist | findstr dotnet
# Kill it
taskkill /F /PID <process_id>
```

### Restart PostgreSQL
```bash
docker restart ecommerce-postgres
```

---

## 📝 Notes

1. **Swagger UI**: Best way to test all endpoints interactively at http://localhost:5000/swagger
2. **Authentication**: Most endpoints require authentication. Get a token by logging in first.
3. **Admin Features**: Use admin@ecommerce.com to test admin-only features.
4. **Rate Limiting**: 100 requests per minute, 500 per 15 minutes, 1000 per hour.
5. **File Uploads**: Supported for product images (max 5MB, jpg/jpeg/png/gif).

---

## 🚀 Ready to Test!

Your backend is fully operational and ready for frontend integration. All features are working and the database is populated with sample data.

**Start testing from your frontend now!**
