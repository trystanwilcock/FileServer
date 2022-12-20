using FileServer.Data;
using FileServer.Entities;
using FileServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace FileServer.Pages
{
    public class UploadModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IDirectoryService _directoryService;
        private readonly string[] _permittedFileExtensions;
        private readonly int _maxFileSize;

        public UploadModel(ApplicationDbContext context, IDirectoryService directoryService)
        {
            _context = context;
            _directoryService = directoryService;
            _permittedFileExtensions = new string[] { ".txt", ".pdf", ".png" };
            _maxFileSize = 10485760; // 10MB.
        }

        [FromQuery(Name = "directory")]
        public int DirectoryId { get; set; } = 0;

        [BindProperty]
        public UploadDTO? UploadDTO { get; set; } = default!;

        /// <summary>
        /// Full path of directory where the file will be uploaded.
        /// </summary>
        public string DirectoryPath { get; set; } = default!;

        public async Task OnGet() =>
            DirectoryPath = await _directoryService.GetFullDirectoryPathAsync(DirectoryId);

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) // Is model valid?
                return Page();

            if (UploadDTO!.File!.Length == 0) // Is there file data?
            {
                ModelState.AddModelError("File", "No file data detected.");
                return Page();
            }
            else if (UploadDTO.File.Length > _maxFileSize) // Does file exceed max file size?
            {
                ModelState.AddModelError("File", "File must not exceed 10MB.");
                return Page();
            }
            else
            {
                var fileExtension = Path.GetExtension(UploadDTO.File.FileName).ToLowerInvariant();

                // Does file have permitted file extension?
                if (string.IsNullOrEmpty(fileExtension) || !_permittedFileExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("File", "Invalid file extension");
                    return Page();
                }

                _directoryService.CreateFileStoreDirectory();
                await SaveFile();
            }

            return RedirectToPage("./Index");
        }

        private async Task SaveFile()
        {
            var generatedFileName = Path.GetRandomFileName();
            var directoryPath = await _directoryService.GetFullDirectoryPathAsync(DirectoryId);
            var fullFilePath = Path.Combine(directoryPath, generatedFileName);
            using (var stream = System.IO.File.Create(fullFilePath))
                await UploadDTO!.File!.CopyToAsync(stream);
            await SaveFileRecord(generatedFileName, UploadDTO.File.Length);
        }

        private async Task SaveFileRecord(string fileName, long fileLength)
        {
            var fileDisplayName = HttpUtility.HtmlEncode(UploadDTO!.File!.FileName);
            _context.Add(new FileRecord
            {
                DirectoryRecordId = DirectoryId,
                Uploaded = DateTime.Now,
                DisplayName = fileDisplayName,
                FileName = fileName,
                FileLength = fileLength
            });
            await _context.SaveChangesAsync();
        }
    }

    public class UploadDTO
    {
        [Required]
        public IFormFile? File { get; set; }
    }
}