# Performance Web Application

A Blazor Server application for employee performance evaluation and feedback.

## 1. Data Architecture & Flow

### Data Origins and Seeding
- **Source of Truth for Employees**: The initial employee data is loaded from the Excel file located at `Docs/system_software.xlsx`.
- **Database Seeder**: The python script `seeder.py` parses the Excel file, normalizes the text data (e.g. converting Arabic 'ي' and 'ك' to Persian 'ی' and 'ک'), and seeds it into the application database.

### Core Database Tables
- **`Employees`**: Stores all personnel information. It includes a self-referencing hierarchy using `Manager1Id` through `Manager4Id` columns to build the organizational tree and map direct reports (sub-team / زیرمجموعه).
- **`Evaluations`**: Stores the actual performance feedback given by managers. It links to the `Employees` table and tracks both `Behavioral` (رفتاری) and `Functional` (عملکردی) evaluation types.

## 2. Database Schema

### Database Name
`test` (configured in `appsettings.json` via `DefaultConnection`)

### Tables

| Table | Description |
|---|---|
| `Employees` | Stores every company employee. The `PersonnelCode` column matches the Active Directory `sAMAccountName` and is used as the login key. The four nullable `Manager1Id`–`Manager4Id` columns store the GUIDs of this employee's managers at up to four hierarchy levels, enabling the sub-team query without circular FK constraints. |
| `Evaluations` | Stores every submitted evaluation record. `EvaluatorId` (FK → `Employees`) is the manager who submitted the record. `TargetEmployeeId` (FK → `Employees`) is the person being evaluated. `EvalType` is an integer enum: `1 = Behavioral (رفتاری)`, `2 = Functional (عملکردی)`. `ObservationDate` is the date the behaviour was witnessed; `CreatedAt` is the UTC insertion timestamp. |
| `__EFMigrationsHistory` | Standard EF Core migrations tracking table. Do not modify manually. |

### Entity Relationships
```
Employees ──< Evaluations (EvaluatorId)      [one manager → many evaluations given]
Employees ──< Evaluations (TargetEmployeeId) [one employee → many evaluations received]
Employees ──< Employees (Manager1Id…Manager4Id) [self-referencing hierarchy, no FK constraint]
```

## 3. Dependencies, Tools & Licenses

### Third-Party Packages
- **nsoftware.IPWorksAuth**: 
  - **Purpose**: Used for LDAP / Active Directory authentication (LDAP bind to the company domain controller).
  - **Location**: Local NuGet package stored at `LocalDependencies/nsoftware.ipworksauth.24.0.9545.nupkg` — tracked in Git.
  - **Configuration**: A custom `NuGet.Config` adds `LocalDependencies/` as a feed. The runtime license is loaded from `IANJA.lic` placed next to the executable (git-ignored for security).
- **Syncfusion.Blazor** (Enterprise License, v34.1.31):
  - **Purpose**: Premium UI components for the web app.
  - **Packages installed**:
    - `Syncfusion.Blazor.Core` — base Syncfusion runtime
    - `Syncfusion.Blazor.Calendars` — `SfDatePicker` for Jalali (fa-IR) date selection in the evaluation modal
    - `Syncfusion.Blazor.Themes` — Bootstrap5 theme CSS
    - `Syncfusion.Blazor.Grid` — `SfGrid` for the Reports page tabular data
    - `Syncfusion.Blazor.DropDowns` — `SfMultiSelect` with checkbox + avatar item template on the Reports page
    - `Syncfusion.Blazor.Navigations` — `SfTab` for the Reports page tab panels
  - **License Key**: `@32392e302e303b32393bKq35AiUSRDJT5uIaFzRCrJWDo7gKUKH1Rwb6jH+WX4o=`
  - **License Registration**: Called via `SyncfusionLicenseProvider.RegisterLicense(...)` in `Program.cs` before `builder.Build()`.
  - **Culture**: `fa-IR` is set as the default request localization culture in `Program.cs` so all Syncfusion date pickers render natively in Shamsi/Jalali.

## 4. Getting Started (Clone & Run)

Follow these instructions to run the application locally.

### Prerequisites
- .NET 10.0 SDK
- Python 3.x (with `pandas` and `pyodbc` for running the seeder script)
- SQL Server (LocalDB or a dedicated instance)

### Setup Instructions

1. **Clone the Repository**
   ```bash
   git clone https://github.com/Hossein558/Performance.git
   cd Performance
   ```

2. **Configure Database Connection**
   Open `Performance.Web/appsettings.json` and ensure the `DefaultConnection` string points to your local SQL Server instance.

3. **Apply Database Migrations**
   ```bash
   cd Performance.Web
   dotnet ef database update
   cd ..
   ```

4. **Seed the Database**
   Run the python seeder script to populate the `Employees` table from the Excel file:
   ```bash
   pip install pandas pyodbc openpyxl
   python seeder.py
   ```
   *(Note: The `system_software.xlsx`, local NuGet dependencies, and `seeder.py` are fully tracked in Git and available immediately after clone.)*

5. **Provide IPWorks License**
   Place your `IANJA.lic` file in the root of the `Performance.Web` directory (this file is git-ignored).

6. **Run the Application**
   ```bash
   cd Performance.Web
   dotnet run
   ```

