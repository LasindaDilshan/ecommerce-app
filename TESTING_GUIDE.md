# E-Commerce Platform - UI Testing Guide

## Getting Started

### Access the Application
- **Frontend URL**: http://localhost:4200
- **Backend API**: http://localhost:5000
- **Swagger Documentation**: http://localhost:5000/swagger

### Login Credentials
See `LOGIN_CREDENTIALS.md` for complete credentials:
- **Admin**: admin@ecommerce.com / Admin@123
- **Regular User**: user@ecommerce.com / User@123

---

## Feature Testing Checklist

### 1. Authentication & Authorization

#### Registration
- [ ] Navigate to Register page
- [ ] Fill in registration form with valid data
- [ ] Verify email validation (proper email format)
- [ ] Verify password validation (min 8 chars, uppercase, lowercase, digit, special char)
- [ ] Verify password confirmation matching
- [ ] Submit and verify account creation
- [ ] Verify redirect to login page

#### Login
- [ ] Navigate to Login page
- [ ] Test login with admin credentials
- [ ] Test login with user credentials
- [ ] Test login with invalid credentials
- [ ] Verify JWT token is stored
- [ ] Verify redirect to home page after login

#### Logout
- [ ] Click logout button
- [ ] Verify redirect to home page
- [ ] Verify user is logged out (protected routes inaccessible)

---

### 2. Product Browsing (Public Access)

#### Product List Page
- [ ] View all products on home page
- [ ] Verify product images are displayed (using placeholder images)
- [ ] Verify product names, prices are shown
- [ ] Verify discounted prices are shown with strikethrough on original price
- [ ] Check pagination controls (if more than 12 products)

#### Product Search & Filtering
- [ ] Use search bar to search for products by name
- [ ] Filter products by category (Electronics, Clothing, Books, Home & Garden)
- [ ] Verify search results update correctly
- [ ] Verify filter results update correctly
- [ ] Test combining search and category filter

#### Product Detail Page
- [ ] Click on a product to view details
- [ ] Verify all product information is displayed:
  - [ ] Product name
  - [ ] Description
  - [ ] Price (with discount if applicable)
  - [ ] Stock quantity
  - [ ] SKU
  - [ ] Category
  - [ ] Multiple product images (image gallery)
- [ ] Verify you can navigate between product images
- [ ] Test "Add to Cart" button (as guest and logged-in user)

---

### 3. Shopping Cart

#### Guest Cart (Not Logged In)
- [ ] Add products to cart without logging in
- [ ] View cart page
- [ ] Verify cart items are stored in session
- [ ] Update product quantities
- [ ] Remove items from cart
- [ ] Verify cart total is calculated correctly

#### User Cart (Logged In)
- [ ] Login as regular user (user@ecommerce.com)
- [ ] Add products to cart
- [ ] Verify cart persists across sessions
- [ ] Update quantities
- [ ] Remove items
- [ ] Verify cart total with discounts

#### Cart Actions
- [ ] Apply discount code (if available)
- [ ] Verify discount is applied correctly
- [ ] Proceed to checkout

---

### 4. Wishlist (Authenticated Users Only)

- [ ] Login as regular user
- [ ] Navigate to product list
- [ ] Click heart icon on products to add to wishlist
- [ ] Verify heart icon changes color when in wishlist
- [ ] Navigate to wishlist page
- [ ] View all wishlist items
- [ ] Remove items from wishlist
- [ ] Add wishlist items to cart
- [ ] Verify wishlist persists across sessions

---

### 5. Checkout & Orders

#### Guest Checkout
- [ ] Add items to cart (without logging in)
- [ ] Proceed to checkout
- [ ] Fill in guest information (email, shipping address)
- [ ] Select payment method
- [ ] Complete order
- [ ] Receive order confirmation
- [ ] Note order tracking number

#### Authenticated Checkout
- [ ] Login as regular user
- [ ] Add items to cart
- [ ] Proceed to checkout
- [ ] Verify saved addresses are available (if any)
- [ ] Add new shipping address
- [ ] Select shipping address
- [ ] Select payment method
- [ ] Review order summary
- [ ] Place order
- [ ] Verify order confirmation page

