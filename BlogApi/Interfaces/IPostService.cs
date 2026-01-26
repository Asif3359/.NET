using System;
using System.Collections.Generic;
using System.Linq;
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