# 🚀 Step-by-Step Guide: Building Your Next .NET API Project

This guide shows you **exactly** how to build a new API project from scratch, following professional standards.

---

## 📋 Project Ideas for Learning

Choose one of these projects (or create your own):

### 1. **Blog API** (Recommended for Beginners)
- **Entities**: User, Post, Comment, Tag
- **Features**: CRUD posts, comments, tags, user authentication
- **Complexity**: ⭐⭐ Medium

### 2. **Task Management API** (Todo App)
- **Entities**: User, Project, Task, Tag
- **Features**: Create projects, add tasks, assign users, set deadlines
- **Complexity**: ⭐⭐ Medium

### 3. **Movie Review API**
- **Entities**: User, Movie, Review, Rating, Genre
- **Features**: Browse movies, write reviews, rate movies, filter by genre
- **Complexity**: ⭐⭐⭐ Medium-High

### 4. **Social Media API** (Advanced)
- **Entities**: User, Post, Comment, Like, Follow, Message
- **Features**: Posts, following, likes, comments, messaging
- **Complexity**: ⭐⭐⭐⭐ High

### 5. **Library Management API**
- **Entities**: User, Book, Author, Loan, Reservation
- **Features**: Book catalog, borrowing system, reservations
- **Complexity**: ⭐⭐⭐ Medium-High

---

## 🎯 Example: Let's Build a **Blog API**

I'll use this as an example throughout the guide.

**Features we'll implement:**
- User authentication (signup/login)
- Create, read, update, delete posts
- Add comments to posts
- Tag system
- User profiles

---

# 📝 PHASE 1: PROJECT SETUP (Day 1)

## Step 1: Create the Project

```bash
# Navigate to your projects folder
cd ~/Documents/.NET

# Create new Web API project
dotnet new webapi -n BlogApi

# Navigate into project
cd BlogApi

# Open in VS Code
code .
```

## Step 2: Install Required Packages

```bash
# Database & EF Core
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools

# Authentication
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package BCrypt.Net-Next

# Documentation
dotnet add package NSwag.AspNetCore
```

## Step 3: Create Folder Structure

```bash
# Create all folders at once
mkdir Models DTOs Controllers Services Repositories Interfaces Data Helpers Middleware Enums
```

Your structure should look like:
```
BlogApi/
├── Models/
├── DTOs/
├── Controllers/
├── Services/
├── Repositories/
├── Interfaces/
├── Data/
├── Helpers/
├── Middleware/
├── Enums/
└── Program.cs
```

## Step 4: Create .gitignore

Create `.gitignore` file with:
```gitignore
bin/
obj/
appsettings.Development.json
*.user
.vs/
.vscode/
```

## Step 5: Initialize Git

```bash
git init
git add .
git commit -m "Initial project setup"
```

---

# 📝 PHASE 2: CREATE DOMAIN MODELS (Day 1-2)

## Step 1: Create Enums First

### `Enums/UserRole.cs`
```csharp
namespace BlogApi.Enums
{
    public enum UserRole
    {
        User = 0,
        Admin = 1,
        Moderator = 2
    }
}
```

### `Enums/PostStatus.cs`
```csharp
namespace BlogApi.Enums
{
    public enum PostStatus
    {
        Draft = 0,
        Published = 1,
        Archived = 2
    }
}
```

## Step 2: Create Models (Start Simple, Add More Later)

### `Models/User.cs`
```csharp
using BlogApi.Enums;

namespace BlogApi.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
```

### `Models/Post.cs`
```csharp
using BlogApi.Enums;

namespace BlogApi.Models
{
    public class Post
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public PostStatus Status { get; set; } = PostStatus.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Foreign Keys
        public long AuthorId { get; set; }
        public User Author { get; set; } = null!;
        
        // Navigation properties
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}
```

### `Models/Comment.cs`
```csharp
namespace BlogApi.Models
{
    public class Comment
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign Keys
        public long PostId { get; set; }
        public Post Post { get; set; } = null!;
        
        public long UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
```

### `Models/Tag.cs`
```csharp
namespace BlogApi.Models
{
    public class Tag
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        
        // Navigation properties
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}
```

### `Models/PostTag.cs` (Join table for Many-to-Many)
```csharp
namespace BlogApi.Models
{
    public class PostTag
    {
        public long PostId { get; set; }
        public Post Post { get; set; } = null!;
        
        public long TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
```

---

# 📝 PHASE 3: CREATE DATABASE CONTEXT (Day 2)

### `Data/AppDbContext.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using BlogApi.Models;
using BlogApi.Enums;

namespace BlogApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<PostTag> PostTags { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure many-to-many relationship
            builder.Entity<PostTag>()
                .HasKey(pt => new { pt.PostId, pt.TagId });

