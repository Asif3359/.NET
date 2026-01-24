Reviewing the EcommerceApi service pattern, then creating the BlogApi services. Explaining the purpose of Services and IServices:


[6 tools called]


Checking available DTOs:


[8 tools called]


Checking the PostTag model:


[2 tools called]


Checking for JWT/auth helpers:


[2 tools called]


Creating all service interfaces and implementations. Purpose of Services and IServices:

- Services (business logic layer)
  - Contain business rules and validation
  - Coordinate between repositories
  - Transform data (Model ↔ DTO)
  - Handle transactions
  - Provide a clean API for controllers

- IServices (service interfaces)
  - Enable dependency injection
  - Make services testable (mockable)
  - Define contracts
  - Support multiple implementations

Creating the service interfaces and implementations:


[1 tool called]


Creating all service interfaces and implementations:

## 1. IAuthService Interface

```csharp:BlogApi/Interfaces/IAuthService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using BlogApi.DTOs;
using BlogApi.Models;

namespace BlogApi.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, UserInfoDto? User, string? Token)> SignupAsync(SignupDto dto);
        Task<(bool Success, string Message, UserInfoDto? User, string? Token)> LoginAsync(LoginDto dto);
        Task<User?> GetUserByIdAsync(long id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> DeleteUserAsync(long id, long currentUserId);
        string GenerateJwtToken(User user);
    }
}
```

## 2. IPostService Interface

```csharp:BlogApi/Interfaces/IPostService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using BlogApi.DTOs;

namespace BlogApi.Interfaces
{
    public interface IPostService
    {
        Task<IEnumerable<PostResponseDto>> GetAllPostsAsync();
        Task<IEnumerable<PostResponseDto>> GetPublishedPostsAsync();
        Task<IEnumerable<PostResponseDto>> GetPublishedPostsPaginatedAsync(int page, int pageSize);
        Task<IEnumerable<PostResponseDto>> GetPostsByAuthorAsync(long authorId);
        Task<IEnumerable<PostResponseDto>> GetPostsByTagAsync(string tagName);
        Task<PostResponseDto?> GetPostByIdAsync(long id);
        Task<(bool Success, string Message, PostResponseDto? Data)> CreatePostAsync(PostDto dto, long authorId);
        Task<(bool Success, string Message, PostResponseDto? Data)> UpdatePostAsync(long id, UpdatePostDto dto, long userId);
        Task<(bool Success, string Message)> DeletePostAsync(long id, long userId);
        Task<(bool Success, string Message)> PublishPostAsync(long id, long userId);
    }
}
```

## 3. ICommentService Interface

```csharp:BlogApi/Interfaces/ICommentService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using BlogApi.DTOs;

namespace BlogApi.Interfaces
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentResponseDto>> GetCommentsByPostIdAsync(long postId);
        Task<IEnumerable<CommentResponseDto>> GetCommentsByUserIdAsync(long userId);
        Task<CommentResponseDto?> GetCommentByIdAsync(long id);
        Task<(bool Success, string Message, CommentResponseDto? Data)> CreateCommentAsync(CommentDto dto, long postId, long userId);
        Task<(bool Success, string Message)> UpdateCommentAsync(long id, CommentDto dto, long userId);
        Task<(bool Success, string Message)> DeleteCommentAsync(long id, long userId);
    }
}
```

## 4. ITagService Interface

```csharp:BlogApi/Interfaces/ITagService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using BlogApi.DTOs;

namespace BlogApi.Interfaces
{
    public interface ITagService
    {
        Task<IEnumerable<TagDto>> GetAllTagsAsync();
        Task<IEnumerable<TagDto>> GetPopularTagsAsync(int count = 10);
        Task<IEnumerable<TagDto>> GetTagsByPostIdAsync(long postId);
        Task<TagDto?> GetTagByIdAsync(long id);
        Task<TagDto?> GetTagByNameAsync(string name);
    }
}
```

## 5. AuthService Implementation

