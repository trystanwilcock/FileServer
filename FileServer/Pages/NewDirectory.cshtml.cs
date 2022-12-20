using FileServer.Data;
using FileServer.Entities;
using FileServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace FileServer.Pages
{
    public class NewDirectoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IDirectoryService _directoryService;

        public NewDirectoryModel(ApplicationDbContext context, IDirectoryService directoryService)
        {
            _context = context;
            _directoryService = directoryService;
        }

        [FromQuery(Name = "parent-directory")]
        public int ParentDirectoryId { get; set; }

        [BindProperty]
        public NewDirectoryDTO? NewDirectoryDTO { get; set; } = default!;

        /// <summary>
        /// Full path of where the new directory will be created.
        /// </summary>
        public string DirectoryPath { get; set; } = default!;

        public async Task OnGet() =>
            DirectoryPath = await _directoryService.GetFullDirectoryPathAsync(ParentDirectoryId);

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            await CreateDirectory();
            await CreateDirectoryRecord();
            return RedirectToPage("./Index");
        }

        private async Task CreateDirectory()
        {
            var newDirectoryPath = await _directoryService.GetNewDirectoryPathAsync(NewDirectoryDTO!.DirectoryName, ParentDirectoryId);
            Directory.CreateDirectory(newDirectoryPath);
        }

        private async Task CreateDirectoryRecord()
        {
            var newDirectoryRecord = new DirectoryRecord
            {
                Name = NewDirectoryDTO!.DirectoryName,
                ParentDirectoryId = ParentDirectoryId
            };
            await _context.AddAsync(newDirectoryRecord);
            await _context.SaveChangesAsync();
        }
    }

    public class NewDirectoryDTO
    {
        [Required]
        [StringLength(100)]
        public string DirectoryName { get; set; } = default!;
    }
}