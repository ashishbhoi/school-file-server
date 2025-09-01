# GitHub Copilot Instructions - School File Server

## Project Context
This is a **School File Server** web application built with **.NET 9** for sharing educational files on a school intranet. The application will be deployed on **Windows Server IIS** and must work **offline** without any internet connectivity.

## Core Architecture Guidelines

### Technology Stack
- **Framework**: ASP.NET Core (.NET 9) with MVC pattern
- **Database**: SQLite with Entity Framework Core
- **Frontend**: Razor Views with Tailwind CSS
- **Deployment**: Windows Server IIS with Virtual Directories
- **Asset Management**: Bundled CSS/JS (NO CDNs allowed)

### Current Project Structure
```
SchoolFileServer/
### Project Structure
├── Models/                   # Entity models and ViewModels
├── Controllers/          # MVC Controllers
├── Models/              # Entity models and ViewModels
├── Views/               # Razor views
├── Data/                # DbContext and migrations
├── Services/            # Business logic services
├── Middleware/          # Custom middleware
├── wwwroot/            # Static files
│   ├── css/            # Compiled Tailwind CSS
│   ├── js/             # Bundled JavaScript
│   ├── uploads/        # File storage directory
│   └── lib/            # Third-party libraries (local copies)
└── Utilities/          # Helper classes and extensions
## Current Entity Models

## Development Principles

### 1. Offline-First Development
- **NO CDN References**: All CSS, JS, fonts, and libraries must be local
- **Self-Contained**: Application must work without internet connectivity
- **Local Assets**: Download and include all third-party libraries locally
- **Bundling**: Combine all CSS and JS into single files for performance

### 2. Touch-Optimized UI
// Controllers: PascalCase with Controller suffix
public class FileViewController : Controller

// Models: PascalCase, descriptive names
- **Touch Gestures**: Support swipe gestures for PDF navigation
public class UserAccount

// Services: Interface with I prefix, implementation without
public interface IFileService
public class FileService

// Methods: PascalCase, action-oriented
public async Task<IActionResult> UploadFileAsync()

// Variables: camelCase
var uploadedFile = Request.Form.Files[0];

// Constants: PascalCase
public const string DefaultAdminUsername = "admin";
```

### Database Patterns
```csharp
// Always use async/await for database operations
public async Task<List<SchoolFile>> GetFilesByClassAsync(string className)
{
    return await _context.Files
        .Where(f => f.Class == className)
        .OrderBy(f => f.Subject)
        .ToListAsync();
}

// Use proper disposal patterns
using var context = new SchoolFileContext();
    public string AssignedClasses { get; set; } // JSON array
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
}

public class SchoolFile
{
    public int FileId { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string FileType { get; set; }
    public string Class { get; set; }
    public string Subject { get; set; }
public enum UserType
    // Navigation properties
    public virtual UserAccount? UploadedByUser { get; set; }
}

// UserAccount - Current implementation with collections
public class UserAccount
{
    public int UserId { get; set; }
    [Required, StringLength(50)]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    [Required]
    public UserType UserType { get; set; }
    public string AssignedClasses { get; set; } = "[]"; // JSON array
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<SchoolFile> UploadedFiles { get; set; } = new List<SchoolFile>();
    public virtual ICollection<Subject> CreatedSubjects { get; set; } = new List<Subject>();
}

// SchoolClass - Current implementation with DisplayName and SortOrder
public class SchoolClass
{
    public int ClassId { get; set; }
    [Required, StringLength(10)]
    Admin = 1,
    Teacher = 2
    // Navigation properties
    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}

// Subject - Current implementation with relationships
public class Subject
{
    public int SubjectId { get; set; }
    [Required, StringLength(100)]
```

public class FileBrowserViewModel
{
    public string? SelectedClass { get; set; }
    public string? SelectedSubject { get; set; }
## Feature Implementation Guidelines
    public string? NextFileId { get; set; }
}

public class AdminDashboardViewModel
{
    public List<UserAccount> Teachers { get; set; } = new();
    public List<SchoolClass> Classes { get; set; } = new();
    public int TotalFiles { get; set; }
    public long TotalFileSize { get; set; }
    public List<SchoolFile> RecentFiles { get; set; } = new();
}

