# 🚀 Quick Start Guide

Get up and running with the Ecommerce API in 5 minutes!

## Prerequisites

- .NET 10.0 SDK installed
- Docker installed (for SQL Server)
- Git installed

## Step-by-Step Setup

### 1. Start the Database
```bash
cd /home/asifahammad/Documents/.NET/EcommerceApi
docker-compose up -d
```

**Verify it's running:**
```bash
docker ps
# You should see "sqlserver" container running
```

### 2. Restore Packages
```bash
dotnet restore
```

### 3. Apply Database Migrations
```bash
dotnet ef database update
```

**Expected Output:**
```
Build started...
Build succeeded.
Applying migration '20240101000000_InitialCreate'.
Done.
```

### 4. Run the Application
```bash
dotnet run
```

**You should see:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
```

### 5. Open Swagger UI
Open your browser and navigate to:
```
https://localhost:5001/swagger
```

---

## 🧪 Testing the API

### Step 1: Create an Admin User (First User)

Since there's no admin user yet, you'll need to manually create one in the database or modify the signup to make the first user an admin.

**Option A: Using SQL (Recommended for first admin)**

```bash
# Connect to SQL Server
docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C

# Switch to database
USE EcommerceApiDb;
GO

# Check existing users
SELECT * FROM Users;
GO

# Update first user to Admin (after you create one via API)
UPDATE Users SET Role = 1 WHERE Id = 1;
GO
```

**Option B: Temporary Code Change (For Development)**

Temporarily modify `AuthService.cs` for the first user:
```csharp
// In SignupAsync method, change:
Role = UserRole.User

