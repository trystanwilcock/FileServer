namespace FileServer.Entities
{
    public class DirectoryRecord
    {
        public int Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// 0 denotes root directory.
        /// </summary>
        public int ParentDirectoryId { get; set; }
    }
}