#### Order Tracking
- [ ] Navigate to "Track Order" page
- [ ] Enter order ID or email
- [ ] View order status
- [ ] Verify order details are correct

#### Order History (Authenticated Users)
- [ ] Login as regular user
- [ ] Navigate to Profile > Orders
- [ ] View all past orders
- [ ] Click on an order to view details
- [ ] Verify order items, totals, status, and dates

---

### 6. User Profile & Account Management

#### Profile Page
- [ ] Login as regular user
- [ ] Navigate to Profile page
- [ ] View user information
- [ ] Update first name, last name
- [ ] Update phone number
- [ ] Save changes
- [ ] Verify changes are saved

#### Address Management
- [ ] Add new address
- [ ] Edit existing address
- [ ] Delete address
- [ ] Set default address

#### Change Password
- [ ] Navigate to change password
- [ ] Enter current password
- [ ] Enter new password
- [ ] Confirm new password
- [ ] Verify password is updated
- [ ] Logout and login with new password

#### Two-Factor Authentication (2FA)
- [ ] Navigate to 2FA settings
- [ ] Enable 2FA
- [ ] Scan QR code with authenticator app
- [ ] Enter verification code
- [ ] Save recovery codes
- [ ] Logout and login with 2FA
- [ ] Test 2FA code validation
- [ ] Disable 2FA

---

### 7. Product Reviews & Ratings

#### Add Review
- [ ] Login as regular user
- [ ] Navigate to a product detail page
- [ ] Submit a product review with rating (1-5 stars)
- [ ] Add review comment
- [ ] Submit review
- [ ] Verify review appears on product page

#### View Reviews
- [ ] View all reviews for a product
- [ ] Verify average rating is calculated correctly
- [ ] Verify review author and date are shown

#### Edit/Delete Review
- [ ] Edit your own review
- [ ] Update rating or comment
- [ ] Save changes
- [ ] Delete your review

---

### 8. Admin Dashboard (Admin Only)

#### Access Dashboard
- [ ] Login as admin (admin@ecommerce.com)
- [ ] Verify "Admin Dashboard" link is visible in navigation
- [ ] Navigate to admin dashboard
- [ ] Verify dashboard statistics are displayed:
  - [ ] Total revenue
  - [ ] Total orders
  - [ ] Total products
  - [ ] Total users

#### Product Management
- [ ] Navigate to Product Management
- [ ] View all products in a table
- [ ] **Add New Product**:
  - [ ] Click "Add Product" button
  - [ ] Fill in product details (name, description, price, stock, SKU, category)
  - [ ] **Upload product images** (single or multiple)
  - [ ] Set featured product flag
  - [ ] Submit and verify product is created
  - [ ] Verify uploaded images are displayed
- [ ] **Edit Product**:
  - [ ] Click edit on a product
  - [ ] Update product information
  - [ ] Add/remove product images
  - [ ] Save changes
- [ ] **Delete Product**:
  - [ ] Delete a product
  - [ ] Confirm deletion
- [ ] Search and filter products

#### Category Management
- [ ] Navigate to Category Management
- [ ] View all categories
- [ ] Add new category
- [ ] Edit category
- [ ] Delete category (if no products assigned)
- [ ] Activate/Deactivate category

#### User Management
- [ ] Navigate to User Management
- [ ] View all registered users
- [ ] Search users by email or name
- [ ] View user details
- [ ] Activate/Deactivate user accounts
- [ ] View user order history

#### Discount Code Management
- [ ] Navigate to Discount Codes
- [ ] View all discount codes
- [ ] **Create Discount Code**:
  - [ ] Enter code name
  - [ ] Set discount type (Percentage/Fixed)
  - [ ] Set discount value
  - [ ] Set usage limits
  - [ ] Set expiration date
  - [ ] Save discount code
- [ ] Edit discount code
- [ ] Delete discount code
- [ ] Test applying discount code in checkout

