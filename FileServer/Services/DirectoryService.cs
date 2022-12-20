using FileServer.Data;
using FileServer.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileServer.Services
{
    public interface IDirectoryService
    {
        void CreateFileStoreDirectory();
        Task<string> GetFullDirectoryPathAsync(int directoryRecordId);
        /// <summary>
        /// Returns the full directory path based on the specified name and parent.
        /// </summary>
        /// <param name="newDirectoryName"></param>
        /// <param name="parentDirectoryId">0 denotes root directory.</param>
        /// <returns></returns>
        Task<string> GetNewDirectoryPathAsync(string newDirectoryName, int parentDirectoryId);
        Task<int?> GetParentDirectoryId(int currentDirectoryId);
    }

    public class DirectoryService : IDirectoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _fileStoreRootDirectory;

        public DirectoryService(ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _fileStoreRootDirectory = configuration["FileStoreRootDirectory"]!;
        }

        public async Task<string> GetNewDirectoryPathAsync(string newDirectoryName, int parentDirectoryId)
        {
            var path = _fileStoreRootDirectory;
            if (parentDirectoryId > 0)
                path = await GetFullPath(parentDirectoryId);

            return Path.Combine(path, newDirectoryName);
        }

        public async Task<string> GetFullDirectoryPathAsync(int directoryRecordId)
        {
            if (directoryRecordId == 0)
                return _fileStoreRootDirectory;

            return await GetFullPath(directoryRecordId);
        }

        private async Task<string> GetFullPath(int directoryRecordId)
        {
            DirectoryRecord currentDirectoryRecord = default!;
            List<string> pathElements = new();
            bool firstIteration = true;

            do
            {
                if (firstIteration)
                {
                    currentDirectoryRecord = await _context
                    .DirectoryRecords
                    .AsNoTracking()
                    .Where(d => d.Id == directoryRecordId)
                    .FirstAsync();
                }
                else if (currentDirectoryRecord.ParentDirectoryId != 0)
                {
                    currentDirectoryRecord = await _context
                    .DirectoryRecords
                    .AsNoTracking()
                    .Where(d => d.Id == currentDirectoryRecord.ParentDirectoryId)
                    .FirstAsync();
                }

                pathElements.Add(currentDirectoryRecord!.Name);

                firstIteration = false;

            } while (currentDirectoryRecord.ParentDirectoryId != 0);

            pathElements.Reverse();
            pathElements.Insert(0, _fileStoreRootDirectory);
            return Path.Combine(pathElements.ToArray());
        }

        public async Task<int?> GetParentDirectoryId(int directoryRecordId)
        {
            var directoryRecord = await _context
                .DirectoryRecords
                .AsNoTracking()
                .Where(d => d.Id == directoryRecordId)
                .FirstAsync();

            return directoryRecord!.ParentDirectoryId;
        }

        public void CreateFileStoreDirectory()
        {
            if (!Directory.Exists(_fileStoreRootDirectory))
                Directory.CreateDirectory(_fileStoreRootDirectory);
        }
    }
}