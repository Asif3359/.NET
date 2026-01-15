# 🎯 Developer Cheatsheet

Quick reference for common operations and patterns in this project.

---

## 🔧 Common Commands

### Project Management
```bash
# Build project
dotnet build

# Run project
dotnet run

# Run with auto-reload
dotnet watch run

# Clean build artifacts
dotnet clean

# Restore packages
dotnet restore
```

### Database Operations
```bash
# Create a new migration
dotnet ef migrations add MigrationName

# Update database to latest migration
dotnet ef database update

# Update to specific migration
dotnet ef database update MigrationName

# Rollback to previous migration
dotnet ef database update PreviousMigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove

# List all migrations
dotnet ef migrations list

# Generate SQL script for migration
dotnet ef migrations script

# Drop database
dotnet ef database drop
```

### Docker
```bash
# Start SQL Server
docker-compose up -d

# Stop SQL Server
docker-compose down

# View logs
docker logs sqlserver

# Connect to SQL Server
docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -C
```

---

## 📝 Code Snippets

### Adding a New Entity

#### 1. Create Model
```csharp
// Models/Review.cs
public class Review
{
    public long Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; }
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

#### 2. Create DTOs
```csharp
// DTOs/ReviewDto.cs
public class ReviewDto
{
    [Required(ErrorMessage = "Content is required")]
    [StringLength(500, MinimumLength = 10)]
    public string Content { get; set; } = string.Empty;

    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [Required]
    public long ProductId { get; set; }
}

