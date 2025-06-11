# Key Value Manager

A Windows Forms application for securely managing key-value pairs with password protection and encryption.

## Features

- Secure password protection with SHA-256 hashing
- Reset key functionality for password recovery
- SQLite database for data storage
- Windows Data Protection API for value encryption
- Automatic clipboard clearing after 1 minute
- CRUD operations for key-value entries
- User-friendly interface with data grid view

## Requirements

- .NET 8.0 SDK or later
- Windows operating system
- Visual Studio 2022 or later (recommended)

## Setup

1. Clone the repository:
```bash
git clone [repository-url]
```

2. Open the solution in Visual Studio or your preferred IDE

3. Build the solution:
```bash
dotnet build
```

4. Run the application:
```bash
dotnet run
```

## First Run

1. When you first run the application, you'll be prompted to set a password
2. A reset key will be generated - make sure to save this in a secure location
3. You can use this reset key to reset your password if you forget it

## Usage

- Add new key-value pairs using the "Add" button
- Edit existing entries by double-clicking them
- Delete entries using the "Delete" button
- Copy values to clipboard using the "Copy" button
- Values in clipboard are automatically cleared after 1 minute

## Security Features

- Password protection with SHA-256 hashing
- Encrypted value storage using Windows Data Protection API
- Automatic clipboard clearing
- Reset key functionality for password recovery

## Development

- Built with .NET 8.0
- Uses Microsoft.Data.Sqlite for database operations
- Implements Windows Forms for the user interface

## Creating a Portable Release

To create a portable version of the application that can run on any Windows machine without requiring .NET installation:

### Using Visual Studio:
1. Right-click on the project in Solution Explorer
2. Select "Publish"
3. Click "New" to create a new publish profile
4. Select "Folder" as the target
5. Click "Browse" and select your output folder
6. Click "Finish"
7. Click "Publish"

### Using Command Line:
```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeAllContentForSelfExtract=true /p:AssemblyName="Key Value Manager"
```

The portable version will be created as a single executable file named "Key Value Manager.exe" in the publish folder. This executable:
- Contains all necessary dependencies
- Can run on any Windows machine without .NET installation
- Includes the SQLite database
- Maintains all security features

### Distribution:
1. Copy the generated "Key Value Manager.exe" to any location
2. Run the executable directly - no installation required
3. The application will create its database in the same directory as the executable

## License

[Your chosen license] 