// To:
Role = dto.Email.Contains("admin") ? UserRole.Admin : UserRole.User
```

Then signup with email containing "admin" (e.g., admin@test.com)

---

### Step 2: Complete API Testing Workflow

#### A. **Signup as a Regular User**

In Swagger UI, find `POST /api/auth/signup`:

**Request:**
```json
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
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8v..."
  }
}
```

**📋 Copy the `token` value!**

---

#### B. **Signup as Admin**

**Request:**
```json
{
  "name": "Admin User",
  "email": "admin@test.com",
  "password": "admin123"
}
```

Then update the user's role in the database as shown above, OR modify the code temporarily.

**Login to get fresh token:**
```json
{
  "email": "admin@test.com",
  "password": "admin123"
}
```

**📋 Copy the Admin `token` value!**

---

#### C. **Authorize in Swagger**

1. Click the **"Authorize"** button (🔓 icon) at the top of Swagger UI
2. Enter: `Bearer YOUR_TOKEN_HERE` (include the word "Bearer" followed by space and your token)
3. Click "Authorize"
4. Click "Close"

You're now authenticated! 🎉

---

#### D. **Create a Category (Admin Only)**

Find `POST /api/category` and try it:

**Request:**
```json
{
  "name": "Electronics"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Category created successfully",
  "data": {
    "id": 1,
    "name": "Electronics",
    "productCount": 0
  }
}
```

---

#### E. **Create More Categories**

```json
{"name": "Clothing"}
{"name": "Books"}
{"name": "Home & Garden"}
```

---

#### F. **Create a Product (Admin Only)**

Find `POST /api/product`:

**Request:**
```json
{
  "name": "iPhone 15 Pro",
  "price": 999.99,
  "description": "Latest flagship smartphone with advanced features",
  "categoryId": 1
}
```

**Response:**
```json
{
  "success": true,
  "message": "Product created successfully",
  "data": {
    "id": 1,
    "name": "iPhone 15 Pro",
    "price": 999.99,
    "description": "Latest flagship smartphone with advanced features",
    "categoryId": 1,
    "categoryName": "Electronics"
  }
}
```

---

#### G. **Create More Products**

```json
{
  "name": "MacBook Pro 16\"",
  "price": 2499.99,
  "description": "Powerful laptop for professionals",
  "categoryId": 1
}
```

```json
{
  "name": "Nike Air Max",
  "price": 129.99,
  "description": "Comfortable running shoes",
  "categoryId": 2
}
```

```json
{
  "name": "The Great Gatsby",
  "price": 12.99,
  "description": "Classic American novel",
  "categoryId": 3
}
```

---

#### H. **Get All Products (No Auth Required)**

Find `GET /api/product` - **Click "Try it out"** → **Execute**

You should see all products with their categories!

---

#### I. **Get Products by Category**

Find `GET /api/product/category/{categoryId}` - Try with `categoryId = 1`

You'll see only Electronics products.

---

#### J. **Create an Order (Authenticated User)**

**First, make sure you're authorized as a regular user (not admin).**

Find `POST /api/order`:

**Request:**
```json
{
  "shippingAddress": "123 Main Street, Apartment 4B, New York, NY 10001",
  "orderItems": [
    {
      "productId": 1,
      "quantity": 1
    },
    {
      "productId": 2,
      "quantity": 1
    }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "message": "Order created successfully",
  "data": {
    "id": 1,
    "orderNumber": "ORD-20260116-A3F5B2C8",
    "orderDate": "2026-01-16T10:30:00Z",
    "status": "Pending",
    "shippingAddress": "123 Main Street, Apartment 4B, New York, NY 10001",
    "userId": 1,
    "userName": "John Doe",
    "userEmail": "john@example.com",
    "orderItems": [
      {
        "productId": 1,
        "productName": "iPhone 15 Pro",
        "quantity": 1,
        "price": 999.99,
        "subtotal": 999.99
      },
      {
        "productId": 2,
        "productName": "MacBook Pro 16\"",
        "quantity": 1,
        "price": 2499.99,
        "subtotal": 2499.99
      }
    ],
    "totalAmount": 3499.98
  }
}
```

---

#### K. **Get My Orders**

Find `GET /api/order` - Execute

You'll see only **your** orders. Admin users see **all** orders.

---

#### L. **Update Order Status (Admin Only)**

**Switch to Admin token in Authorization**

Find `PUT /api/order/{id}/status`:

**Request:**
```json
{
  "status": "Processing"
}
```

Valid statuses: `Pending`, `Processing`, `Shipped`, `Delivered`, `Cancelled`

---

#### M. **Get Current User Info**

Find `GET /api/auth/me`:

```json
{
  "success": true,
  "message": "User retrieved successfully",
  "data": {
    "id": 1,
    "name": "John Doe",
    "email": "john@example.com",
    "role": "User"
  }
}
```

---

## 📊 Testing Different Scenarios

### Scenario 1: Validation Errors

Try creating a product with invalid data:
```json
{
  "name": "A",
  "price": -10,
  "description": "",
  "categoryId": 999
}
```

You should get validation errors!

### Scenario 2: Duplicate Prevention

Try creating a category with an existing name:
```json
{
  "name": "Electronics"
}
```

Should return: "Category with this name already exists"

### Scenario 3: Authorization

1. Log out (remove Bearer token from Authorization)
2. Try to create a product
3. Should get: **401 Unauthorized**

### Scenario 4: Forbidden Access

1. Login as regular User
2. Try to create a product (Admin only)
3. Should get: **403 Forbidden**

### Scenario 5: Not Found

Try to get a product that doesn't exist:
`GET /api/product/9999`

Should return: **404 Not Found**

---

## 🔍 Debugging Tips

### Check Logs

Watch the console where `dotnet run` is running. You'll see:
- Incoming requests
- SQL queries (if logging level is Information)
- Errors and exceptions

### Check Database

```bash
docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C

USE EcommerceApiDb;
GO

-- View all tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';
GO

-- View data
SELECT * FROM Users;
SELECT * FROM Categories;
SELECT * FROM Products;
SELECT * FROM Orders;
SELECT * FROM OrderItems;
GO
```

### Common Issues

**Issue: "Cannot connect to database"**
```bash
# Check if SQL Server is running
docker ps

# If not running, start it
docker-compose up -d

# Check logs
docker logs sqlserver
```

**Issue: "Migration not found"**
```bash
# List migrations
dotnet ef migrations list

# Add migration if missing
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update
```

**Issue: "401 Unauthorized"**
- Check if token is valid (not expired)
- Ensure you added "Bearer " before the token
- Token expires after 24 hours (1440 minutes)

---

## 🧹 Clean Up

### Stop the Application
Press `Ctrl + C` in the terminal where `dotnet run` is running

### Stop Docker Container
```bash
docker-compose down
```

### Reset Database (Start Fresh)
```bash
# Stop containers
docker-compose down

# Remove volume (deletes all data)
docker volume rm ecommerceapi_sqlserver_data

# Start again
docker-compose up -d

# Recreate database
dotnet ef database update
```

---

## 🎯 Next Steps

1. ✅ **Explored all endpoints** - You've tested the API!
2. 📚 **Read WORKFLOW_GUIDE.md** - Learn the development workflow
3. 🏗️ **Add a new feature** - Try adding Reviews or Wishlists
4. 🧪 **Write unit tests** - Create tests for services
5. 🚀 **Deploy** - Try deploying to Azure or AWS

---

## 📞 Common Commands Reference

```bash
# Build project
dotnet build

# Run project
dotnet run

# Run with watch (auto-restart on changes)
dotnet watch run

# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Restore packages
dotnet restore

# Clean build artifacts
dotnet clean
```

---

**Happy Testing! 🎉**

Remember: This is a **learning project**. Experiment, break things, and learn!