#### Order Management
- [ ] Navigate to Order Management
- [ ] View all orders
- [ ] Filter orders by status (Pending, Processing, Shipped, Delivered, Cancelled)
- [ ] Click on an order to view details
- [ ] Update order status
- [ ] View customer information
- [ ] View order items and totals

#### Inventory Management
- [ ] Navigate to Inventory Management
- [ ] View stock levels for all products
- [ ] Update stock quantities
- [ ] Set low stock alerts
- [ ] View out-of-stock products
- [ ] Restock products

---

### 9. Advanced Features

#### Subscription Management (If Implemented)
- [ ] Login as user
- [ ] Subscribe to newsletter
- [ ] Manage subscription preferences
- [ ] Unsubscribe

#### Advanced Search
- [ ] Test search with multiple keywords
- [ ] Search by product attributes
- [ ] Filter by price range
- [ ] Sort products (price, name, date added)

#### Performance Testing
- [ ] Test page load times
- [ ] Test with multiple products in cart
- [ ] Test pagination with many products
- [ ] Test image loading

---

## API Testing (Optional)

### Using Swagger UI
1. Navigate to http://localhost:5000/swagger
2. Explore all available API endpoints
3. Test endpoints directly from Swagger UI
4. For authenticated endpoints:
   - First call `/api/Auth/login` with credentials
   - Copy the JWT token from response
   - Click "Authorize" button in Swagger
   - Enter: `Bearer {your-token}`
   - Test protected endpoints

---

## Image Management Testing

### Product Images
- [ ] **Admin adds product with single image**:
  - [ ] Upload image file (JPG, PNG, GIF)
  - [ ] Verify image preview before submission
  - [ ] Submit product
  - [ ] Verify image is displayed on product list
  - [ ] Verify image is displayed on product detail page

- [ ] **Admin adds product with multiple images**:
  - [ ] Upload multiple images
  - [ ] Set one as primary image
  - [ ] Verify all images are uploaded
  - [ ] View product detail page
  - [ ] Verify image gallery with all images
  - [ ] Navigate between images

- [ ] **Edit product images**:
  - [ ] Edit existing product
  - [ ] Add new images
  - [ ] Remove existing images
  - [ ] Change primary image
  - [ ] Save and verify changes

- [ ] **Image validation**:
  - [ ] Try uploading invalid file type (e.g., .txt, .pdf)
  - [ ] Try uploading file larger than 5MB
  - [ ] Verify error messages

---

## Known Limitations & Notes

1. **Email Functionality**: SendGrid API key needs to be configured for email features
2. **Payment Integration**: Stripe integration requires valid API keys
3. **Image Storage**: Currently using placeholder images from via.placeholder.com
4. **Real Image Upload**: To test actual image uploads, admin needs to upload images which will be stored in `Backend/wwwroot/uploads`

---

## Testing Tips

1. **Test in Different Browsers**: Chrome, Firefox, Edge, Safari
2. **Test Responsive Design**: Resize browser window, test on mobile
3. **Test Edge Cases**:
   - Empty cart
   - Out of stock products
   - Invalid coupon codes
   - Maximum quantity limits
4. **Test Error Handling**:
   - Network errors
   - Validation errors
   - Server errors
5. **Test Security**:
   - Try accessing admin pages as regular user
   - Try accessing other users' data
   - Test XSS and SQL injection (should be prevented)

---

## Reporting Issues

When you find an issue, please note:
1. Steps to reproduce
2. Expected behavior
3. Actual behavior
4. Screenshots if applicable
5. Browser and OS version
6. User account used (admin/user)

---

## Summary of Key Features to Test

✅ User Authentication (Register, Login, Logout)
✅ Product Browsing (List, Search, Filter, Details)
✅ Shopping Cart (Guest & User)
✅ Wishlist
✅ Checkout & Orders
✅ Order Tracking
✅ Product Reviews & Ratings
✅ User Profile Management
✅ Two-Factor Authentication
✅ Admin Dashboard
✅ Product Management (with Image Upload)
✅ Category Management
✅ User Management
✅ Discount Code Management
✅ Order Management
✅ Inventory Management

**Happy Testing!** 🎉
