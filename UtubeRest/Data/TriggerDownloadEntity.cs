using Azure.Data.Tables;
using Azure;

namespace UtubeRest.Data;

public class TriggerDownloadEntity : ITableEntity
{

    // custom    // hashId , videoId, url, 
    public required string Log { get; set; }
    public bool Success { get; set; }


    // interface
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }

    public ETag ETag { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; } = default!;
}