```csharp:BlogApi/Services/AuthService.cs
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BlogApi.DTOs;
using BlogApi.Helpers;
using BlogApi.Interfaces;
using BlogApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BlogApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthService> _logger;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IUnitOfWork unitOfWork,
            ILogger<AuthService> logger,
            IOptions<JwtSettings> jwtSettings)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<(bool Success, string Message, UserInfoDto? User, string? Token)> SignupAsync(SignupDto dto)
        {
            try
            {
                // 1. Validation
                if (await _unitOfWork.Users.EmailExistsAsync(dto.Email))
                {
                    return (false, "Email already exists", null, null);
                }

                // 2. Business logic - Hash password (in production, use BCrypt or similar)
                var hashedPassword = HashPassword(dto.Password);

                // 3. Create user
                var user = new User
                {
                    Name = dto.Name,
                    Email = dto.Email.ToLower(),
                    Password = hashedPassword,
                    Role = Enums.UserRole.User,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // 4. Generate token
                var token = GenerateJwtToken(user);

                var userDto = new UserInfoDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                };

                _logger.LogInformation("User {Email} signed up successfully", dto.Email);

                return (true, "User created successfully", userDto, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Email}", dto.Email);
                throw;
            }
        }

        public async Task<(bool Success, string Message, UserInfoDto? User, string? Token)> LoginAsync(LoginDto dto)
        {
            try
            {
                // 1. Validation
                var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
                if (user == null)
                {
                    return (false, "Invalid email or password", null, null);
                }

                // 2. Business logic - Verify password
                if (!VerifyPassword(dto.Password, user.Password))
                {
                    return (false, "Invalid email or password", null, null);
                }

                // 3. Generate token
                var token = GenerateJwtToken(user);

                var userDto = new UserInfoDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                };

                _logger.LogInformation("User {Email} logged in successfully", dto.Email);

                return (true, "Login successful", userDto, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", dto.Email);
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(long id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _unitOfWork.Users.GetByEmailAsync(email);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _unitOfWork.Users.GetAllAsync();
        }

        public async Task<bool> DeleteUserAsync(long id, long currentUserId)
        {
            if (id == currentUserId)
            {
                return false; // Cannot delete yourself
            }

            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            await _unitOfWork.Users.DeleteAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Helper methods (in production, use BCrypt.Net or similar)
        private string HashPassword(string password)
        {
            // TODO: Use proper password hashing (BCrypt, Argon2, etc.)
            // This is a placeholder - NEVER use in production!
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            // TODO: Use proper password verification
            var hashed = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
            return hashed == hashedPassword;
        }
    }
}
```

## 6. PostService Implementation

