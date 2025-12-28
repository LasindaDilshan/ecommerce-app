# Full-Stack E-Commerce Application

A production-ready e-commerce application built with .NET 8 Web API and Angular 17.

## Features

### User Features
- User registration and authentication with JWT
- Browse products with search, filter, and pagination
- Product details with image gallery
- Shopping cart management
- Checkout with shipping address
- Order history and tracking
- User profile management

### Admin Features
- Admin dashboard with statistics and analytics
- Product management (CRUD operations with image upload)
- Category management
- Order management (view and update order status)
- User management
- Revenue tracking and reports

### Technical Features
- JWT authentication with refresh tokens
- Role-based authorization (Admin/User)
- Entity Framework Core with SQL Server
- RESTful API architecture
- Payment integration ready (Stripe)
- Image upload functionality
- Responsive design
- Form validation
- Error handling and logging

## Technology Stack

### Backend
- .NET 8 Web API
- Entity Framework Core 8
- SQL Server
- JWT Authentication
- AutoMapper
- FluentValidation
- Serilog
- Stripe.NET
- BCrypt.Net

### Frontend
- Angular 17
- TypeScript
- SCSS
- RxJS
- Stripe.js

## Prerequisites

- .NET 8 SDK
- Node.js (v18 or higher)
- SQL Server (LocalDB, Express, or Full)
- Visual Studio 2022 or VS Code (optional)

## Getting Started

### Backend Setup

1. Navigate to the Backend folder:
```bash
cd Backend
```

2. Restore NuGet packages:
```bash
dotnet restore
```

3. Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EcommerceDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

4. Update JWT settings in `appsettings.json`:
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "EcommerceAPI",
    "Audience": "EcommerceClient",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  }
}
```

5. Create and apply database migrations:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

6. Run the backend API:
```bash
dotnet run
```

The API will be available at `https://localhost:5001` or `http://localhost:5000`

### Frontend Setup

1. Navigate to the Frontend folder:
```bash
cd Frontend
```

2. Install dependencies:
```bash
npm install
```

3. Update the API URL in `src/environments/environment.ts`:
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  stripePublishableKey: 'pk_test_your_stripe_publishable_key'
};
```

4. Run the Angular development server:
```bash
npm start
```

The application will be available at `http://localhost:4200`

## Default Credentials

The application seeds two default users:

### Admin User
- Email: `admin@ecommerce.com`
- Password: `Admin@123`

### Regular User
- Email: `user@ecommerce.com`
- Password: `User@123`

## Project Structure

### Backend Structure
```
Backend/
├── Controllers/          # API Controllers
├── Models/              # Entity models
├── DTOs/                # Data Transfer Objects
├── Services/            # Business logic services
├── Data/                # DbContext and database seeder
├── wwwroot/uploads/     # Uploaded files
├── appsettings.json     # Configuration
└── Program.cs           # Application entry point
```

### Frontend Structure
```
Frontend/
├── src/
│   ├── app/
│   │   ├── components/      # Angular components
│   │   │   ├── admin/       # Admin pages
│   │   │   ├── auth/        # Login/Register
│   │   │   ├── products/    # Product pages
│   │   │   ├── orders/      # Order pages
│   │   │   ├── cart/        # Shopping cart
│   │   │   ├── checkout/    # Checkout page
│   │   │   └── shared/      # Shared components
│   │   ├── services/        # Angular services
│   │   ├── models/          # TypeScript interfaces
│   │   ├── guards/          # Route guards
│   │   └── interceptors/    # HTTP interceptors
│   ├── environments/        # Environment config
│   └── styles.scss          # Global styles
```

## API Endpoints

### Authentication
- POST `/api/auth/register` - Register new user
- POST `/api/auth/login` - Login
- POST `/api/auth/refresh` - Refresh access token
- POST `/api/auth/revoke` - Revoke refresh token

