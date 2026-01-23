using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogApi.Models;

namespace BlogApi.Interfaces
{
    public interface ITagRepository : IRepository<Tag>
    {
        Task<Tag?> GetByNameAsync(string name);

        Task<bool> TagExistsByNameAsync(string name);

        Task<Tag> GetOrCreateAsync(string name);

        Task<IEnumerable<Tag>> GetTagsByPostIdAsync(long postId);

        Task<IEnumerable<Tag>> GetPopularTagsAsync(int count = 10);
    }
}