public class CreateUserViewModel
{
    [Required, StringLength(50)]
    public string Username { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
    [Required]
    public UserType UserType { get; set; }
    public List<string> AssignedClasses { get; set; } = new();
    public List<SchoolClass> AvailableClasses { get; set; } = new();
}
```

## Current Service Interfaces

### IFileService - Current Implementation
```csharp
public interface IFileService
{
    bool ValidateFile(IFormFile file);
    Task<string> SaveFileAsync(IFormFile file, string className, string subject, int userId);
    Task<SchoolFile?> GetFileAsync(int fileId);
    Task<List<SchoolFile>> GetFilesByClassAsync(string? className = null, string? subject = null);
    Task<List<SchoolFile>> SearchFilesAsync(string searchTerm);
    Task<bool> DeleteFileAsync(int fileId, int userId);
    Task<SchoolFile> SaveFileRecordAsync(SchoolFile fileRecord);
    string GetContentType(string fileExtension);
    Task<(string? previousId, string? nextId)> GetAdjacentFilesAsync(int currentFileId, string className, string subject);
### 1. Authentication System
- **Simple Login**: Username/password only (no email)
- **Session Management**: Use ASP.NET Core Identity or custom sessions
- **Default Admin**: Create admin user on first run
- **Password Security**: Use proper hashing (bcrypt or similar)
```

## Current Utility Classes

### DirectoryHelper - Current Implementation
```csharp
public static class DirectoryHelper
{
    public static string GetClassPath(string className)
        => Path.Combine("uploads", $"Class {className}");
// Example upload action pattern
[HttpPost]
[Authorize(Roles = "Admin,Teacher")]
public async Task<IActionResult> UploadFile(IFormFile file, string className, string subject)
    public static string GetSubjectPath(string className, string subject)
    if (!await ValidateFileAsync(file))
        return BadRequest("Invalid file");

    var filePath = await SaveFileAsync(file, className, subject);
    var fileRecord = new SchoolFile
    {
        FileName = file.FileName,
        FilePath = filePath,
        FileType = Path.GetExtension(file.FileName),
        Class = className,
        Subject = subject,
### 4. Virtual Directory Structure
        UploadDate = DateTime.Now,
// Directory helper methods
public class DirectoryHelper
    };

    {
        return Path.Combine("uploads", $"Class {className}");
    }

### 3. File Viewing System
- **PDF Viewer**: Implement page navigation with prev/next buttons
- **Image Viewer**: Support zoom and full-screen mode
- **Video Player**: Use HTML5 video with custom controls
        return Path.Combine(GetClassPath(className), subject);
    }

}
```

## UI Development Guidelines

### Tailwind CSS Usage
```html
    nextPage() {
        if (this.currentPage < this.totalPages) {
            this.currentPage++;
            this.renderPage();
        }
    }
// PDF navigation example
class PDFNavigator {
    constructor(pdfUrl) {
        this.currentPage = 1;
        this.totalPages = 0;
        this.loadPDF(pdfUrl);
    }
```json
{
    prevPage() {
        if (this.currentPage > 1) {
            this.currentPage--;
            this.renderPage();
        }
    }
}
    "postcss": "^8.5.6",
    "tailwindcss": "^3.4.17"
  },
  "dependencies": {
// Touch event handling
document.addEventListener('touchstart', handleTouchStart, false);
## Security Guidelines
### File Security
# Development with watch mode
pnpm run build-css

# Production minified
pnpm run build-css-prod
// Prevent dangerous file types
private readonly string[] _dangerousExtensions =

### Current JavaScript Features
- **PDF Viewer**: PDF.js integration with navigation
    ".exe", ".bat", ".cmd", ".com", ".pif", ".scr", ".vbs", ".js", ".jar", ".asp", ".aspx", ".php"
};

// Sanitize file names
public static string SanitizeFileName(string fileName)
{
    var invalidChars = Path.GetInvalidFileNameChars();
    return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
}

// Validate file content (not just extension)
public static bool ValidateFileContent(IFormFile file)
{
    // Implement file signature validation
    // Check file headers to ensure they match the claimed file type
            entity.Property(e => e.UploadDate).HasDefaultValueSql("datetime('now')");
            
### Input Validation
```

## Development Guidelines

### 1. Follow Current Patterns
- **Entity Models**: Use current validation attributes and navigation properties
- **ViewModels**: Extend existing comprehensive ViewModels
- **Services**: Follow current async/await patterns with proper error handling
- **Controllers**: Use current dependency injection and authorization patterns

### 2. File Handling
```csharp
// Follow current validation patterns
bool ValidateFile(IFormFile file)
{
    var allowedTypes = new[] { ".pdf", ".jpg", ".png", ".mp4", ".txt", ".docx", ".xlsx", ".pptx" };
    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
// Always validate user inputs
[Required]
[StringLength(50, MinimumLength = 3)]
public string Username { get; set; }

[Required]
[StringLength(100)]
public string Subject { get; set; }

// Use model validation in controllers
if (!ModelState.IsValid)

// Use current directory structure
var filePath = Path.Combine(_webHostEnvironment.WebRootPath, 
    DirectoryHelper.GetSubjectPath(className, subject), safeFileName);
```
### Database Optimization
## Performance Guidelines
    return View(model);
    