### Products
- GET `/api/products` - Get products with filters
- GET `/api/products/featured` - Get featured products
- GET `/api/products/{id}` - Get product by ID
- POST `/api/products` - Create product (Admin)
- PUT `/api/products/{id}` - Update product (Admin)
- DELETE `/api/products/{id}` - Delete product (Admin)

### Categories
- GET `/api/categories` - Get all categories
- POST `/api/categories` - Create category (Admin)
- PUT `/api/categories/{id}` - Update category (Admin)
- DELETE `/api/categories/{id}` - Delete category (Admin)

### Cart
- GET `/api/cart` - Get user cart
- POST `/api/cart/add` - Add item to cart
- PUT `/api/cart/{cartItemId}` - Update cart item
- DELETE `/api/cart/{cartItemId}` - Remove from cart
- DELETE `/api/cart` - Clear cart

### Orders
- POST `/api/orders` - Create order
- GET `/api/orders` - Get user orders
- GET `/api/orders/{id}` - Get order by ID
- GET `/api/orders/all` - Get all orders (Admin)
- PUT `/api/orders/{id}/status` - Update order status (Admin)

### Users
- GET `/api/users/profile` - Get current user profile
- PUT `/api/users/profile` - Update profile
- PUT `/api/users/change-password` - Change password
- GET `/api/users` - Get all users (Admin)
- PUT `/api/users/{id}/role` - Update user role (Admin)

### Dashboard
- GET `/api/dashboard/stats` - Get dashboard statistics (Admin)

## Configuration

### Stripe Payment Integration

1. Get your Stripe API keys from https://dashboard.stripe.com/apikeys

2. Update backend `appsettings.json`:
```json
{
  "Stripe": {
    "SecretKey": "sk_test_your_stripe_secret_key",
    "PublishableKey": "pk_test_your_stripe_publishable_key"
  }
}
```

3. Update frontend `environment.ts`:
```typescript
stripePublishableKey: 'pk_test_your_stripe_publishable_key'
```

### File Upload Configuration

Configure file upload settings in `appsettings.json`:
```json
{
  "FileUpload": {
    "MaxFileSize": 5242880,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif"],
    "UploadPath": "wwwroot/uploads"
  }
}
```

## Database Migrations

### Create a new migration:
```bash
dotnet ef migrations add MigrationName
```

### Apply migrations:
```bash
dotnet ef database update
```

### Remove last migration:
```bash
dotnet ef migrations remove
```

## Testing

### Test the Backend API
Use the Swagger UI available at: `https://localhost:5001/swagger`

### Test Accounts
Login with the default credentials provided above to test different roles.

## Deployment

### Backend Deployment
1. Update `appsettings.json` with production values
2. Publish the application:
```bash
dotnet publish -c Release -o ./publish
```

### Frontend Deployment
1. Update `environment.prod.ts` with production values
2. Build for production:
```bash
npm run build
```

The output will be in the `dist/` folder.

## Security Considerations

- Change the JWT secret key in production
- Use HTTPS in production
- Store sensitive configuration in environment variables or Azure Key Vault
- Implement rate limiting
- Add input sanitization
- Enable CORS only for trusted origins
- Use strong passwords for database and admin accounts

## Future Enhancements

- Product reviews and ratings
- Wishlist functionality
- Email notifications
- Advanced search with Elasticsearch
- Product recommendations
- Multi-currency support
- Social media authentication
- Real-time order tracking
- Inventory management alerts
- Discount codes and coupons

## Troubleshooting

### Backend Issues
- **Database connection error**: Check connection string and SQL Server status
- **Migration error**: Delete migrations folder and recreate migrations
- **JWT error**: Verify secret key is at least 32 characters

### Frontend Issues
- **API connection error**: Check if backend is running and API URL is correct
- **CORS error**: Verify CORS configuration in backend `Program.cs`
- **Module not found**: Run `npm install` again

## License

This project is licensed under the MIT License.

## Support

For issues and questions, please open an issue on GitHub.
