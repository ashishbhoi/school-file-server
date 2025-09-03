using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolFileServer.Data;
using SchoolFileServer.Models.ViewModels;

namespace SchoolFileServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly SchoolFileContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(SchoolFileContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Get recent files for the home page
            var recentFiles = await _context.Files
                .Include(f => f.UploadedByUser)
                .OrderByDescending(f => f.UploadDate)
                .Take(12)
                .ToListAsync();

            ViewBag.RecentFiles = recentFiles;
            
            // Get classes for navigation
            var classes = await _context.Classes
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            ViewBag.Classes = classes;

            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            var viewModel = new AdminDashboardViewModel
            {
                Teachers = await _context.Users
                    .Where(u => u.UserType == Models.UserType.Teacher && u.IsActive)
                    .OrderBy(u => u.Username)
                    .ToListAsync(),

                Classes = await _context.Classes
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ToListAsync(),

                TotalFiles = await _context.Files.CountAsync(),

                TotalFileSize = await _context.Files.SumAsync(f => f.FileSize),

                RecentFiles = await _context.Files
                    .Include(f => f.UploadedByUser)
                    .OrderByDescending(f => f.UploadDate)
                    .Take(10)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult CreateClass()
        {
            return View(new CreateClassViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClass(CreateClassViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Check if class name already exists
                var existingClass = await _context.Classes
                    .FirstOrDefaultAsync(c => c.ClassName.ToLower() == model.ClassName.ToLower());

                if (existingClass != null)
                {
                    ModelState.AddModelError("ClassName", "A class with this name already exists.");
                    return View(model);
                }

                var schoolClass = new Models.SchoolClass
                {
                    ClassName = model.ClassName.Trim(),
                    DisplayName = model.DisplayName.Trim(),
                    SortOrder = model.SortOrder,
                    IsActive = true
                };

                _context.Classes.Add(schoolClass);
                await _context.SaveChangesAsync();

                // Create the directory structure for the new class
                var classPath = Path.Combine("wwwroot", "uploads", $"Class {schoolClass.ClassName}");
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), classPath);
                
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                    _logger.LogInformation("Created directory for class: {ClassName}", schoolClass.ClassName);
                }

                TempData["SuccessMessage"] = $"Class '{model.DisplayName}' has been created successfully.";
                return RedirectToAction("Admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating class: {ClassName}", model.ClassName);
                ModelState.AddModelError("", "An error occurred while creating the class. Please try again.");
                return View(model);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateClass(int classId)
        {
            try
            {
                var schoolClass = await _context.Classes.FindAsync(classId);
                if (schoolClass == null)
                {
                    return NotFound();
                }

                // Check if class has any files
                var hasFiles = await _context.Files.AnyAsync(f => f.Class == schoolClass.ClassName);
                if (hasFiles)
                {
                    TempData["ErrorMessage"] = "Cannot deactivate class that contains files. Please move or delete all files first.";
                    return RedirectToAction("Admin");
                }

                schoolClass.IsActive = false;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Class '{schoolClass.DisplayName}' has been deactivated.";
                return RedirectToAction("Admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating class: {ClassId}", classId);
                TempData["ErrorMessage"] = "An error occurred while deactivating the class.";
                return RedirectToAction("Admin");
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditClass(int id)
        {
            var schoolClass = await _context.Classes.FindAsync(id);
            if (schoolClass == null)
            {
                return NotFound();
            }

            // Get file count for this class
            var fileCount = await _context.Files.CountAsync(f => f.Class == schoolClass.ClassName);

            var viewModel = new EditClassViewModel
            {
                ClassId = schoolClass.ClassId,
                ClassName = schoolClass.ClassName,
                DisplayName = schoolClass.DisplayName,
                SortOrder = schoolClass.SortOrder,
                IsActive = schoolClass.IsActive,
                FileCount = fileCount,
                OriginalClassName = schoolClass.ClassName
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClass(EditClassViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload file count for display
                model.FileCount = await _context.Files.CountAsync(f => f.Class == (model.OriginalClassName ?? ""));
                return View(model);
            }

            try
            {
                var schoolClass = await _context.Classes.FindAsync(model.ClassId);
                if (schoolClass == null)
                {
                    return NotFound();
                }

                // Check if class name is being changed and if new name already exists
                if (schoolClass.ClassName != model.ClassName)
                {
                    var existingClass = await _context.Classes
                        .FirstOrDefaultAsync(c => c.ClassName.ToLower() == model.ClassName.ToLower() && c.ClassId != model.ClassId);

                    if (existingClass != null)
                    {
                        ModelState.AddModelError("ClassName", "A class with this name already exists.");
                        model.FileCount = await _context.Files.CountAsync(f => f.Class == schoolClass.ClassName);
                        return View(model);
                    }

                    // If class name changed and there are files, update file records
                    var hasFiles = await _context.Files.AnyAsync(f => f.Class == schoolClass.ClassName);
                    if (hasFiles)
                    {
                        // Update all file records with the new class name
                        var files = await _context.Files
                            .Where(f => f.Class == schoolClass.ClassName)
                            .ToListAsync();

                        foreach (var file in files)
                        {
                            file.Class = model.ClassName;
                        }

                        // Move physical directories
                        var oldClassPath = Path.Combine("wwwroot", "uploads", $"Class {schoolClass.ClassName}");
                        var newClassPath = Path.Combine("wwwroot", "uploads", $"Class {model.ClassName}");
                        var fullOldPath = Path.Combine(Directory.GetCurrentDirectory(), oldClassPath);
                        var fullNewPath = Path.Combine(Directory.GetCurrentDirectory(), newClassPath);

                        if (Directory.Exists(fullOldPath) && !Directory.Exists(fullNewPath))
                        {
                            Directory.Move(fullOldPath, fullNewPath);
                            _logger.LogInformation("Moved class directory from {OldPath} to {NewPath}", fullOldPath, fullNewPath);

                            // Update file paths in database
                            foreach (var file in files)
                            {
                                file.FilePath = file.FilePath.Replace($"Class {schoolClass.ClassName}", $"Class {model.ClassName}");
                            }
                        }
                    }
                }

                // Update class properties
                schoolClass.ClassName = model.ClassName.Trim();
                schoolClass.DisplayName = model.DisplayName.Trim();
                schoolClass.SortOrder = model.SortOrder;
                schoolClass.IsActive = model.IsActive;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Class '{model.DisplayName}' has been updated successfully.";
                return RedirectToAction("Admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating class: {ClassId}", model.ClassId);
                ModelState.AddModelError("", "An error occurred while updating the class. Please try again.");
                model.FileCount = await _context.Files.CountAsync(f => f.Class == (model.OriginalClassName ?? ""));
                return View(model);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClass(int classId)
        {
            try
            {
                var schoolClass = await _context.Classes.FindAsync(classId);
                if (schoolClass == null)
                {
                    return NotFound();
                }

                // Check if class has any files
                var hasFiles = await _context.Files.AnyAsync(f => f.Class == schoolClass.ClassName);
                if (hasFiles)
                {
                    TempData["ErrorMessage"] = "Cannot delete class that contains files. Please move or delete all files first.";
                    return RedirectToAction("Admin");
                }

                // Remove the class from database
                _context.Classes.Remove(schoolClass);
                await _context.SaveChangesAsync();

                // Remove physical directory if it exists and is empty
                var classPath = Path.Combine("wwwroot", "uploads", $"Class {schoolClass.ClassName}");
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), classPath);
                
                if (Directory.Exists(fullPath))
                {
                    try
                    {
                        // Only delete if directory is empty
                        if (!Directory.EnumerateFileSystemEntries(fullPath).Any())
                        {
                            Directory.Delete(fullPath, false);
                            _logger.LogInformation("Deleted empty directory for class: {ClassName}", schoolClass.ClassName);
                        }
                    }
                    catch (Exception dirEx)
                    {
                        _logger.LogWarning(dirEx, "Could not delete directory for class: {ClassName}", schoolClass.ClassName);
                    }
                }

                TempData["SuccessMessage"] = $"Class '{schoolClass.DisplayName}' has been deleted successfully.";
                return RedirectToAction("Admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting class: {ClassId}", classId);
                TempData["ErrorMessage"] = "An error occurred while deleting the class.";
                return RedirectToAction("Admin");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
