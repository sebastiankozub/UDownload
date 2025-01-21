using AngleSharp.Io;
using System.Linq.Expressions;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using UtubeRest.Options;

namespace UtubeRest.Data;

public interface ITableRepository<T> where T : ITableEntity
{
    Task<T> GetAsync(string rowKey);
    IAsyncEnumerable<T> QueryAsync(string filter);
    IAsyncEnumerable<T> QueryAsync(Expression<Func<T, bool>> filter);
    Task<Response> CreateAsync(string rowKey, T entity);
    Task<Response> UpdateAsync(string rowKey, T entity);
    Task<Response> CreateOrUpdateAsync(string rowKey, T entity);
    Task<Response> DeleteAsync(string rowKey);
}

