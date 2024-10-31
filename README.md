# Backup Database Application

This is a full-stack web application that automates database backups for PostgreSQL. Users can specify a database name, and the application performs every 3-hours (or any set value) backups while logging the history.

## Technologies used

The application is built using the following technologies:

- **Frontend**: Angular 17
- **Backend**: .NET 8
- **Task Scheduling**: Quartz.NET
- **ORM**: Entity Framework Core
- **Database**: PostgreSQL
- **IDE**: Visual Studio

## Features

- Automatic backups of specified databases
- Backup history tracking in PostgreSQL
- Basic dashboard for managing backups
- Error handling and detailed logging of backup operations

## Getting Started

### Prerequisites

Before running this application, make sure you have the following installed:

- [Node.js](https://nodejs.org/)
- [.NET SDK](https://dotnet.microsoft.com/download) (8.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- [Angular CLI](https://angular.dev/tools/cli) (`npm install -g @angular/cli`)

### Installation

1. Create a PostgreSQL database:
```sql
   CREATE DATABASE BackupDb;
```

2. Update the connection string in appsettings.json (dotnet-angular-postgres-backup-tool.Server/):
```json
{
  "ConnectionStrings": {
    "DatabaseConnection": "Host=localhost;Port=5432;Database=BackupDb;Username=postgres;Password=root"
  },
  "BackupSettings": {
    "DbName": "PgBackupToolDb",
    "Path": "C:\\PgBackups",
    "PostgresPassword": "root"
  }
}
```

#### Backend Setup
1. Navigate to the API project directory:
```sh
cd dotnet-angular-postgres-backup-tool.Server/
```

2. Run migrations:
```sh
dotnet ef database update
```

3. Start the API:
```sh
dotnet run
```

The API will be available at https://localhost:5087

#### Frontend Setup
1. Navigate to the Angular project directory:
```
dotnet-angular-postgres-backup-tool.client/
```

2. Install dependencies:
```sh
npm install
```

3. Start the Angular application:
```sh
ng serve
```

The application will be available at http://localhost:4200
