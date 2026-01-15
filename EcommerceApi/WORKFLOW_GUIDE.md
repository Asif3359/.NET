# Official Development Workflow Guide

This guide explains the **professional workflow** you should follow when building real-world .NET APIs.

## 🎯 Table of Contents

1. [Project Setup Workflow](#1-project-setup-workflow)
2. [Feature Development Workflow](#2-feature-development-workflow)
3. [Code Organization Principles](#3-code-organization-principles)
4. [Best Practices](#4-best-practices)
5. [Common Patterns](#5-common-patterns)
6. [Testing Strategy](#6-testing-strategy)

---

## 1. Project Setup Workflow

### Step 1: Create Project Structure

```bash
# Create the project
dotnet new webapi -n YourApiName

# Create folder structure
mkdir Models DTOs Controllers Services Repositories Interfaces Data Helpers Middleware Enums
```

### Step 2: Install Required Packages

```bash
# Database
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design

# Authentication
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package BCrypt.Net-Next

# Documentation
dotnet add package NSwag.AspNetCore

# Optional: Logging
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
```

### Step 3: Configure Git

```bash
git init
echo "appsettings.Development.json" >> .gitignore
echo "*.user" >> .gitignore
git add .
git commit -m "Initial project setup"
```

---

## 2. Feature Development Workflow

### Complete Workflow for Adding a New Feature

Let's say you want to add a "Review" feature. Here's the **exact step-by-step process**:

#### **STEP 1: Create the Domain Model**

📁 `Models/Review.cs`
```csharp
public class Review
{
    public long Id { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int Rating { get; set; }  // 1-5
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

#### **STEP 2: Create DTOs**

📁 `DTOs/ReviewDto.cs`
```csharp
public class ReviewDto
{
    [Required(ErrorMessage = "Comment is required")]
    [StringLength(500, MinimumLength = 10)]
    public string Comment { get; set; } = string.Empty;

    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [Required]
    public long ProductId { get; set; }
}
```

📁 `DTOs/ReviewResponseDto.cs`
```csharp
public class ReviewResponseDto
{
    public long Id { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int Rating { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

#### **STEP 3: Update DbContext**

📁 `Data/AppDbContext.cs`
```csharp
public DbSet<Review> Reviews { get; set; }

protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    
    builder.Entity<Review>()
        .HasOne(r => r.Product)
        .WithMany(p => p.Reviews)
        .HasForeignKey(r => r.ProductId)
        .OnDelete(DeleteBehavior.Cascade);
        
    builder.Entity<Review>()
        .HasOne(r => r.User)
        .WithMany()
        .HasForeignKey(r => r.UserId)
        .OnDelete(DeleteBehavior.Restrict);
}
```

#### **STEP 4: Create Migration**

```bash
dotnet ef migrations add AddReviewEntity
dotnet ef database update
```

#### **STEP 5: Create Repository Interface**

📁 `Interfaces/IReviewRepository.cs`
```csharp
public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetReviewsByProductIdAsync(long productId);
    Task<IEnumerable<Review>> GetReviewsByUserIdAsync(long userId);
    Task<Review?> GetReviewWithDetailsAsync(long id);
    Task<bool> UserHasReviewedProductAsync(long userId, long productId);
    Task<double> GetAverageRatingForProductAsync(long productId);
}
```

#### **STEP 6: Implement Repository**

📁 `Repositories/ReviewRepository.cs`
```csharp
public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(long productId)
    {
        return await _dbSet
            .Include(r => r.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review?> GetReviewWithDetailsAsync(long id)
    {
        return await _dbSet
            .Include(r => r.Product)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> UserHasReviewedProductAsync(long userId, long productId)
    {
        return await _dbSet.AnyAsync(r => r.UserId == userId && r.ProductId == productId);
    }

    public async Task<double> GetAverageRatingForProductAsync(long productId)
    {
        var reviews = await _dbSet
            .Where(r => r.ProductId == productId)
            .ToListAsync();
            
        return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
    }
}
```

#### **STEP 7: Update Unit of Work**

📁 `Interfaces/IUnitOfWork.cs`
```csharp
public interface IUnitOfWork : IDisposable
{
    // ... existing repositories
    IReviewRepository Reviews { get; }
    // ...
}
```

📁 `Repositories/UnitOfWork.cs`
```csharp
public IReviewRepository Reviews { get; }

public UnitOfWork(AppDbContext context)
{
    _context = context;
    // ... existing
    Reviews = new ReviewRepository(_context);
}
```

#### **STEP 8: Create Service Interface**

📁 `Interfaces/IReviewService.cs`
```csharp
public interface IReviewService
{
    Task<IEnumerable<ReviewResponseDto>> GetReviewsByProductIdAsync(long productId);
    Task<ReviewResponseDto?> GetReviewByIdAsync(long id);
    Task<(bool Success, string Message, ReviewResponseDto? Review)> CreateReviewAsync(ReviewDto dto, long userId);
    Task<(bool Success, string Message)> UpdateReviewAsync(long id, ReviewDto dto, long userId);
    Task<(bool Success, string Message)> DeleteReviewAsync(long id, long userId, UserRole userRole);
    Task<double> GetAverageRatingAsync(long productId);
}
```

#### **STEP 9: Implement Service**

📁 `Services/ReviewService.cs`
```csharp
public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(IUnitOfWork unitOfWork, ILogger<ReviewService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, ReviewResponseDto? Review)> 
        CreateReviewAsync(ReviewDto dto, long userId)
    {
        try
        {
            // Validate product exists
            var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
            if (product == null)
                return (false, "Product not found", null);

            // Check if user already reviewed this product
            if (await _unitOfWork.Reviews.UserHasReviewedProductAsync(userId, dto.ProductId))
                return (false, "You have already reviewed this product", null);

            var review = new Review
            {
                Comment = dto.Comment,
                Rating = dto.Rating,
                ProductId = dto.ProductId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            var createdReview = await _unitOfWork.Reviews.GetReviewWithDetailsAsync(review.Id);

            var reviewDto = MapToDto(createdReview!);

            _logger.LogInformation("Review created for product {ProductId} by user {UserId}", 
                dto.ProductId, userId);

            return (true, "Review created successfully", reviewDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating review");
            throw;
        }
    }

    private ReviewResponseDto MapToDto(Review review)
    {
        return new ReviewResponseDto
        {
            Id = review.Id,
            Comment = review.Comment,
            Rating = review.Rating,
            ProductId = review.ProductId,
            ProductName = review.Product?.Name ?? "",
            UserId = review.UserId,
            UserName = review.User?.Name ?? "",
            CreatedAt = review.CreatedAt
        };
    }
}
```

#### **STEP 10: Register Service in Program.cs**

📁 `Program.cs`
```csharp
builder.Services.AddScoped<IReviewService, ReviewService>();
```

#### **STEP 11: Create Controller**

📁 `Controllers/ReviewController.cs`
```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("product/{productId}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetProductReviews(long productId)
    {
        var reviews = await _reviewService.GetReviewsByProductIdAsync(productId);
        return Ok(ApiResponse<IEnumerable<ReviewResponseDto>>
            .SuccessResponse(reviews, "Reviews retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult> CreateReview([FromBody] ReviewDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse.FailureResponse("Validation failed", errors));
        }

        var userId = GetCurrentUserId();
        var (success, message, review) = await _reviewService.CreateReviewAsync(dto, userId);

        if (!success)
            return BadRequest(ApiResponse.FailureResponse(message));

        return CreatedAtAction(nameof(GetReview), 
            new { id = review!.Id }, 
            ApiResponse<ReviewResponseDto>.SuccessResponse(review, message));
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
```

#### **STEP 12: Test the API**

1. Start the application
2. Use Swagger UI to test
3. Test authentication flow
4. Test CRUD operations
5. Test edge cases

---

## 3. Code Organization Principles

### Folder Structure
```
YourApi/
├── Controllers/         # Thin layer - only handles HTTP
├── Services/           # Business logic - 80% of your code
├── Repositories/       # Data access - EF Core queries
├── Interfaces/         # Contracts for DI
├── Models/             # Database entities
├── DTOs/               # API request/response models
│   ├── Requests/       # (Optional) Separate request DTOs
│   └── Responses/      # (Optional) Separate response DTOs
├── Data/               # DbContext and configurations
├── Helpers/            # Utilities (ApiResponse, Pagination, etc.)
├── Middleware/         # Custom middleware
├── Enums/              # Enumerations
└── Extensions/         # Extension methods
```

### Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| Interface | I{Name} | `IProductService` |
| Implementation | {Name} | `ProductService` |
| DTO | {Entity}Dto | `ProductDto` |
| Response DTO | {Entity}ResponseDto | `ProductResponseDto` |
| Controller | {Entity}Controller | `ProductController` |
| Repository | {Entity}Repository | `ProductRepository` |
| Async Methods | {Verb}Async | `GetProductAsync` |

---

## 4. Best Practices

### ✅ DO

1. **Keep Controllers Thin**
   ```csharp
   // Good
   [HttpPost]
   public async Task<ActionResult> CreateProduct([FromBody] ProductDto dto)
   {
       if (!ModelState.IsValid)
           return BadRequest(ModelState);
           
       var (success, message, product) = await _productService.CreateProductAsync(dto);
       
       if (!success)
           return BadRequest(ApiResponse.FailureResponse(message));
           
       return Ok(ApiResponse<ProductResponseDto>.SuccessResponse(product, message));
   }
   ```

2. **Put Business Logic in Services**
   ```csharp
   // Good - Service handles validation and business rules
   public async Task<(bool, string, ProductResponseDto?)> CreateProductAsync(ProductDto dto)
   {
       if (await _unitOfWork.Products.ProductNameExistsAsync(dto.Name))
           return (false, "Product already exists", null);
           
       if (dto.Price < 0)
           return (false, "Price cannot be negative", null);
           
       // ... create product
   }
   ```

3. **Use DTOs for All API Communication**
   ```csharp
   // Good - Never expose domain models directly
   [HttpGet("{id}")]
   public async Task<ActionResult<ProductResponseDto>> GetProduct(long id)
   {
       var product = await _service.GetProductByIdAsync(id);
       return Ok(ApiResponse<ProductResponseDto>.SuccessResponse(product));
   }
   ```

4. **Always Validate Input**
   ```csharp
   public class ProductDto
   {
       [Required(ErrorMessage = "Name is required")]
       [StringLength(100, MinimumLength = 2)]
       public string Name { get; set; } = string.Empty;
       
       [Range(0.01, 999999.99)]
       public double Price { get; set; }
   }
   ```

5. **Use Async/Await Properly**
   ```csharp
   // Good
   public async Task<Product?> GetByIdAsync(long id)
   {
       return await _dbSet.FindAsync(id);
   }
   
   // Bad
   public Product? GetById(long id)
   {
       return _dbSet.Find(id);  // Blocking!
   }
   ```

### ❌ DON'T

1. **Don't Access DbContext from Controllers**
   ```csharp
   // BAD
   [HttpGet]
   public async Task<ActionResult> GetProducts()
   {
       return Ok(await _context.Products.ToListAsync());
   }
   
   // GOOD
   [HttpGet]
   public async Task<ActionResult> GetProducts()
   {
       var products = await _productService.GetAllProductsAsync();
       return Ok(ApiResponse<IEnumerable<ProductResponseDto>>.SuccessResponse(products));
   }
   ```

2. **Don't Put Business Logic in Controllers**
   ```csharp
   // BAD
   [HttpPost]
   public async Task<ActionResult> CreateOrder([FromBody] OrderDto dto)
   {
       // Don't do calculations in controller!
       var total = 0.0;
       foreach (var item in dto.Items)
       {
           var product = await _context.Products.FindAsync(item.ProductId);
           total += product.Price * item.Quantity;
       }
       // ...
   }
   ```

3. **Don't Return Domain Models**
   ```csharp
   // BAD
   [HttpGet("{id}")]
   public async Task<ActionResult<User>> GetUser(long id)
   {
       return await _context.Users.FindAsync(id);  // Exposes password!
   }
   
   // GOOD
   [HttpGet("{id}")]
   public async Task<ActionResult<UserInfoDto>> GetUser(long id)
   {
       var user = await _userService.GetUserByIdAsync(id);
       return Ok(ApiResponse<UserInfoDto>.SuccessResponse(user));
   }
   ```

---

## 5. Common Patterns

### Pattern 1: Standard CRUD Service
```csharp
public async Task<IEnumerable<TDto>> GetAllAsync()
{
    var entities = await _repository.GetAllAsync();
    return entities.Select(e => MapToDto(e));
}

public async Task<TDto?> GetByIdAsync(long id)
{
    var entity = await _repository.GetByIdAsync(id);
    return entity != null ? MapToDto(entity) : null;
}

public async Task<(bool Success, string Message, TDto? Data)> CreateAsync(TCreateDto dto)
{
    // Validation
    if (await _repository.ExistsAsync(e => e.Name == dto.Name))
        return (false, "Already exists", null);
    
    // Create
    var entity = MapToEntity(dto);
    await _repository.AddAsync(entity);
    await _unitOfWork.SaveChangesAsync();
    
    // Return
    return (true, "Created successfully", MapToDto(entity));
}

public async Task<(bool Success, string Message)> UpdateAsync(long id, TUpdateDto dto)
{
    var entity = await _repository.GetByIdAsync(id);
    if (entity == null)
        return (false, "Not found");
    
    // Update properties
    entity.Name = dto.Name;
    // ...
    
    await _repository.UpdateAsync(entity);
    await _unitOfWork.SaveChangesAsync();
    
    return (true, "Updated successfully");
}

public async Task<(bool Success, string Message)> DeleteAsync(long id)
{
    var entity = await _repository.GetByIdAsync(id);
    if (entity == null)
        return (false, "Not found");
    
    await _repository.DeleteAsync(entity);
    await _unitOfWork.SaveChangesAsync();
    
    return (true, "Deleted successfully");
}
```

### Pattern 2: Transaction Pattern
```csharp
public async Task<(bool Success, string Message)> ProcessOrderAsync(OrderDto dto)
{
    try
    {
        await _unitOfWork.BeginTransactionAsync();
        
        // Step 1: Create order
        var order = new Order { /* ... */ };
        await _unitOfWork.Orders.AddAsync(order);
        
        // Step 2: Update inventory
        foreach (var item in dto.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            product.Stock -= item.Quantity;
            await _unitOfWork.Products.UpdateAsync(product);
        }
        
        // Step 3: Commit
        await _unitOfWork.CommitTransactionAsync();
        
        return (true, "Order processed successfully");
    }
    catch (Exception)
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

### Pattern 3: Authorization Check Pattern
```csharp
public async Task<OrderResponseDto?> GetOrderByIdAsync(long id, long userId, UserRole userRole)
{
    var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(id);
    
    if (order == null)
        return null;
    
    // Authorization: Admin sees all, User sees only their own
    if (userRole != UserRole.Admin && order.UserId != userId)
        return null;
    
    return MapToDto(order);
}
```

---

## 6. Testing Strategy

### Unit Testing Services
```csharp
public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ProductService>>();
        _service = new ProductService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var dto = new ProductDto { Name = "Test Product", Price = 10.99 };
        _unitOfWorkMock.Setup(x => x.Products.ProductNameExistsAsync(dto.Name, null))
            .ReturnsAsync(false);

        // Act
        var (success, message, product) = await _service.CreateProductAsync(dto);

        // Assert
        Assert.True(success);
        Assert.NotNull(product);
    }
}
```

---

## 🎓 Summary: The Workflow Loop

For **every** new feature, follow this loop:

1. **Model** → Create domain entity
2. **DTOs** → Create request/response DTOs with validation
3. **Database** → Update DbContext and run migration
4. **Repository** → Create repository interface and implementation
5. **Unit of Work** → Add repository to UoW
6. **Service** → Create service interface and implementation (business logic)
7. **Controller** → Create thin controller using service
8. **Program.cs** → Register service
9. **Test** → Test via Swagger and write unit tests

This ensures **clean, maintainable, testable, and scalable** code!

---

**Master this workflow and you'll be ready for any professional .NET project!** 🚀
