namespace ReallySimpleCerts.Core
{
    public class BlobStorePersistenceOptions
    {
        public string StorageConnectionString { get; set; }
        public string ContainerName { get; set; } = "ReallySimpleCerts";
        public string BlobPathPrefix { get; set; } = "";
    }
}