```csharp:BlogApi/Services/PostService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogApi.DTOs;
using BlogApi.Enums;
using BlogApi.Interfaces;
using BlogApi.Models;
using Microsoft.Extensions.Logging;

namespace BlogApi.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PostService> _logger;

        public PostService(IUnitOfWork unitOfWork, ILogger<PostService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<PostResponseDto>> GetAllPostsAsync()
        {
            var posts = await _unitOfWork.Posts.GetAllAsync();
            return posts.Select(MapToDto);
        }

        public async Task<IEnumerable<PostResponseDto>> GetPublishedPostsAsync()
        {
            var posts = await _unitOfWork.Posts.GetPublishedPostsAsync();
            return posts.Select(MapToDto);
        }

        public async Task<IEnumerable<PostResponseDto>> GetPublishedPostsPaginatedAsync(int page, int pageSize)
        {
            var posts = await _unitOfWork.Posts.GetPublishedPostsPaginatedAsync(page, pageSize);
            return posts.Select(MapToDto);
        }

        public async Task<IEnumerable<PostResponseDto>> GetPostsByAuthorAsync(long authorId)
        {
            var posts = await _unitOfWork.Posts.GetByAuthorIdAsync(authorId);
            return posts.Select(MapToDto);
        }

        public async Task<IEnumerable<PostResponseDto>> GetPostsByTagAsync(string tagName)
        {
            var posts = await _unitOfWork.Posts.GetByTagAsync(tagName);
            return posts.Select(MapToDto);
        }

        public async Task<PostResponseDto?> GetPostByIdAsync(long id)
        {
            var post = await _unitOfWork.Posts.GetByIdWithDetailsAsync(id);
            return post != null ? MapToDto(post) : null;
        }

        public async Task<(bool Success, string Message, PostResponseDto? Data)> CreatePostAsync(PostDto dto, long authorId)
        {
            try
            {
                // 1. Validation
                // (DTO validation is handled by [Required] attributes)

                // 2. Business logic - Start transaction
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // 3. Create post
                    var post = new Post
                    {
                        Title = dto.Title,
                        Content = dto.Content,
                        AuthorId = authorId,
                        Status = PostStatus.Draft,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Posts.AddAsync(post);
                    await _unitOfWork.SaveChangesAsync(); // Save to get post.Id

                    // 4. Handle tags
                    var postTags = new List<PostTag>();
                    foreach (var tagName in dto.Tags)
                    {
                        if (string.IsNullOrWhiteSpace(tagName))
                            continue;

                        var tag = await _unitOfWork.Tags.GetOrCreateAsync(tagName.Trim());
                        postTags.Add(new PostTag
                        {
                            PostId = post.Id,
                            TagId = tag.Id
                        });
                    }

                    // Add PostTags to context
                    foreach (var postTag in postTags)
                    {
                        _unitOfWork.Posts.GetByIdAsync(post.Id).Result?.PostTags.Add(postTag);
                    }

                    // 5. Save all changes
                    await _unitOfWork.CommitTransactionAsync();

                    // 6. Get full post with details
                    var createdPost = await _unitOfWork.Posts.GetByIdWithDetailsAsync(post.Id);
                    var responseDto = MapToDto(createdPost!);

                    _logger.LogInformation("Post {PostId} created by user {UserId}", post.Id, authorId);

                    return (true, "Post created successfully", responseDto);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post for user {UserId}", authorId);
                throw;
            }
        }

        public async Task<(bool Success, string Message, PostResponseDto? Data)> UpdatePostAsync(long id, UpdatePostDto dto, long userId)
        {
            try
            {
                // 1. Validation
                var post = await _unitOfWork.Posts.GetByIdAsync(id);
                if (post == null)
                {
                    return (false, "Post not found", null);
                }

                // 2. Business logic - Check authorization
                if (!await _unitOfWork.Posts.IsAuthorAsync(id, userId))
                {
                    return (false, "You are not authorized to update this post", null);
                }

                // 3. Update post
                if (!string.IsNullOrWhiteSpace(dto.Title))
                    post.Title = dto.Title;

                if (!string.IsNullOrWhiteSpace(dto.Content))
                    post.Content = dto.Content;

                if (dto.Status.HasValue)
                    post.Status = dto.Status.Value;

                post.UpdatedAt = DateTime.UtcNow;

                // 4. Update tags if provided
                if (dto.Tags != null)
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        // Remove existing tags
                        var existingPost = await _unitOfWork.Posts.GetByIdWithDetailsAsync(id);
                        if (existingPost != null)
                        {
                            existingPost.PostTags.Clear();
                        }

                        // Add new tags
                        foreach (var tagName in dto.Tags)
                        {
                            if (string.IsNullOrWhiteSpace(tagName))
                                continue;

                            var tag = await _unitOfWork.Tags.GetOrCreateAsync(tagName.Trim());
                            existingPost?.PostTags.Add(new PostTag
                            {
                                PostId = id,
                                TagId = tag.Id
                            });
                        }

                        await _unitOfWork.Posts.UpdateAsync(post);
                        await _unitOfWork.CommitTransactionAsync();
                    }
                    catch
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        throw;
                    }
                }
                else
                {
                    await _unitOfWork.Posts.UpdateAsync(post);
                    await _unitOfWork.SaveChangesAsync();
                }

                // 5. Return updated post
                var updatedPost = await _unitOfWork.Posts.GetByIdWithDetailsAsync(id);
                var responseDto = MapToDto(updatedPost!);

                _logger.LogInformation("Post {PostId} updated by user {UserId}", id, userId);

                return (true, "Post updated successfully", responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post {PostId}", id);
                throw;
            }
        }

        public async Task<(bool Success, string Message)> DeletePostAsync(long id, long userId)
        {
            try
            {
                // 1. Validation
                var post = await _unitOfWork.Posts.GetByIdAsync(id);
                if (post == null)
                {
                    return (false, "Post not found");
                }

                // 2. Business logic - Check authorization
                if (!await _unitOfWork.Posts.IsAuthorAsync(id, userId))
                {
                    return (false, "You are not authorized to delete this post");
                }

                // 3. Delete post (cascade will handle comments and tags)
                await _unitOfWork.Posts.DeleteAsync(post);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Post {PostId} deleted by user {UserId}", id, userId);

                return (true, "Post deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {PostId}", id);
                throw;
            }
        }

        public async Task<(bool Success, string Message)> PublishPostAsync(long id, long userId)
        {
            try
            {
                var post = await _unitOfWork.Posts.GetByIdAsync(id);
                if (post == null)
                {
                    return (false, "Post not found");
                }

                if (!await _unitOfWork.Posts.IsAuthorAsync(id, userId))
                {
                    return (false, "You are not authorized to publish this post");
                }

                post.Status = PostStatus.Published;
                post.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Posts.UpdateAsync(post);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Post {PostId} published by user {UserId}", id, userId);

                return (true, "Post published successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing post {PostId}", id);
                throw;
            }
        }

        private PostResponseDto MapToDto(Post post)
        {
            return new PostResponseDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Status = post.Status,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                AuthorId = post.AuthorId,
                AuthorName = post.Author?.Name ?? "",
                CommentCount = post.Comments?.Count ?? 0,
                Tags = post.PostTags?.Select(pt => pt.Tag?.Name ?? "").Where(t => !string.IsNullOrEmpty(t)).ToList() ?? new List<string>()
            };
        }
    }
}
```

