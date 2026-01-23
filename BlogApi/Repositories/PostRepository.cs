using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogApi.Data;
using BlogApi.Enums;
using BlogApi.Interfaces;
using BlogApi.Models;
using BlogApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Repositories
{
    public class PostRepository : Repository<Post>, IPostRepository
    {
        public PostRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Post>> GetByAuthorIdAsync(long authorId)
        {
            return await _dbSet
                .Where(p => p.AuthorId == authorId)
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Post?> GetByIdWithDetailsAsync(long id)
        {
            return await _dbSet
                .Include(p => p.Author)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Post>> GetByStatusAsync(PostStatus status)
        {
            return await _dbSet
                .Where(p => p.Status == status)
                .Include(p => p.Author)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetByTagAsync(string tagName)
        {
            return await _dbSet
                .Include(p => p.Author)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .Where(p => p.PostTags.Any(pt => pt.Tag.Name.ToLower() == tagName.ToLower()))
                .Where(p => p.Status == PostStatus.Published)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetPublishedPostsAsync()
        {
            return await _dbSet
                .Where(p => p.Status == PostStatus.Published)
                .Include(p => p.Author)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetPublishedPostsPaginatedAsync(int page, int pageSize)
        {
            return await _dbSet
                .Where(p => p.Status == PostStatus.Published)
                .Include(p => p.Author)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> IsAuthorAsync(long postId, long userId)
        {
            return await _dbSet
                .AnyAsync(p => p.Id == postId && p.AuthorId == userId);
        }

        public async Task<bool> PostExistsAsync(long id)
        {
            return await _dbSet.AnyAsync(p => p.Id == id);
        }
    }
}