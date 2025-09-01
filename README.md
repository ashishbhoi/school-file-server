# School File Server

A web-based file server application designed for school intranet use, enabling teachers to upload and share educational
materials with students via touch screen digital class boards.

## Features

- 🎯 **Touch-Optimized Interface**: Designed for touch screen digital class boards
- 📱 **Responsive Design**: Works on various screen sizes and devices
- 🔒 **Offline Capability**: No internet connection required
- 📁 **File Management**: Support for PDF, images, videos, audio, and documents
- 🎨 **Full-Screen Viewing**: Immersive viewing experience for all media types
- 📄 **PDF Presentation Mode**: Navigate through PDF pages with forward/backward buttons
- 👥 **User Management**: Admin and teacher accounts with role-based permissions
- 🏫 **Class Organization**: Files organized by class and subject structure

## Technology Stack

- **Backend**: .NET 9 (ASP.NET Core MVC)
- **Database**: SQLite (portable, no server required)
- **Frontend**: Razor Views with Tailwind CSS
- **Authentication**: Cookie-based authentication
- **File Storage**: Local file system with virtual directory structure

## Quick Start

### Prerequisites

- .NET 9 SDK
- Windows Server with IIS (for production deployment)

### Development Setup

1. **Clone and Navigate**

   ```powershell
   git clone <repository-url>
   cd SchoolFileServer
   ```

2. **Restore Dependencies**

   ```powershell
   dotnet restore
   ```

3. **Run the Application**

   ```powershell
   dotnet run
   ```

4. **Access the Application**
    - Open browser to: `http://localhost:5272`
    - Default admin login:
        - Username: `admin`
        - Password: `admin123`

## Production Deployment on Windows Server IIS

### Step 1: Prepare the Server

1. **Install IIS with ASP.NET Core Module**

   ```powershell
   # Enable IIS
   Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole, IIS-WebServer, IIS-CommonHttpFeatures, IIS-HttpErrors, IIS-HttpRedirect, IIS-ApplicationDevelopment, IIS-NetFxExtensibility45, IIS-HealthAndDiagnostics, IIS-HttpLogging, IIS-Security, IIS-RequestFiltering, IIS-Performance, IIS-WebServerManagementTools, IIS-ManagementConsole, IIS-IIS6ManagementCompatibility, IIS-Metabase, IIS-ASPNET45

   # Download and install ASP.NET Core Hosting Bundle
   # https://dotnet.microsoft.com/en-us/download/dotnet/9.0
   ```

### Step 2: Publish the Application

1. **Publish for Production**
   ```powershell
   dotnet publish -c Release -o "C:\inetpub\wwwroot\SchoolFileServer"
   ```

### Step 3: Configure IIS

1. **Create Application Pool**

   ```powershell
   # In IIS Manager:
   # - Create new Application Pool named "SchoolFileServer"
   # - Set .NET CLR Version to "No Managed Code"
   # - Set Process Model Identity to "ApplicationPoolIdentity"
   ```

2. **Create Virtual Directory Structure**

   ```
   Default Web Site
   └── SchoolFileServer (Application)
       ├── Physical Path: C:\inetpub\wwwroot\SchoolFileServer
       └── Application Pool: SchoolFileServer
   ```

3. **Set Permissions**

   ```powershell
   # Grant IIS_IUSRS permissions to application directory
   icacls "C:\inetpub\wwwroot\SchoolFileServer" /grant "IIS_IUSRS:(OI)(CI)F" /T

   # Create uploads directory
   mkdir "C:\inetpub\wwwroot\SchoolFileServer\wwwroot\uploads"
   icacls "C:\inetpub\wwwroot\SchoolFileServer\wwwroot\uploads" /grant "IIS_IUSRS:(OI)(CI)F" /T
   ```

### Step 4: Configure the Application

1. **Update Connection String** (if needed)

   ```json
   // appsettings.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=C:\\inetpub\\wwwroot\\SchoolFileServer\\SchoolFileServer.db"
     }
   }
   ```

2. **Ensure Database Permissions**
   ```powershell
   # Grant IIS_IUSRS permissions to database file (after first run)
   icacls "C:\inetpub\wwwroot\SchoolFileServer\SchoolFileServer.db" /grant "IIS_IUSRS:F"
   ```

### Step 5: Test the Deployment

1. Browse to: `http://localhost/SchoolFileServer`
2. Login with default credentials:
    - Username: `admin`
    - Password: `admin123`

