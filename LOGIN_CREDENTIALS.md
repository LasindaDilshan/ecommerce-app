# Login Credentials for E-Commerce Demo

## 🚀 Quick Start

1. **Open Frontend**: http://localhost:4200
2. **Login as Admin** using credentials below
3. **Navigate to Product Management** to add/edit/delete products
4. All changes are **SAVED TO DATABASE** immediately

---

## 🌐 Services Running

### Frontend
- **URL**: http://localhost:4200
- **Features**: Shopping, Cart, Wishlist, Admin Dashboard

### Backend API
- **URL**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/Health

### Database
- **PostgreSQL**: localhost:5432
- **Database Name**: ecommerce_db
- **PgAdmin**: http://localhost:5050

---

## 🔑 User Accounts

### 👨‍💼 Admin Account (FULL ACCESS)
```
Email:    admin@ecommerce.com
Password: Admin@123
```
**What You Can Do:**
- ✅ Add/Edit/Delete Products (saves to database)
- ✅ Manage Categories
- ✅ Manage Users
- ✅ Create Discount Codes
- ✅ View Orders & Update Status
- ✅ Manage Inventory
- ✅ View Dashboard Analytics

### 👤 Regular User Account
```
Email:    user@ecommerce.com
Password: User@123
```
**What You Can Do:**
- ✅ Browse & Search Products
- ✅ Add to Cart & Wishlist
- ✅ Place Orders
- ✅ Write Product Reviews
- ✅ Manage Profile & Addresses
- ✅ Track Orders

---

## 📝 How to Test Product Management

### Adding Products (Admin Only)
1. **Login as admin** (admin@ecommerce.com / Admin@123)
2. Click **"Admin Dashboard"** in navigation
3. Click **"Product Management"**
4. Click **"Add Product"** button
5. Fill in product details:
   - **Name**: e.g., "iPhone 15 Pro"
   - **SKU**: e.g., "IPH-15-PRO-001"
   - **Description**: Product details
   - **Price**: e.g., 999.99
   - **Discount Price**: (optional) e.g., 899.99
   - **Stock**: e.g., 50
   - **Category**: Select from dropdown
   - **Image URL**: Use placeholder like:
     `https://via.placeholder.com/600x400/FF6B6B/FFFFFF?text=iPhone+15+Pro`
   - Check "Featured" to show on homepage
   - Check "Active" to make visible to users
6. Click **"Save Product"**
7. ✅ **Product is SAVED TO DATABASE**
8. Product will appear in the table below

### Editing Products
1. In Product Management table, click **"Edit"** button
2. Form appears with existing product data
3. Modify any fields
4. Click **"Save Product"**
5. ✅ **Changes are SAVED TO DATABASE**

### Deleting Products
1. Click **"Delete"** button next to product
2. Confirm deletion
3. ✅ **Product is REMOVED FROM DATABASE**
4. Table updates automatically

## Database Access (PgAdmin)
- **URL**: http://localhost:5050
- **Email**: admin@ecommerce.com
- **Password**: admin
- **Server Connection**:
  - Host: ecommerce-postgres (or localhost)
  - Port: 5432
  - Database: ecommerce_db
  - Username: ecommerce
  - Password: SecurePassword123!

## Demo Data

The database has been seeded with:

### Categories
1. Electronics
2. Clothing
3. Books
4. Home & Garden

### Products (with multiple images each)
1. **Laptop Pro 15** - $1,299.99 (Discounted: $1,199.99)
2. **Wireless Mouse** - $29.99
3. **Men's T-Shirt** - $19.99 (Discounted: $14.99)
4. **Programming Guide Book** - $49.99
5. **LED Desk Lamp** - $39.99

All products have multiple product images using placeholder images from via.placeholder.com

## Notes
- Two-factor authentication is disabled by default for demo accounts
- Email verification is not required for demo accounts
- All products have stock available
- Images are served from external placeholder service (via.placeholder.com)
