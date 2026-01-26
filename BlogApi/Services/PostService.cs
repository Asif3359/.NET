using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogApi.DTOs;
using BlogApi.Enums;
using BlogApi.Interfaces;
using BlogApi.Models;

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

        public async Task<(bool Success, string Message, PostResponseDto? Data)> CreatePostAsync(PostDto dto, long authorId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var post = new Post
                    {
                        Title = dto.Title,
                        Content = dto.Content,
                        AuthorId = authorId,
                        Status = PostStatus.Draft,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Posts.AddAsync(post);
                    await _unitOfWork.SaveChangesAsync();

                    var postTags = new List<PostTag>();

                    foreach (var tagName in dto.Tags)
                    {
                        if (string.IsNullOrWhiteSpace(tagName))
                        {
                            continue;
                        }

                        var tag = await _unitOfWork.Tags.GetOrCreateAsync(tagName.Trim());
                        postTags.Add(new PostTag
                        {
                            PostId = post.Id,
                            TagId = tag.Id
                        });
                    }

                    foreach (var postTag in postTags)
                    {
                        _unitOfWork.Posts.GetByIdAsync(post.Id).Result?.PostTags.Add(postTag);
                    }

                    await _unitOfWork.CommitTransactionAsync();


                    var createdPost = await _unitOfWork.Posts.GetByIdWithDetailsAsync(post.Id);

                    var responseDto = MapToDto(createdPost!);

                    _logger.LogInformation("Post {PostId} created by user {UserId}", post.Id, authorId);

                    return (true, "Post created syccessfully", responseDto);


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

        public async Task<(bool Success, string Message)> DeletePostAsync(long id, long userId)
        {
            try
            {
                var post = await _unitOfWork.Posts.GetByIdAsync(id);

                if (post == null)
                {
                    return (false, "Post not Found");
                }


                if (!await _unitOfWork.Posts.IsAuthorAsync(id, userId))
                {
                    return (false, "You are not authorized");
                }

                await _unitOfWork.Posts.DeleteAsync(post);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Post {PostId} deleted by user {UserId}", id, userId);

                return (true, "Post Delete successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {postId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<PostResponseDto>> GetAllPostsAsync()
        {
            var post = await _unitOfWork.Posts.GetAllAsync();
            return post.Select(MapToDto);
        }

        public async Task<PostResponseDto?> GetPostByIdAsync(long id)
        {
            var post = await _unitOfWork.Posts.GetByIdWithDetailsAsync(id);
            return post != null ? MapToDto(post) : null;
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

        public async Task<IEnumerable<PostResponseDto>> GetPublishedPostsAsync()
        {
            var post = await _unitOfWork.Posts.GetPublishedPostsAsync();

            return post.Select(MapToDto);
        }

        public async Task<IEnumerable<PostResponseDto>> GetPublishedPostsPaginatedAsync(int page, int pageSize)
        {
            var posts = await _unitOfWork.Posts.GetPublishedPostsPaginatedAsync(page, pageSize);
            return posts.Select(MapToDto);
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