## File Organization Structure

The application organizes files in the following structure:

```
wwwroot/uploads/
├── Class VI/
│   ├── Mathematics/
│   ├── Science/
│   ├── English/
│   └── [Other Subjects]/
├── Class VII/
├── Class VIII/
├── Class IX/
├── Class X/
├── Class XI/
└── Class XII/
```

## User Roles and Permissions

### Anonymous Users (Students)

- Browse and view all files
- Access full-screen viewing mode
- Use PDF presentation mode
- No upload or administrative capabilities

### Teachers

- All anonymous user capabilities
- Upload files to assigned classes/subjects
- Create new subject folders
- Delete their own uploaded files

### Administrators

- All teacher capabilities
- Create and manage teacher accounts
- Access admin dashboard
- Delete any files
- Full system administration

## Security Features

- **File Type Validation**: Only allowed file types can be uploaded
- **File Size Limits**: Maximum 100MB per file
- **Path Traversal Protection**: Secure file path handling
- **Content Type Validation**: File content verification
- **Role-Based Access Control**: Different permissions for different user types
- **Secure File Storage**: Files stored outside web root when possible

## Supported File Types

- **Documents**: PDF, Word (.doc, .docx), PowerPoint (.ppt, .pptx), Excel (.xls, .xlsx), Text (.txt)
- **Images**: JPEG, PNG, GIF, BMP, WebP
- **Videos**: MP4, AVI, MOV, WMV, FLV, WebM
- **Audio**: MP3, WAV, FLAC, AAC, OGG

## Configuration

### Default Admin Credentials

```json
// appsettings.json
{
  "DefaultCredentials": {
    "AdminUsername": "admin",
    "AdminPassword": "admin123"
  }
}
```

### File Upload Settings

```json
// appsettings.json
{
  "FileUpload": {
    "MaxFileSize": 104857600, // 100MB in bytes
    "AllowedExtensions": [".pdf", ".jpg", ".jpeg", "..."]
  }
}
```

### Database Configuration

The application uses SQLite for easy deployment and portability. The database is automatically created on first run
with:

- Default admin user (admin/admin123)
- Standard class structure (VI through XII)
- Required database schema

## Troubleshooting

### Common Issues

1. **Database Permission Errors**

   ```powershell
   # Ensure IIS_IUSRS has write access to the database file
   icacls "path\to\SchoolFileServer.db" /grant "IIS_IUSRS:F"
   ```

2. **File Upload Errors**

   ```powershell
   # Check uploads directory permissions
   icacls "C:\inetpub\wwwroot\SchoolFileServer\wwwroot\uploads" /grant "IIS_IUSRS:(OI)(CI)F" /T
   ```

3. **Application Won't Start**
    - Verify ASP.NET Core Hosting Bundle is installed
    - Check Application Pool settings (.NET CLR Version = "No Managed Code")
    - Review IIS logs in `logs\stdout` directory

### Log Locations

- **Application Logs**: `logs\stdout` directory
- **IIS Logs**: `C:\inetpub\logs\LogFiles\W3SVC1\`
- **Windows Event Logs**: Application and System logs

## Backup and Maintenance

### Database Backup

```powershell
# Copy SQLite database file
copy "C:\inetpub\wwwroot\SchoolFileServer\SchoolFileServer.db" "C:\Backups\SchoolFileServer_$(Get-Date -Format 'yyyyMMdd').db"
```

### File Backup

```powershell
# Backup uploads directory
robocopy "C:\inetpub\wwwroot\SchoolFileServer\wwwroot\uploads" "C:\Backups\uploads_$(Get-Date -Format 'yyyyMMdd')" /E
```

### Updating the Application

1. Stop the application pool
2. Backup current application and database
3. Deploy new version using `dotnet publish`
4. Start the application pool

## Performance Optimization

- **Static File Caching**: Configured in web.config for 7 days
- **Compression**: Enabled for static content
- **Database Indexing**: Optimized indexes on frequently queried columns
- **File Streaming**: Large files are streamed rather than loaded into memory

## Support and Customization

The application is designed to be easily customizable for different school environments:

- **Class Names**: Modify the `SchoolClass` seed data in `SchoolFileContext.cs`
- **File Types**: Update allowed extensions in `FileService.cs` and `appsettings.json`
- **UI Styling**: Tailwind CSS classes can be modified in views
- **Upload Limits**: Adjust in both `appsettings.json` and `web.config`

## License

This project is designed for educational use in school environments.
