using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogApi.Models;

namespace BlogApi.Interfaces
{
    public interface ICommentRepository : IRepository<Comment>
    {
        Task<bool> CommentExistsAsync(long id);

        Task<IEnumerable<Comment>> GetByPostIdAsync(long postId);

        Task<IEnumerable<Comment>> GetByUserIdAsync(long userId);

        Task<Comment?> GetByIdWithUserAsync(long id);

        Task<int> GetCountByPostIdAsync(long postId);

        Task<bool> IsOwnerAsync(long commentId, long userId);
    }
}