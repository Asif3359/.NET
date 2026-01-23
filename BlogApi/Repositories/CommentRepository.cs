using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogApi.Data;
using BlogApi.Interfaces;
using BlogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Repositories
{
    public class CommentRepository : Repository<Comment>, ICommentRepository
    {
        public CommentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> CommentExistsAsync(long id)
        {
            return await _dbSet.AnyAsync(c => c.Id == id);
        }

        public async Task<Comment?> GetByIdWithUserAsync(long id)
        {
            return await _dbSet
                .Include(c => c.User)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Comment>> GetByPostIdAsync(long postId)
        {
            return await _dbSet
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetByUserIdAsync(long userId)
        {
            return await _dbSet
                .Where(c => c.UserId == userId)
                .Include(c => c.Post)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetCountByPostIdAsync(long postId)
        {
            return await _dbSet.CountAsync(c => c.PostId == postId);
        }

        public async Task<bool> IsOwnerAsync(long commentId, long userId)
        {
            return await _dbSet.AnyAsync(c => c.Id == commentId && c.UserId == userId);
        }
    }
}