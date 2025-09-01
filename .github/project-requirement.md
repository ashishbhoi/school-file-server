# School File Server - Project Requirements

## Project Overview
A web-based file server application designed for school intranet use, enabling teachers to upload and share educational materials (PDF, images, text, video, and audio files) with students via touch screen digital class boards.

## Technical Specifications

### Framework & Technology Stack
- **Backend Framework**: .NET 9 (ASP.NET Core)
- **Database**: SQLite (for easy deployment and portability)
- **CSS Framework**: Tailwind CSS
- **Deployment**: Windows Server IIS
- **Asset Management**: Single bundled CSS and JS files (no CDN dependencies)

### Architecture Requirements
- **Offline Capability**: Must work without internet connectivity
- **Performance**: Optimized for touch screen digital boards
- **Deployment**: Simple deployment on Windows Server IIS using virtual directories

## Functional Requirements

### 1. File Access (Public)
- **Anonymous Access**: Any user can view files without authentication
- **File Types Supported**: PDF, images (jpg, png, gif, etc.), text files, video (mp4, avi, etc.), audio (mp3, wav, etc.)
- **Full Screen Viewing**: Option to view files in full screen mode
- **PDF Presentation Mode**: Navigate through PDF pages with forward/backward buttons (presentation-style)

### 2. File Organization Structure
```
Root Directory
├── Class VI
│   ├── Mathematics
│   ├── Science
│   ├── English
│   └── [Other Subjects]
├── Class VII
├── Class VIII
├── Class IX
├── Class X
├── Class XI
└── Class XII
```

### 3. User Management
- **Default Admin**: Pre-configured admin user account
- **Teacher Accounts**: Admin can create teacher accounts
- **Authentication**: Username/password only (no email required for intranet use)

### 4. File Management (Teachers Only)
- **Upload Files**: Teachers can upload files to their assigned subjects
- **Create Subject Folders**: Teachers can create new subject directories within their class
- **File Organization**: Files stored in virtual directory structure by class → subject

## User Roles & Permissions

### Anonymous Users (Students/General Access)
- View all files
- Access full screen mode
- Use PDF presentation mode
- No upload or modification rights

### Teachers
- All anonymous user permissions
- Upload files to assigned class/subject folders
- Create new subject folders within their assigned classes
- Manage their uploaded content

### Admin
- All teacher permissions
- Create new teacher accounts
- Assign teachers to classes/subjects
- Full system administration access

## Non-Functional Requirements

### Performance
- **Fast Loading**: Single bundled CSS/JS files
- **Touch Optimized**: Interface designed for touch screen interaction
- **Responsive**: Works on various screen sizes

### Security
- **Intranet Only**: Designed for internal network use
- **Basic Authentication**: Simple username/password authentication
- **File Safety**: Prevent execution of uploaded files as code

### Deployment
- **IIS Compatibility**: Must deploy seamlessly on Windows Server IIS
- **SQLite Database**: Portable database that doesn't require separate database server
- **Virtual Directory Structure**: Utilize IIS virtual directories for file organization

## Database Schema Requirements

### Users Table
```sql
- UserID (Primary Key)
- Username (Unique)
- PasswordHash
- UserType (Admin/Teacher)
- AssignedClasses (JSON array)
- CreatedDate
- IsActive
```

### Files Table
```sql
- FileID (Primary Key)
- FileName
- FilePath
- FileType
- Class
- Subject
- UploadedBy (UserID)
- UploadDate
- FileSize
```

### Classes Table
```sql
- ClassID (Primary Key)
- ClassName (VI, VII, VIII, IX, X, XI, XII)
- IsActive
```

### Subjects Table
```sql
- SubjectID (Primary Key)
- SubjectName
- ClassID (Foreign Key)
- CreatedBy (UserID)
- CreatedDate
```

## Development Guidelines

### GitHub Copilot Usage
- **Primary Development Tool**: Utilize GitHub Copilot Agent mode for code generation
- **Code Comments**: Include detailed comments for Copilot context
- **Modular Development**: Break down features into smaller, manageable components

### Code Structure
```
SchoolFileServer/
├── Controllers/
├── Models/
├── Views/
├── Data/
├── wwwroot/
│   ├── css/ (bundled output)
│   ├── js/ (bundled output)
│   └── uploads/ (file storage)
├── Services/
└── Utilities/
```

### Asset Bundling
- **CSS**: Compile all Tailwind CSS into single minified file
- **JavaScript**: Bundle all JS dependencies into single file
- **No External Dependencies**: All assets must be self-contained

## Implementation Phases

### Phase 1: Core Setup
- Project initialization with .NET 9
- SQLite database setup
- Basic authentication system
- File upload functionality

### Phase 2: File Management
- Virtual directory structure implementation
- File viewing capabilities
- PDF presentation mode
- Full screen functionality

### Phase 3: User Interface
- Tailwind CSS implementation
- Touch-optimized interface
- Responsive design
- File browser interface

### Phase 4: Administration
- Admin user creation system
- Teacher account management
- Class/subject assignment system

### Phase 5: Deployment & Testing
- IIS deployment configuration
- Performance optimization
- Security hardening
- Touch screen testing

## Success Criteria

1. **Accessibility**: Students can easily browse and view files without login
2. **Usability**: Touch-friendly interface suitable for digital class boards
3. **Performance**: Fast loading times with bundled assets
4. **Reliability**: Stable operation on school intranet without internet dependency
5. **Maintainability**: Easy for school IT staff to deploy and maintain
6. **Scalability**: Can handle multiple concurrent users during class hours

## Constraints & Assumptions

### Technical Constraints
- Must work offline (no internet dependency)
- Windows Server IIS deployment only
- SQLite database limitations (concurrent writes)
- File size limitations based on server capacity

### Assumptions
- School has Windows Server with IIS installed
- Touch screen digital boards support standard web browsers
- Internal network has sufficient bandwidth for video/audio streaming
- School IT staff has basic IIS administration knowledge

## Risk Mitigation

### File Storage Risks
- **Disk Space**: Implement file size monitoring
- **File Types**: Restrict dangerous file extensions
- **Backup**: Regular backup procedures for uploaded content

### Performance Risks
- **Concurrent Access**: Monitor database performance under load
- **Large Files**: Implement file size limits and compression where appropriate
- **Browser Compatibility**: Test on various browsers used in school