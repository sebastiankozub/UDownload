using AngleSharp.Io;
using System.Linq.Expressions;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using UtubeRest.Options;

namespace UtubeRest.Data;


public class TriggerDownloadRepository : ITableRepository<TriggerDownloadEntity>
{
    private readonly ILogger<TriggerDownloadRepository> _logger;
    private readonly TableClient _tableClient;
    private readonly TableStorageOptions _tableStorageOptions;

    public TriggerDownloadRepository(ILoggerFactory loggerFactory, IAzureClientFactory<TableServiceClient> tableClientFactory, TableStorageOptions tableStorageOptions)
    {
        _logger = loggerFactory.CreateLogger<TriggerDownloadRepository>();
        _tableStorageOptions = tableStorageOptions;

        _tableClient = tableClientFactory.CreateClient("UtubeRestClient").GetTableClient(_tableStorageOptions.TriggerDownloadTableName);
        _tableClient.CreateIfNotExists();
    }

    public async Task<TriggerDownloadEntity> GetAsync(string rowKey)
    {
        var tableResponse = await _tableClient.GetEntityAsync<TriggerDownloadEntity>(_tableStorageOptions.PartitionKey, rowKey);
        return tableResponse.Value;
    }

    public IAsyncEnumerable<TriggerDownloadEntity> QueryAsync(string filter)
    {
        return _tableClient.QueryAsync<TriggerDownloadEntity>(filter);
    }

    public IAsyncEnumerable<TriggerDownloadEntity> QueryAsync(Expression<Func<TriggerDownloadEntity, bool>> filter)
    {
        return _tableClient.QueryAsync<TriggerDownloadEntity>(filter);
    }

    public Task<Response> CreateAsync(string rowKey, TriggerDownloadEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<Response> UpdateAsync(string rowKey, TriggerDownloadEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<Response> CreateOrUpdateAsync(string rowKey, TriggerDownloadEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<Response> DeleteAsync(string rowKey)
    {
        throw new NotImplementedException();
    }
    
}