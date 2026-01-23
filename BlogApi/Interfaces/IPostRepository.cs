using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogApi.Enums;
using BlogApi.Models;

namespace BlogApi.Interfaces
{
    public interface IPostRepository : IRepository<Post>
    {
        Task<bool> PostExistsAsync(long id);

        Task<Post?> GetByIdWithDetailsAsync(long id);

        Task<IEnumerable<Post>> GetByAuthorIdAsync(long authorId);

        Task<IEnumerable<Post>> GetByStatusAsync(PostStatus status);

        Task<IEnumerable<Post>> GetPublishedPostsAsync();

        Task<IEnumerable<Post>> GetPublishedPostsPaginatedAsync(int page, int pageSize);

        Task<IEnumerable<Post>> GetByTagAsync(string tagName);

        Task<bool> IsAuthorAsync(long postId, long userId);

    }
}