## 7. CommentService Implementation

```csharp:BlogApi/Services/CommentService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogApi.DTOs;
using BlogApi.Interfaces;
using BlogApi.Models;
using Microsoft.Extensions.Logging;

namespace BlogApi.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CommentService> _logger;

        public CommentService(IUnitOfWork unitOfWork, ILogger<CommentService> logger)
        {
            _unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<IEnumerable<CommentResponseDto>> GetCommentsByPostIdAsync(long postId)
        {
            var comments = await _unitOfWork.Comments.GetByPostIdAsync(postId);
            return comments.Select(MapToDto);
        }

        public async Task<IEnumerable<CommentResponseDto>> GetCommentsByUserIdAsync(long userId)
        {
            var comments = await _unitOfWork.Comments.GetByUserIdAsync(userId);
            return comments.Select(MapToDto);
        }

        public async Task<CommentResponseDto?> GetCommentByIdAsync(long id)
        {
            var comment = await _unitOfWork.Comments.GetByIdWithUserAsync(id);
            return comment != null ? MapToDto(comment) : null;
        }

        public async Task<(bool Success, string Message, CommentResponseDto? Data)> CreateCommentAsync(CommentDto dto, long postId, long userId)
        {
            try
            {
                // 1. Validation
                if (!await _unitOfWork.Posts.PostExistsAsync(postId))
                {
                    return (false, "Post not found", null);
                }

                // 2. Business logic
                var comment = new Comment
                {
                    Content = dto.Content,
                    PostId = postId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                // 3. Save to database
                await _unitOfWork.Comments.AddAsync(comment);
                await _unitOfWork.SaveChangesAsync();

                // 4. Return result
                var createdComment = await _unitOfWork.Comments.GetByIdWithUserAsync(comment.Id);
                var responseDto = MapToDto(createdComment!);

                logger.LogInformation("Comment {CommentId} created by user {UserId} on post {PostId}", comment.Id, userId, postId);

                return (true, "Comment created successfully", responseDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating comment for post {PostId}", postId);
                throw;
            }
        }

        public async Task<(bool Success, string Message)> UpdateCommentAsync(long id, CommentDto dto, long userId)
        {
            try
            {
                // 1. Validation
                var comment = await _unitOfWork.Comments.GetByIdAsync(id);
                if (comment == null)
                {
                    return (false, "Comment not found");
                }

                // 2. Business logic - Check authorization
                if (!await _unitOfWork.Comments.IsOwnerAsync(id, userId))
                {
                    return (false, "You are not authorized to update this comment");
                }

                // 3. Update comment
                comment.Content = dto.Content;

                await _unitOfWork.Comments.UpdateAsync(comment);
                await _unitOfWork.SaveChangesAsync();

                logger.LogInformation("Comment {CommentId} updated by user {UserId}", id, userId);

                return (true, "Comment updated successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating comment {CommentId}", id);
                throw;
            }
        }

        public async Task<(bool Success, string Message)> DeleteCommentAsync(long id, long userId)
        {
            try
            {
                // 1. Validation
                var comment = await _unitOfWork.Comments.GetByIdAsync(id);
                if (comment == null)
                {
                    return (false, "Comment not found");
                }

                // 2. Business logic - Check authorization
                if (!await _unitOfWork.Comments.IsOwnerAsync(id, userId))
                {
                    return (false, "You are not authorized to delete this comment");
                }

                // 3. Delete comment
                await _unitOfWork.Comments.DeleteAsync(comment);
                await _unitOfWork.SaveChangesAsync();

                logger.LogInformation("Comment {CommentId} deleted by user {UserId}", id, userId);

                return (true, "Comment deleted successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting comment {CommentId}", id);
                throw;
            }
        }

        private CommentResponseDto MapToDto(Comment comment)
        {
            return new CommentResponseDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UserId = comment.UserId,
                UserName = comment.User?.Name ?? ""
            };
        }
    }
}
```

