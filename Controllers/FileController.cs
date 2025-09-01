using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolFileServer.Data;
using SchoolFileServer.Models;
using SchoolFileServer.Models.ViewModels;
using SchoolFileServer.Services;
using SchoolFileServer.Utilities;
using System.Security.Claims;

namespace SchoolFileServer.Controllers
{
    public class FileController : Controller
    {
        private readonly IFileService _fileService;
        private readonly SchoolFileContext _context;
        private readonly ILogger<FileController> _logger;

        public FileController(IFileService fileService, SchoolFileContext context, ILogger<FileController> logger)
        {
            _fileService = fileService;
            _context = context;
            _logger = logger;
        }

        // Public file browsing - no authentication required
        public async Task<IActionResult> Browse(string? className = null, string? subject = null, string? search = null)
        {
            var viewModel = new FileBrowserViewModel
            {
                SelectedClass = className,
                SelectedSubject = subject,
                SearchTerm = search ?? string.Empty
            };

            // Load classes
            viewModel.Classes = await _context.Classes
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            // Load subjects for selected class
            if (!string.IsNullOrEmpty(className))
            {
                viewModel.Subjects = await _context.Subjects
                    .Include(s => s.Class)
                    .Where(s => s.Class!.ClassName == className && s.IsActive)
                    .OrderBy(s => s.SubjectName)
                    .ToListAsync();
            }

            // Load files
            if (!string.IsNullOrEmpty(search))
            {
                viewModel.Files = await _fileService.SearchFilesAsync(search);
            }
            else
            {
                viewModel.Files = await _fileService.GetFilesByClassAsync(className, subject);
            }

            return View(viewModel);
        }

        // Public file viewing - no authentication required
        public async Task<IActionResult> View(int id)
        {
            var file = await _fileService.GetFileAsync(id);
            if (file == null)
            {
                return NotFound();
            }

            var (previousId, nextId) = await _fileService.GetAdjacentFilesAsync(id, file.Class, file.Subject);

            var viewModel = new FileViewerViewModel
            {
                File = file,
                ContentType = _fileService.GetContentType(file.FileType),
                CanDelete = User.IsInRole("Admin") || (User.IsInRole("Teacher") && 
                           User.FindFirst(ClaimTypes.NameIdentifier)?.Value == file.UploadedBy.ToString()),
                PreviousFileId = previousId,
                NextFileId = nextId
            };

            return View(viewModel);
        }

        // Public file viewing - serves files inline for viewing in browser
        public async Task<IActionResult> ViewInline(int id)
        {
            var file = await _fileService.GetFileAsync(id);
            if (file == null)
            {
                return NotFound();
            }

            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(physicalPath))
            {
                _logger.LogError("Physical file not found: {FilePath}", physicalPath);
                return NotFound("File not found on disk");
            }

            var contentType = _fileService.GetContentType(file.FileType);
            var fileStream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read);

            // Return file without filename to display inline instead of download
            return File(fileStream, contentType);
        }

        // Public file download/serving - no authentication required
        public async Task<IActionResult> Download(int id)
        {
            var file = await _fileService.GetFileAsync(id);
            if (file == null)
            {
                return NotFound();
            }

            var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(physicalPath))
            {
                _logger.LogError("Physical file not found: {FilePath}", physicalPath);
                return NotFound("File not found on disk");
            }

            var contentType = _fileService.GetContentType(file.FileType);
            var fileStream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read);

            return File(fileStream, contentType, file.FileName);
        }

        // File upload - requires authentication
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Upload()
        {
            var viewModel = new FileUploadViewModel();
            ViewBag.Classes = await _context.Classes
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(FileUploadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Classes = await _context.Classes
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ToListAsync();
                return View(model);
            }

            if (!_fileService.ValidateFile(model.File))
            {
                ModelState.AddModelError("File", "Invalid file. Please check file type and size.");
                ViewBag.Classes = await _context.Classes
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ToListAsync();
                return View(model);
            }

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var filePath = await _fileService.SaveFileAsync(model.File, model.Class, model.Subject, userId);

                var fileRecord = new SchoolFile
                {
                    FileName = model.File.FileName,
                    FilePath = filePath,
                    FileType = Path.GetExtension(model.File.FileName),
                    Class = model.Class,
                    Subject = model.Subject,
                    UploadedBy = userId,
                    UploadDate = DateTime.Now,
                    FileSize = model.File.Length,
                    Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description
                };

                await _fileService.SaveFileRecordAsync(fileRecord);

                // Create subject if it doesn't exist
                var existingSubject = await _context.Subjects
                    .FirstOrDefaultAsync(s => s.SubjectName == model.Subject && s.Class!.ClassName == model.Class);

                if (existingSubject == null)
                {
                    var schoolClass = await _context.Classes.FirstOrDefaultAsync(c => c.ClassName == model.Class);
                    if (schoolClass != null)
                    {
                        var newSubject = new Subject
                        {
                            SubjectName = model.Subject,
                            ClassId = schoolClass.ClassId,
                            CreatedBy = userId,
                            CreatedDate = DateTime.Now,
                            IsActive = true
                        };

                        _context.Subjects.Add(newSubject);
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["Success"] = "File uploaded successfully!";
                return RedirectToAction("Browse", new { className = model.Class, subject = model.Subject });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                ModelState.AddModelError(string.Empty, "An error occurred while uploading the file.");
                ViewBag.Classes = await _context.Classes
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ToListAsync();
                return View(model);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var file = await _fileService.GetFileAsync(id);
            if (file == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var canDelete = User.IsInRole("Admin") || file.UploadedBy == userId;

            if (!canDelete)
            {
                return Forbid();
            }

            var success = await _fileService.DeleteFileAsync(id, userId);
            if (success)
            {
                TempData["Success"] = "File deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete file.";
            }

            return RedirectToAction("Browse", new { className = file.Class, subject = file.Subject });
        }

        // API endpoint to get subjects for a class
        [HttpGet]
        public async Task<IActionResult> GetSubjects(string className)
        {
            var subjects = await _context.Subjects
                .Include(s => s.Class)
                .Where(s => s.Class!.ClassName == className && s.IsActive)
                .Select(s => new { value = s.SubjectName, text = s.SubjectName })
                .Distinct()
                .OrderBy(s => s.text)
                .ToListAsync();

            return Json(subjects);
        }
    }
}
