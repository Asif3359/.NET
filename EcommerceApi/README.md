# Ecommerce API - Clean Architecture Implementation

A professional, production-ready ASP.NET Core Web API implementing clean architecture principles with Repository pattern, Service layer, JWT authentication, and comprehensive error handling.

## 🏗️ Architecture Overview

This project follows **Clean Architecture** principles with clear separation of concerns:

```
EcommerceApi/
├── Controllers/          # API endpoints (thin controllers)
├── Services/            # Business logic layer
├── Repositories/        # Data access layer
├── Interfaces/          # Contracts/Abstractions
├── Models/              # Domain entities
├── DTOs/                # Data Transfer Objects
├── Data/                # Database context
├── Helpers/             # Utility classes
├── Middleware/          # Custom middleware
├── Enums/               # Enumerations
└── Migrations/          # EF Core migrations
```

## 🎯 Key Features

### ✅ Clean Architecture
- **Repository Pattern** - Abstraction over data access
- **Unit of Work** - Transaction management
- **Service Layer** - Business logic separation
- **Dependency Injection** - Loose coupling

### 🔐 Security
- **JWT Authentication** - Token-based auth (not cookies)
- **Role-Based Authorization** - Admin/User policies
- **Password Hashing** - BCrypt implementation
- **CORS Configuration** - Cross-origin support

### 🛡️ Error Handling
- **Global Exception Middleware** - Centralized error handling
- **Consistent API Responses** - Standardized response format
- **Validation** - Data annotation validation
- **Detailed Logging** - Request/response logging

### 📊 Data Management
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **Migrations** - Version control for schema
- **Eager Loading** - Performance optimization

## 🚀 Getting Started

### Prerequisites
- .NET 10.0 SDK
- SQL Server (or Docker)
- Your favorite IDE (VS Code, Rider, Visual Studio)

### Installation

1. **Clone and navigate to the project**
   ```bash
   cd EcommerceApi
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Start SQL Server (using Docker)**
   ```bash
   docker-compose up -d
   ```

4. **Update database connection** (Optional)
   - For production, use User Secrets:
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
   dotnet user-secrets set "JwtSettings:Secret" "your-secret-key-at-least-32-characters"
   ```

5. **Apply migrations**
   ```bash
   dotnet ef database update
   ```

6. **Run the application**
   ```bash
   dotnet run
   ```

7. **Access Swagger UI**
   ```
   https://localhost:5001/swagger
   ```

## 📡 API Endpoints

### Authentication (`/api/auth`)
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/signup` | Register new user | No |
| POST | `/login` | Login user | No |
| GET | `/me` | Get current user | Yes |
| GET | `/{id}` | Get user by ID | Yes |
| GET | `/` | Get all users | Admin |
| DELETE | `/{id}` | Delete user | Admin |

### Products (`/api/product`)
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/` | Get all products | No |
| GET | `/{id}` | Get product by ID | No |
| GET | `/category/{categoryId}` | Get products by category | No |
| POST | `/` | Create product | Admin |
| PUT | `/{id}` | Update product | Admin |
| DELETE | `/{id}` | Delete product | Admin |

### Categories (`/api/category`)
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/` | Get all categories | No |
| GET | `/{id}` | Get category with products | No |
| POST | `/` | Create category | Admin |
| PUT | `/{id}` | Update category | Admin |
| DELETE | `/{id}` | Delete category | Admin |

### Orders (`/api/order`)
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/` | Get orders (all for Admin, own for User) | Yes |
| GET | `/{id}` | Get order by ID | Yes |
| POST | `/` | Create order | Yes |
| PUT | `/{id}/status` | Update order status | Admin |
| DELETE | `/{id}` | Delete order | Yes |

## 🔑 Authentication Flow

### 1. Signup
```http
POST /api/auth/signup
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "password123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Signup successful",
  "data": {
    "user": {
      "id": 1,
      "name": "John Doe",
      "email": "john@example.com",
      "role": "User"
    },
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

### 2. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "password123"
}
```

### 3. Using the Token
```http
GET /api/order
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## 📝 Code Structure Examples

### Repository Pattern
```csharp
// Interface
public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetProductsWithCategoryAsync();
    Task<Product?> GetProductWithCategoryAsync(long id);
}