## 8. TagService Implementation

```csharp:BlogApi/Services/TagService.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogApi.DTOs;
using BlogApi.Interfaces;
using Microsoft.Extensions.Logging;

namespace BlogApi.Services
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TagService> _logger;

        public TagService(IUnitOfWork unitOfWork, ILogger<TagService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
        {
            var tags = await _unitOfWork.Tags.GetAllAsync();
            return tags.Select(MapToDto);
        }

        public async Task<IEnumerable<TagDto>> GetPopularTagsAsync(int count = 10)
        {
            var tags = await _unitOfWork.Tags.GetPopularTagsAsync(count);
            return tags.Select(MapToDto);
        }

        public async Task<IEnumerable<TagDto>> GetTagsByPostIdAsync(long postId)
        {
            var tags = await _unitOfWork.Tags.GetTagsByPostIdAsync(postId);
            return tags.Select(MapToDto);
        }

        public async Task<TagDto?> GetTagByIdAsync(long id)
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(id);
            return tag != null ? MapToDto(tag) : null;
        }

        public async Task<TagDto?> GetTagByNameAsync(string name)
        {
            var tag = await _unitOfWork.Tags.GetByNameAsync(name);
            return tag != null ? MapToDto(tag) : null;
        }

        private TagDto MapToDto(Models.Tag tag)
        {
            return new TagDto
            {
                Id = tag.Id,
                Name = tag.Name
            };
        }
    }
}
```

## Summary: Service Pattern

Each service follows this pattern:

```csharp
public async Task<(bool Success, string Message, TDto? Data)> CreateAsync(TDto dto)
{
    // 1. Validation
    if (invalid) return (false, "Error message", null);
    
    // 2. Business logic
    var entity = MapToEntity(dto);
    
    // 3. Save to database
    await _unitOfWork.Repository.AddAsync(entity);
    await _unitOfWork.SaveChangesAsync();
    
    // 4. Return result
    return (true, "Success message", MapToDto(entity));
}
```

## Register Services in Program.cs

```csharp
// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ITagService, TagService>();
```

Should I create the controllers that use these services?