// DTOs/ReviewResponseDto.cs
public class ReviewResponseDto
{
    public long Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

#### 3. Update DbContext
```csharp
// Data/AppDbContext.cs
public DbSet<Review> Reviews { get; set; }

protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    
    builder.Entity<Review>()
        .HasIndex(r => r.ProductId);
}
```

#### 4. Repository Interface
```csharp
// Interfaces/IReviewRepository.cs
public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetByProductIdAsync(long productId);
    Task<Review?> GetWithDetailsAsync(long id);
}
```

#### 5. Repository Implementation
```csharp
// Repositories/ReviewRepository.cs
public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Review>> GetByProductIdAsync(long productId)
    {
        return await _dbSet
            .Include(r => r.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review?> GetWithDetailsAsync(long id)
    {
        return await _dbSet
            .Include(r => r.Product)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}
```

#### 6. Update UnitOfWork
```csharp
// Interfaces/IUnitOfWork.cs
IReviewRepository Reviews { get; }

// Repositories/UnitOfWork.cs
public IReviewRepository Reviews { get; }

public UnitOfWork(AppDbContext context)
{
    _context = context;
    // ... other repositories
    Reviews = new ReviewRepository(_context);
}
```

#### 7. Service Interface
```csharp
// Interfaces/IReviewService.cs
public interface IReviewService
{
    Task<IEnumerable<ReviewResponseDto>> GetByProductIdAsync(long productId);
    Task<(bool Success, string Message, ReviewResponseDto? Data)> CreateAsync(ReviewDto dto, long userId);
}
```

#### 8. Service Implementation
```csharp
// Services/ReviewService.cs
public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(IUnitOfWork unitOfWork, ILogger<ReviewService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, ReviewResponseDto? Data)> 
        CreateAsync(ReviewDto dto, long userId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
        if (product == null)
            return (false, "Product not found", null);

        var review = new Review
        {
            Content = dto.Content,
            Rating = dto.Rating,
            ProductId = dto.ProductId,
            UserId = userId
        };

        await _unitOfWork.Reviews.AddAsync(review);
        await _unitOfWork.SaveChangesAsync();

        var result = await _unitOfWork.Reviews.GetWithDetailsAsync(review.Id);
        return (true, "Review created", MapToDto(result!));
    }

    private ReviewResponseDto MapToDto(Review review) => new()
    {
        Id = review.Id,
        Content = review.Content,
        Rating = review.Rating,
        ProductName = review.Product?.Name ?? "",
        UserName = review.User?.Name ?? "",
        CreatedAt = review.CreatedAt
    };
}
```

#### 9. Controller
```csharp
// Controllers/ReviewController.cs
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
        var reviews = await _reviewService.GetByProductIdAsync(productId);
        return Ok(ApiResponse<IEnumerable<ReviewResponseDto>>
            .SuccessResponse(reviews));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] ReviewDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse.FailureResponse("Validation failed"));

        var userId = GetUserId();
        var (success, message, data) = await _reviewService.CreateAsync(dto, userId);

        if (!success)
            return BadRequest(ApiResponse.FailureResponse(message));

        return Ok(ApiResponse<ReviewResponseDto>.SuccessResponse(data!, message));
    }

    private long GetUserId() =>
        long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
}
```

#### 10. Register Service
```csharp
// Program.cs
builder.Services.AddScoped<IReviewService, ReviewService>();
```

---

## 🔑 Common Patterns

### Get All Items
```csharp
[HttpGet]
public async Task<ActionResult> GetAll()
{
    var items = await _service.GetAllAsync();
    return Ok(ApiResponse<IEnumerable<ItemDto>>.SuccessResponse(items));
}
```

### Get By ID
```csharp
[HttpGet("{id}")]
public async Task<ActionResult> GetById(long id)
{
    var item = await _service.GetByIdAsync(id);
    if (item == null)
        return NotFound(ApiResponse.FailureResponse("Not found"));
    return Ok(ApiResponse<ItemDto>.SuccessResponse(item));
}
```

### Create
```csharp
[HttpPost]
[Authorize(Roles = "Admin")]
public async Task<ActionResult> Create([FromBody] ItemDto dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ApiResponse.FailureResponse("Validation failed"));

    var (success, message, data) = await _service.CreateAsync(dto);
    
    if (!success)
        return BadRequest(ApiResponse.FailureResponse(message));

    return CreatedAtAction(nameof(GetById), 
        new { id = data!.Id }, 
        ApiResponse<ItemDto>.SuccessResponse(data, message));
}
```

### Update
```csharp
[HttpPut("{id}")]
[Authorize(Roles = "Admin")]
public async Task<ActionResult> Update(long id, [FromBody] ItemDto dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ApiResponse.FailureResponse("Validation failed"));

    var (success, message) = await _service.UpdateAsync(id, dto);
    
    if (!success)
        return NotFound(ApiResponse.FailureResponse(message));

    return Ok(ApiResponse.SuccessResponse(message));
}
```

### Delete
```csharp
[HttpDelete("{id}")]
[Authorize(Roles = "Admin")]
public async Task<ActionResult> Delete(long id)
{
    var (success, message) = await _service.DeleteAsync(id);
    
    if (!success)
        return NotFound(ApiResponse.FailureResponse(message));

    return Ok(ApiResponse.SuccessResponse(message));
}
```

### Get Current User ID
```csharp
private long GetCurrentUserId()
{
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    return long.TryParse(userIdClaim, out var userId) ? userId : 0;
}
```

### Get Current User Role
```csharp
private UserRole GetCurrentUserRole()
{
    var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
    return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.User;
}
```

---

## 📊 EF Core Query Patterns

### Basic Queries
```csharp
// Get all
await _dbSet.ToListAsync();

// Get by ID
await _dbSet.FindAsync(id);

// Get first matching
await _dbSet.FirstOrDefaultAsync(x => x.Name == "Test");

// Get where
await _dbSet.Where(x => x.Price > 100).ToListAsync();

// Check exists
await _dbSet.AnyAsync(x => x.Email == email);

// Count
await _dbSet.CountAsync();

// Count with condition
await _dbSet.CountAsync(x => x.IsActive);
```

### Include Related Data
```csharp
// Single level
await _dbSet
    .Include(x => x.Category)
    .ToListAsync();

// Multiple relations
await _dbSet
    .Include(x => x.Category)
    .Include(x => x.Reviews)
    .ToListAsync();

// Nested include
await _dbSet
    .Include(x => x.OrderItems)
        .ThenInclude(oi => oi.Product)
    .ToListAsync();
```

### Filtering and Sorting
```csharp
// Where + OrderBy
await _dbSet
    .Where(x => x.Price > 100)
    .OrderBy(x => x.Name)
    .ToListAsync();

// Multiple conditions
await _dbSet
    .Where(x => x.Price > 100 && x.CategoryId == 1)
    .OrderByDescending(x => x.CreatedAt)
    .ToListAsync();

// Take first N
await _dbSet
    .OrderBy(x => x.Name)
    .Take(10)
    .ToListAsync();

// Skip and Take (pagination)
await _dbSet
    .OrderBy(x => x.Name)
    .Skip(pageSize * (pageNumber - 1))
    .Take(pageSize)
    .ToListAsync();
```

### Projection
```csharp
// Select specific fields
await _dbSet
    .Select(x => new ProductDto
    {
        Id = x.Id,
        Name = x.Name,
        Price = x.Price
    })
    .ToListAsync();

// With related data
await _dbSet
    .Include(x => x.Category)
    .Select(x => new ProductDto
    {
        Id = x.Id,
        Name = x.Name,
        CategoryName = x.Category.Name
    })
    .ToListAsync();
```

---

## 🔐 JWT Operations

### Generate Token
```csharp
var tokenHandler = new JwtSecurityTokenHandler();
var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.Name),
        new Claim(ClaimTypes.Role, user.Role.ToString())
    }),
    Expires = DateTime.UtcNow.AddMinutes(1440),
    SigningCredentials = new SigningCredentials(
        new SymmetricSecurityKey(key),
        SecurityAlgorithms.HmacSha256Signature)
};

var token = tokenHandler.CreateToken(tokenDescriptor);
return tokenHandler.WriteToken(token);
```

### Validate Token Manually
```csharp
var tokenHandler = new JwtSecurityTokenHandler();
var key = Encoding.ASCII.GetBytes(secret);

tokenHandler.ValidateToken(token, new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = false,
    ValidateAudience = false,
    ClockSkew = TimeSpan.Zero
}, out SecurityToken validatedToken);
```

---

## 🧪 Testing Snippets

### Service Tests
```csharp
[Fact]
public async Task GetById_ExistingId_ReturnsItem()
{
    // Arrange
    var mockRepo = new Mock<IProductRepository>();
    mockRepo.Setup(x => x.GetByIdAsync(1))
        .ReturnsAsync(new Product { Id = 1, Name = "Test" });

    var service = new ProductService(new Mock<IUnitOfWork>().Object, logger);

    // Act
    var result = await service.GetByIdAsync(1);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(1, result.Id);
}
```

### Controller Tests
```csharp
[Fact]
public async Task GetAll_ReturnsOkResult()
{
    // Arrange
    var mockService = new Mock<IProductService>();
    mockService.Setup(x => x.GetAllAsync())
        .ReturnsAsync(new List<ProductDto>());

    var controller = new ProductController(mockService.Object, logger);

    // Act
    var result = await controller.GetProducts();

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.NotNull(okResult.Value);
}
```

---

## 📋 Validation Attributes

### Common Validations
```csharp
[Required(ErrorMessage = "Field is required")]
public string Name { get; set; }

[StringLength(100, MinimumLength = 2, ErrorMessage = "Must be 2-100 chars")]
public string Title { get; set; }

[EmailAddress(ErrorMessage = "Invalid email")]
public string Email { get; set; }

[Range(0.01, 999999.99, ErrorMessage = "Must be between 0.01 and 999999.99")]
public double Price { get; set; }

[RegularExpression(@"^\d{10}$", ErrorMessage = "Must be 10 digits")]
public string Phone { get; set; }

[Compare("Password", ErrorMessage = "Passwords don't match")]
public string ConfirmPassword { get; set; }

[Url(ErrorMessage = "Invalid URL")]
public string Website { get; set; }

[MinLength(6, ErrorMessage = "Minimum 6 characters")]
public string Password { get; set; }
```

---

## 🔍 SQL Queries for Debugging

```sql
-- View all tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE';

-- View all users
SELECT * FROM Users;

-- View all products with categories
SELECT p.*, c.Name AS CategoryName 
FROM Products p 
LEFT JOIN Categories c ON p.CategoryId = c.Id;

-- View all orders with user info
SELECT o.*, u.Name AS UserName, u.Email 
FROM Orders o 
INNER JOIN Users u ON o.UserId = u.Id;

-- View order items with product details
SELECT oi.*, p.Name AS ProductName, o.OrderNumber 
FROM OrderItems oi
INNER JOIN Products p ON oi.ProductId = p.Id
INNER JOIN Orders o ON oi.OrderId = o.Id;

-- Count products per category
SELECT c.Name, COUNT(p.Id) AS ProductCount
FROM Categories c
LEFT JOIN Products p ON c.Id = p.CategoryId
GROUP BY c.Id, c.Name;

-- Get orders total
SELECT o.Id, o.OrderNumber, SUM(oi.Price * oi.Quantity) AS Total
FROM Orders o
INNER JOIN OrderItems oi ON o.Id = oi.OrderId
GROUP BY o.Id, o.OrderNumber;
```

---

## ⚡ Quick Reference

### HTTP Status Codes
- **200 OK** - Success
- **201 Created** - Resource created
- **204 No Content** - Success with no body
- **400 Bad Request** - Validation failed
- **401 Unauthorized** - Not authenticated
- **403 Forbidden** - Not authorized
- **404 Not Found** - Resource not found
- **500 Internal Server Error** - Server error

### Entity States
- **Detached** - Not tracked
- **Unchanged** - Tracked, not modified
- **Added** - Will be inserted
- **Modified** - Will be updated
- **Deleted** - Will be deleted

### Useful LINQ Methods
- `Where()` - Filter
- `Select()` - Project
- `OrderBy()` / `OrderByDescending()` - Sort
- `Include()` / `ThenInclude()` - Eager load
- `FirstOrDefaultAsync()` - Get first or null
- `SingleOrDefaultAsync()` - Get single or null
- `AnyAsync()` - Check if exists
- `CountAsync()` - Count items
- `SumAsync()` - Sum values
- `ToListAsync()` - Execute and return list

---

**Keep this cheatsheet handy for quick reference!** 📌