            builder.Entity<PostTag>()
                .HasOne(pt => pt.Post)
                .WithMany(p => p.PostTags)
                .HasForeignKey(pt => pt.PostId);

            builder.Entity<PostTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.PostTags)
                .HasForeignKey(pt => pt.TagId);

            // Set default values
            builder.Entity<User>()
                .Property(u => u.Role)
                .HasDefaultValue(UserRole.User);

            builder.Entity<Post>()
                .Property(p => p.Status)
                .HasDefaultValue(PostStatus.Draft);

            // Add indexes
            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            builder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();
        }
    }
}
```

### Setup Connection String

Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=BlogApiDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;Encrypt=true;"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyMustBeAtLeast32CharactersLong!",
    "Issuer": "BlogApi",
    "Audience": "BlogApiUsers",
    "ExpirationInMinutes": 1440
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Create docker-compose.yml
```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: blogapi_sqlserver
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong!Passw0rd
    ports:
      - "1433:1433"
    restart: unless-stopped
    volumes:
      - blogapi_data:/var/opt/mssql

volumes:
  blogapi_data:
```

### Create First Migration

```bash
# Start SQL Server
docker-compose up -d

# Create migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

**✅ Commit your work:**
```bash
git add .
git commit -m "Add domain models and database context"
```

---

# 📝 PHASE 4: CREATE HELPER CLASSES (Day 2)

## Copy from EcommerceApi:

1. `Helpers/ApiResponse.cs`
2. `Helpers/JwtSettings.cs`
3. `Helpers/PaginationParameters.cs`
4. `Middleware/ExceptionMiddleware.cs`

These are reusable across all projects!

**✅ Commit:**
```bash
git add .
git commit -m "Add helper classes and middleware"
```

---

# 📝 PHASE 5: CREATE DTOs (Day 3)

## Authentication DTOs

### `DTOs/SignupDto.cs`
```csharp
using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs
{
    public class SignupDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
```

### `DTOs/LoginDto.cs`
```csharp
using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
```

### `DTOs/UserInfoDto.cs`
```csharp
namespace BlogApi.DTOs
{
    public class UserInfoDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
```

## Post DTOs

### `DTOs/PostDto.cs`
```csharp
using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs
{
    public class PostDto
    {
        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(10000, MinimumLength = 10)]
        public string Content { get; set; } = string.Empty;

        public List<string> Tags { get; set; } = new List<string>();
    }
}
```

### `DTOs/PostResponseDto.cs`
```csharp
namespace BlogApi.DTOs
{
    public class PostResponseDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public int CommentCount { get; set; }
    }
}
```

## Comment DTOs

### `DTOs/CommentDto.cs`
```csharp
using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs
{
    public class CommentDto
    {
        [Required]
        [StringLength(1000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
    }
}
```

### `DTOs/CommentResponseDto.cs`
```csharp
namespace BlogApi.DTOs
{
    public class CommentResponseDto
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}
```

**✅ Commit:**
```bash
git add .
git commit -m "Add DTOs with validation"
```

---

# 📝 PHASE 6: CREATE REPOSITORIES (Day 3-4)

## Generic Repository

### `Interfaces/IRepository.cs`
```csharp
using System.Linq.Expressions;

namespace BlogApi.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(long id);
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    }
}
```

### `Repositories/Repository.cs`
```csharp
using System.Linq.Expressions;
using BlogApi.Data;
using BlogApi.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(long id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            return predicate == null 
                ? await _dbSet.CountAsync() 
                : await _dbSet.CountAsync(predicate);
        }
    }
}
```

## Specific Repositories

Create similar files for:
- `IUserRepository.cs` / `UserRepository.cs`
- `IPostRepository.cs` / `PostRepository.cs`
- `ICommentRepository.cs` / `CommentRepository.cs`
- `ITagRepository.cs` / `TagRepository.cs`

## Unit of Work

### `Interfaces/IUnitOfWork.cs`
```csharp
namespace BlogApi.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IPostRepository Posts { get; }
        ICommentRepository Comments { get; }
        ITagRepository Tags { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
```

**✅ Commit:**
```bash
git add .
git commit -m "Add repository pattern and unit of work"
```

---

# 📝 PHASE 7: CREATE SERVICES (Day 4-5)

Create service interfaces and implementations:

1. `IAuthService.cs` / `AuthService.cs`
2. `IPostService.cs` / `PostService.cs`
3. `ICommentService.cs` / `CommentService.cs`
4. `ITagService.cs` / `TagService.cs`

**Pattern for each service:**
```csharp
public async Task<(bool Success, string Message, TDto? Data)> CreateAsync(TDto dto)
{
    // 1. Validation
    // 2. Business logic
    // 3. Save to database
    // 4. Return result
}
```

**✅ Commit:**
```bash
git add .
git commit -m "Add service layer with business logic"
```