    if (!string.IsNullOrEmpty(className))
        query = query.Where(f => f.Class == className);
        
// Use appropriate indexes
modelBuilder.Entity<SchoolFile>()
    .HasIndex(f => new { f.Class, f.Subject })
    .HasDatabaseName("IX_SchoolFile_Class_Subject");

// Implement pagination for large file lists
public async Task<PagedResult<SchoolFile>> GetFilesPagedAsync(int page, int pageSize, string className = null)
        
    return await query.OrderBy(f => f.FileName).ToListAsync();
}
```


    var totalCount = await query.CountAsync();
    var files = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
### File Serving
```csharp
// Efficient file serving
public async Task<IActionResult> DownloadFile(int fileId)
{
    var file = await _fileService.GetFileAsync(fileId);
    if (file == null) return NotFound();

    var fileStream = new FileStream(file.FilePath, FileMode.Open, FileAccess.Read);
    return File(fileStream, GetContentType(file.FileType), file.FileName);
}

### Unit Testing Patterns
```csharp
[Fact]
public async Task UploadFile_ValidFile_ReturnsSuccess()
{
    // Arrange
    var mockFile = new Mock<IFormFile>();
    mockFile.Setup(f => f.FileName).Returns("test.pdf");
    mockFile.Setup(f => f.Length).Returns(1024);

## Deployment Configuration

### IIS Web.config
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" arguments=".\SchoolFileServer.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" />
    <security>
      <requestFiltering>
        <requestLimits maxAllowedContentLength="104857600" /> <!-- 100MB -->
When working with GitHub Copilot, use these specific prompts:

1. **"Create a controller method for uploading files to class/subject folders with validation"**
2. **"Generate Entity Framework model for school file management with SQLite"**
3. **"Build a touch-optimized file browser view using Tailwind CSS"**
4. **"Implement PDF viewer with navigation buttons for presentation mode"**
5. **"Create middleware for handling file serving with proper content types"**
6. **"Generate database seeding method for default admin user and classes"**

## Error Handling Patterns

Always implement comprehensive error handling:

- **Documents**: PDF, DOCX, TXT
- **Images**: JPG, PNG, GIF
- **Spreadsheets**: XLSX, CSV
- **Presentations**: PPTX
## Common Prompts for Copilot

        if (!System.IO.File.Exists(file.FilePath))
        {
            _logger.LogError("Physical file missing: {FilePath}", file.FilePath);
            return NotFound("File not found on disk");
        }

        return View(file);
wwwroot/uploads/
├── Class VI/
│   ├── Mathematics/
│   ├── Science/
│   └── English/
├── Class VII/
- **Consistent Logging**: Use ILogger throughout the application
- **Configuration**: Use appsettings.json for all configurable values
- **Documentation**: Comment all complex business logic
- **Validation**: Never trust user input, validate everything
- **Performance**: Monitor file operations and database queries
- **Accessibility**: Ensure the UI is accessible for all users

### Asset Management
- **No CDNs**: All libraries are local in `wwwroot/lib/`
- **Bundled CSS**: Single `tailwind.css` file
- **Local JavaScript**: All scripts served locally

## Common Prompts for Copilot

Use these prompts when working with this project:

1. **"Add a new field to SchoolFile entity following current migration patterns"**
2. **"Create a controller method following current async/await and error handling patterns"**
3. **"Generate a ViewModel following current validation attribute patterns"**
4. **"Build a Razor view using current Tailwind CSS classes and touch optimization"**
5. **"Implement file navigation following current adjacent file lookup patterns"**
6. **"Create a service method following current dependency injection patterns"**
7. **"Add database seeding following current EF Core configuration"**
8. **"Implement file validation following current security patterns"**

## Security Notes

### Current Implementation
- **File Validation**: Comprehensive extension and size checking
- **Path Security**: Using `DirectoryHelper` for safe path construction
- **SQL Injection**: Protected by EF Core parameterization
- **Authorization**: Role-based access control implemented
- **File Storage**: Outside web root access, served through controllers

## Performance Optimizations

### Current Database Indexes
- **Composite Index**: `(Class, Subject)` for file browsing
- **Single Index**: `UploadDate` for recent files
- **Unique Index**: `Username` for user lookup

### File Serving
- **Streaming**: Large files served via streaming
- **Content Types**: Proper MIME type detection
- **Caching**: Browser caching headers for static assets

This document reflects the actual current implementation of your School File Server project. Always reference these patterns when extending or modifying the application.
