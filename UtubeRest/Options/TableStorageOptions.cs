using System.ComponentModel.DataAnnotations;

namespace UtubeRest.Options
{
    public class TableStorageOptions
    {
        public const string Section = "TableStorage";

        [Required(AllowEmptyStrings = false)]
        [MinLength(4)]
        public required string JobTableName { get; set; } 

        [Required(AllowEmptyStrings = false)]
        [MinLength(4)]
        public required string ManifestTableName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MinLength(4)]
        public required string TriggerDownloadTableName { get; set; }


        [Required(AllowEmptyStrings = false)]
        [MinLength(4)]
        public required string StreamUrlHashTableName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MinLength(4)]
        public required string PartitionKey { get; set; }

        [Required(AllowEmptyStrings = false)]
        public required string StorageConnectionString { get; set; }
    }
}
