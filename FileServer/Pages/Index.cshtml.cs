using FileServer.Data;
using FileServer.Entities;
using FileServer.Extensions;
using FileServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FileServer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IDirectoryService _directoryService;

        public IndexModel(ApplicationDbContext context,
            IDirectoryService directoryService)
        {
            _context = context;
            _directoryService = directoryService;
        }

        [FromQuery(Name = "directory")]
        public int CurrentDirectoryId { get; set; }

        public DirectoryRecordViewModel[] DirectoryRecords { get; set; } = default!;
        public FileRecordViewModel[] FileRecords { get; set; } = default!;
        public string CurrentDirectoryFullPath { get; set; } = default!;
        public int? ParentDirectoryIdOfCurrentDirectory { get; set; } = null;

        public async Task OnGet()
        {
            DirectoryRecords = await GetDirectoryRecords();
            FileRecords = await GetFileRecords();
            CurrentDirectoryFullPath = await _directoryService.GetFullDirectoryPathAsync(CurrentDirectoryId);
            if (CurrentDirectoryId > 0)
                ParentDirectoryIdOfCurrentDirectory = await _directoryService.GetParentDirectoryId(CurrentDirectoryId);
        }

        private async Task<DirectoryRecordViewModel[]> GetDirectoryRecords()
        {
            return await _context
                .DirectoryRecords
                .AsNoTracking()
                .Where(d => d.ParentDirectoryId == CurrentDirectoryId)
                .Select(d => new DirectoryRecordViewModel
                {
                    Id = d.Id,
                    Name = d.Name
                })
                .ToArrayAsync();
        }

        private async Task<FileRecordViewModel[]> GetFileRecords()
        {
            return await _context
                .FileRecords
                .AsNoTracking()
                .Where(f => f.DirectoryRecordId == CurrentDirectoryId)
                .OrderBy(f => f.Uploaded)
                .Select(f => new FileRecordViewModel
                {
                    Id = f.Id,
                    Name = f.DisplayName,
                    Size = f.FileLength.ToBytesDisplay(),
                    Uploaded = f.Uploaded,
                    LastDownloaded = f.LastDownloaded
                })
                .ToArrayAsync();
        }

        public async Task<ActionResult> OnGetDownload(int fileId)
        {
            var fileRecord = await _context.FileRecords.FindAsync(fileId);
            await MarkLastDownloaded(fileRecord!);
            var directoryPath = await _directoryService.GetFullDirectoryPathAsync(fileRecord!.DirectoryRecordId);
            var fullPath = Path.Combine(directoryPath, fileRecord!.FileName);
            byte[] bytes = System.IO.File.ReadAllBytes(fullPath);
            return File(bytes, "application/octet-stream", fileRecord.DisplayName);
        }

        private async Task MarkLastDownloaded(FileRecord fileRecord)
        {
            fileRecord.LastDownloaded = DateTime.Now;
            _context.Update(fileRecord);
            await _context.SaveChangesAsync();
        }
    }

    public class DirectoryRecordViewModel
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }

    public class FileRecordViewModel
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Size { get; set; }

        public DateTime Uploaded { get; set; }

        public DateTime? LastDownloaded { get; set; }
    }
}