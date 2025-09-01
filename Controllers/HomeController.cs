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
