using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace AcadPrep.Application.Common.Models
{
    public class PaginatedList<T>
    {
        public IReadOnlyCollection<T> Items { get; }
        public int PageNumber { get; }
        public int TotalPage { get; }
        public int TotalCount { get; }

        /// <summary>
        /// Constructor used by System.Text.Json for deserialization (e.g. from Redis cache).
        /// Parameter names must match property names (case-insensitive).
        /// </summary>
        [JsonConstructor]
        public PaginatedList(IReadOnlyCollection<T> items, int pageNumber, int totalPage, int totalCount)
        {
            Items = items;
            PageNumber = pageNumber;
            TotalPage = totalPage;
            TotalCount = totalCount;
        }

        /// <summary>
        /// Constructor used by application code via CreateAsync.
        /// </summary>
        public PaginatedList(IReadOnlyCollection<T> items, int count, int pageNumber, int pageSize, bool _)
        {
            PageNumber = pageNumber;
            TotalPage = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
            Items = items;
        }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPage;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedList<T>(items, count, pageNumber, pageSize, true);
        }
    }
}