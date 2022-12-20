namespace FileServer.Entities
{
    public class FileRecord
    {
        public int Id { get; set; }

        /// <summary>
        /// 0 denotes root directory.
        /// </summary>
        public int DirectoryRecordId { get; set; }

        public DateTime Uploaded { get; set; }

        public string DisplayName { get; set; }

        public string FileName { get; set; }

        /// <summary>
        /// Length of file in bytes.
        /// </summary>
        public long FileLength { get; set; }

        public DateTime? LastDownloaded { get; set; }
    }
}