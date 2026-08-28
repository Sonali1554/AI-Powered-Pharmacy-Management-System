# AI-Powered Pharmacy Management System

Welcome to the AI-Powered Pharmacy Management System repository! 

## Prerequisites

Before you begin, ensure you have the following installed on your machine:
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) (or the version specified in the project, typically .NET 6/7/8).

## Getting Started

Follow these steps to get the project up and running on your local machine:

### 1. Clone the repository
```bash
git clone https://github.com/Sonali1554/AI-Powered-Pharmacy-Management-System.git
cd AI-Powered-Pharmacy-Management-System
```

### 2. Restore Dependencies
Run the following command to restore the NuGet packages required by the project:
```bash
dotnet restore
```

### 3. Database Setup
This project uses SQLite (`app.db`) for its database. Entity Framework Core is used for data access.
To apply any pending migrations and create the database, run:
```bash
dotnet tool install --global dotnet-ef --version 8.* # if you don't have ef tools
dotnet ef database update
```
*(Note: If the application uses `EnsureCreated()` on startup, simply running the app might be enough to create the SQLite DB).*

### 4. Run the Application
Start the application using the .NET CLI:
```bash
dotnet run
```
The application will typically be available at `http://localhost:5000` or `https://localhost:5001`. Check the console output for the exact URL.

## Troubleshooting
- **Build Errors**: Ensure you have the correct version of the .NET SDK installed.
- **Database Issues**: If you face database locked errors, ensure no other process (like DB Browser for SQLite) is holding a lock on `app.db`.

## Contributing
Feel free to submit pull requests or create issues if you find any bugs or have feature requests.
