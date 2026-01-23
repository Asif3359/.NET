using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogApi.Data;
using BlogApi.Interfaces;
using BlogApi.Models;
using BlogApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BlogApi
{
    public class TagRepository : Repository<Tag>, ITagRepository
    {
        public TagRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Tag?> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
        }

        public async Task<Tag> GetOrCreateAsync(string name)
        {
            var tag = await GetByNameAsync(name);
            if (tag != null)
                return tag;

            tag = new Tag { Name = name };
            await AddAsync(tag);
            return tag;
        }

        public async Task<IEnumerable<Tag>> GetTagsByPostIdAsync(long postId)
        {
            return await _dbSet
                .Where(t => t.PostTags.Any(pt => pt.PostId == postId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int count = 10)
        {
            return await _dbSet
                .OrderByDescending(t => t.PostTags.Count)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> TagExistsByNameAsync(string name)
        {
            return await _dbSet.AnyAsync(t => t.Name.ToLower() == name.ToLower());
        }
    }
}