// Implementation
public class ProductRepository : Repository<Product>, IProductRepository
{
    public async Task<Product?> GetProductWithCategoryAsync(long id)
    {
        return await _dbSet
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
```

### Service Layer
```csharp
public interface IProductService
{
    Task<(bool Success, string Message, ProductResponseDto? Product)> 
        CreateProductAsync(ProductDto dto);
}

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<(bool Success, string Message, ProductResponseDto? Product)> 
        CreateProductAsync(ProductDto dto)
    {
        // Business logic here
        // Validation, checking duplicates, etc.
        
        var product = new Product { /* ... */ };
        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        
        return (true, "Product created", productDto);
    }
}
```

### Controller (Thin Layer)
```csharp
[HttpPost]
[Authorize(Roles = "Admin")]
public async Task<ActionResult> PostProduct([FromBody] ProductDto dto)
{
    if (!ModelState.IsValid)
    {
        var errors = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();
        return BadRequest(ApiResponse.FailureResponse("Validation failed", errors));
    }

    var (success, message, product) = await _productService.CreateProductAsync(dto);

    if (!success)
        return BadRequest(ApiResponse.FailureResponse(message));

    return CreatedAtAction(nameof(GetProductById), 
        new { id = product!.Id }, 
        ApiResponse<ProductResponseDto>.SuccessResponse(product, message));
}
```

## 🧪 Testing the API

### Create a Category
```bash
curl -X POST https://localhost:5001/api/category \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name": "Electronics"}'
```

### Create a Product
```bash
curl -X POST https://localhost:5001/api/product \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "iPhone 15",
    "price": 999.99,
    "description": "Latest iPhone model",
    "categoryId": 1
  }'
```

### Create an Order
```bash
curl -X POST https://localhost:5001/api/order \
  -H "Authorization: Bearer YOUR_USER_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "shippingAddress": "123 Main St, City, State 12345",
    "orderItems": [
      {
        "productId": 1,
        "quantity": 2
      }
    ]
  }'
```

## 🗄️ Database Migrations

### Create a new migration
```bash
dotnet ef migrations add MigrationName
```

### Update database
```bash
dotnet ef database update
```

### Remove last migration
```bash
dotnet ef migrations remove
```

## 🔧 Configuration

### JWT Settings (appsettings.json)
```json
{
  "JwtSettings": {
    "Secret": "Your-Secret-Key-At-Least-32-Characters-Long",
    "Issuer": "EcommerceApi",
    "Audience": "EcommerceApiUsers",
    "ExpirationInMinutes": 1440
  }
}
```

**⚠️ Important:** Never commit secrets to source control! Use:
- User Secrets (Development)
- Environment Variables (Production)
- Azure Key Vault (Cloud)

## 📚 Learning Points

### 1. **Repository Pattern**
- Abstracts data access logic
- Makes code testable (can mock repositories)
- Centralizes data access logic

### 2. **Unit of Work**
- Manages transactions across multiple repositories
- Ensures data consistency
- Single SaveChanges() call

### 3. **Service Layer**
- Contains all business logic
- Controllers remain thin
- Reusable across different UI layers

### 4. **DTO Pattern**
- Separates domain models from API contracts
- Controls what data is exposed
- Adds validation layer

### 5. **JWT Authentication**
- Stateless (better for APIs)
- Works with mobile apps
- Scalable (no server-side sessions)

## 🚀 Production Checklist

Before deploying to production:

- [ ] Move secrets to environment variables/Key Vault
- [ ] Enable HTTPS (set `RequireHttpsMetadata = true`)
- [ ] Configure proper CORS policy (not AllowAll)
- [ ] Add rate limiting
- [ ] Set up application monitoring
- [ ] Configure proper logging (Serilog, App Insights)
- [ ] Add health checks
- [ ] Enable API versioning
- [ ] Set up CI/CD pipeline
- [ ] Database backup strategy
- [ ] Review security headers

## 📖 Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Repository Pattern](https://docs.microsoft.com/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application)

## 📄 License

This is a learning/practice project for educational purposes.

## 👨‍💻 Author

Created as a practice project to demonstrate professional API development patterns and best practices.

---

**Happy Learning! 🎓**