---

# 📝 PHASE 8: CREATE CONTROLLERS (Day 5-6)

Create controllers one by one:

1. `AuthController.cs` - Start here (signup, login)
2. `PostController.cs` - Main feature
3. `CommentController.cs` - Related to posts
4. `TagController.cs` - Simple CRUD

**Pattern:**
```csharp
[HttpPost]
[Authorize]
public async Task<ActionResult> Create([FromBody] TDto dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ApiResponse.FailureResponse("Validation failed"));

    var (success, message, data) = await _service.CreateAsync(dto);
    
    if (!success)
        return BadRequest(ApiResponse.FailureResponse(message));

    return Ok(ApiResponse<TDto>.SuccessResponse(data!, message));
}
```

**✅ Commit after each controller:**
```bash
git add .
git commit -m "Add AuthController with signup/login"
```

---

# 📝 PHASE 9: CONFIGURE PROGRAM.CS (Day 6)

Update `Program.cs` with:
1. Database connection
2. JWT authentication
3. Services registration
4. Middleware registration
5. Swagger configuration

(Copy structure from EcommerceApi)

**✅ Commit:**
```bash
git add .
git commit -m "Configure Program.cs with JWT and services"
```

---

# 📝 PHASE 10: TEST & REFINE (Day 7)

1. Run the application: `dotnet run`
2. Test in Swagger
3. Fix any bugs
4. Add missing features
5. Improve error handling

---

# 📊 IMPLEMENTATION ORDER SUMMARY

## Week 1: Foundation

### Day 1: Setup
- ✅ Create project
- ✅ Install packages
- ✅ Create folder structure
- ✅ Create enums
- ✅ Create models

### Day 2: Database
- ✅ Create DbContext
- ✅ Configure relationships
- ✅ Create migration
- ✅ Copy helper classes

### Day 3: DTOs & Repositories
- ✅ Create all DTOs
- ✅ Add validation
- ✅ Create repository interfaces
- ✅ Implement repositories

### Day 4: Services
- ✅ Create service interfaces
- ✅ Implement AuthService
- ✅ Start other services

### Day 5: More Services & Controllers
- ✅ Complete all services
- ✅ Create AuthController
- ✅ Test authentication

### Day 6: Controllers & Configuration
- ✅ Create remaining controllers
- ✅ Configure Program.cs
- ✅ Test all endpoints

### Day 7: Testing & Polish
- ✅ Test all features
- ✅ Fix bugs
- ✅ Add documentation
- ✅ Deploy

---

# 🎯 FEATURE IMPLEMENTATION ORDER

## Phase 1: Authentication (Always First!)
1. User signup
2. User login
3. Get current user
4. JWT token generation

## Phase 2: Core Entity (Main Feature)
For Blog API: **Posts**
1. Create post
2. Get all posts
3. Get post by ID
4. Update post
5. Delete post

## Phase 3: Related Features
1. Comments on posts
2. Tags for posts
3. Filter/search posts

## Phase 4: Advanced Features
1. Pagination
2. Sorting
3. User profiles
4. Post likes
5. Follow users

---

# 🔥 PRO TIPS

## 1. Always Start With Authentication
Without auth, you can't test ownership and permissions properly.

## 2. Implement One Complete Feature at a Time
Don't create all models at once. Do:
- User model → Auth service → Auth controller → Test
- Post model → Post service → Post controller → Test

## 3. Test As You Go
Test each endpoint immediately after creating it.

## 4. Commit Often
Commit after completing each major step.

## 5. Use Real Data
Don't just test with "test", "asdf", etc. Use realistic data.

## 6. Start Simple, Add Complexity
Start with basic CRUD, then add:
- Pagination
- Search
- Filters
- Sorting

---

# 📝 DAILY CHECKLIST

### Before Starting Each Day:
- [ ] Pull latest changes: `git pull`
- [ ] Check database is running: `docker ps`
- [ ] Run migrations: `dotnet ef database update`

### After Each Feature:
- [ ] Test in Swagger
- [ ] Check for errors
- [ ] Add validation
- [ ] Handle edge cases
- [ ] Commit changes

### End of Day:
- [ ] Push to GitHub: `git push`
- [ ] Update TODO list
- [ ] Document any issues

---

# 🎓 LEARNING PATH

### Project 1: Blog API (You are here!)
Focus on: Basic CRUD, authentication, relationships

### Project 2: Task Management API
Focus on: Complex relationships, status management

### Project 3: Social Media API
Focus on: Many-to-many, complex queries, performance

### Project 4: E-commerce v2
Focus on: Transactions, payments, inventory

---

# 📚 RESOURCES

- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- Your EcommerceApi project as reference!

---

**Now go build your Blog API! 🚀**

Remember: **Implementation is learning!** Don't just read, CODE!
