# Backup Database Application

A full-stack application built with .NET 8 and Angular 17 to automatically back up PostgreSQL databases. It performs scheduled backups using a cron expression and logs the backup history using Entity Framework Core. The application includes a simple dashboard for viewing the status of backups.

![webapp screenshot](https://raw.githubusercontent.com/Hukasx0/dotnet-angular-postgres-backup-tool/main/screenshot.png)

## Technologies used

The application is built using the following technologies:

- **Frontend**: Angular 17
- **Backend**: .NET 8
- **Task Scheduling**: Quartz.NET
- **ORM**: Entity Framework Core
- **Database**: PostgreSQL
- **IDE**: Visual Studio

## Features

- Scheduled automatic backups of PostgreSQL databases
- Backup history tracking and logging in PostgreSQL
- Basic dashboard for managing backups
- Detailed error handling and logging for robust operation

## Getting Started

### Prerequisites

Before running this application, make sure you have the following installed:

- [Node.js](https://nodejs.org/)
- [.NET SDK](https://dotnet.microsoft.com/download) (8.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- [Angular CLI](https://angular.dev/tools/cli) (`npm install -g @angular/cli`)

### Installation

1. Clone the Repository
```sh
git clone https://github.com/Hukasx0/dotnet-angular-postgres-backup-tool.git
cd dotnet-angular-postgres-backup-tool
```

2. Create a PostgreSQL database:
Open your PostgreSQL client and run the following SQL command to create the required database:
```sql
CREATE DATABASE PgBackupToolDb;
```

3. Configure the Connection String and Backup Settings
Open the **appsettings.json** file located in **dotnet-angular-postgres-backup-tool.Server/** and update the configuration with your PostgreSQL credentials and backup settings:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DatabaseConnection": "Host=localhost;Port=5432;Database=PgBackupToolDb;Username=postgres;Password=root"
  },
  "BackupSettings": {
    "DbName": "PgBackupToolDb",
    "Path": "C:\\PgBackups",
    "PostgresPassword": "root",
    "CronSchedule": "0 0 */3 * * ?"
  }
}
```

- DatabaseConnection: The primary connection string for accessing your PostgreSQL instance.
- DbName: The name of the database to back up.
- Path: Directory where backups will be stored.
- PostgresPassword: Password for your PostgreSQL user.
- CronSchedule: Specifies how frequently backups are performed. The default value ("0 0 */3 * * ?") schedules a backup every 3 hours. You can modify this cron expression as needed.

> Note: The CronSchedule setting uses Quartz.NET's cron syntax. [Quartz Cron Expression Generator](http://www.cronmaker.com/) can help you customize the schedule.

#### Backend Setup
1. Navigate to the Server Directory
```sh
cd dotnet-angular-postgres-backup-tool.Server/
```

2. Run migrations

Use Entity Framework Core to apply migrations and set up the database schema:
```sh
dotnet ef database update
```

3. Start the API Server:
```sh
dotnet run
```

The backend API will start and should be accessible at https://localhost:5087.

#### Frontend Setup
1. Navigate to the Angular project directory:
```
dotnet-angular-postgres-backup-tool.client/
```

2. Install Angular Dependencies

Run the following command to install all required Node.js packages:
```sh
npm install
```

3. Start the Angular application:
```sh
ng serve
```

The application will be available at http://localhost:4200

### Running Tests

#### Backend Tests (Xunit)
1. Navigate to the Test Project Directory

Go to the test directory for the .NET backend:
```sh
cd ServerTest/
```

2. Run Tests

Execute all backend unit and integration tests using the following command:
```sh
dotnet test
```

#### Frontend Tests (Angular)
1. Navigate to the Client Directory

Ensure you are in the Angular project directory:
```sh
cd dotnet-angular-postgres-backup-tool.client/
```

2. Run Angular Tests

Use Angular's testing tool (Karma) to run all frontend tests:
```sh
ng test
```

### Project structure
```
dotnet-angular-postgres-backup-tool/
├── dotnet-angular-postgres-backup-tool.Server/       # .NET Backend
├── dotnet-angular-postgres-backup-tool.client/       # Angular Frontend
├── ServerTest/                                       # Xunit Tests for .NET backend
├── .gitignore
├── README.md
├── screenshot.png                                    # Project screenshot
└── dotnet-angular-postgres-backup-tool.sln           # Visual Studio Solution File
```
