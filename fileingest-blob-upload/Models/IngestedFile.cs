namespace fileingest_blob_upload.Models
{
    public class IngestedFile
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string originalFileName { get; set; } = "";
        public string blobName { get; set; } = "";
        public DateTimeOffset created { get; set; }
        public long size { get; set; }
        public string type { get; set; } = "";
        public string contact { get; set; } = "";
    }
}
