# Performance Web Application

A Blazor Server application for employee performance evaluation and feedback.

## 1. Data Architecture & Flow

### Data Origins and Seeding
- **Source of Truth for Employees**: The initial employee data is loaded from the Excel file located at `Docs/system_software.xlsx`.
- **Database Seeder**: The python script `seeder.py` parses the Excel file, normalizes the text data (e.g. converting Arabic 'ي' and 'ك' to Persian 'ی' and 'ک'), and seeds it into the application database.

### Core Database Tables
- **`Employees`**: Stores all personnel information. It includes a self-referencing hierarchy using `Manager1Id` and `Manager2Id` columns to build the organizational tree and map direct reports (sub-team / زیرمجموعه).
- **`Evaluations`**: Stores the actual performance feedback given by managers. It links to the `Employees` table and tracks both `Behavioral` (رفتاری) and `Functional` (عملکردی) evaluation types.

## 2. Dependencies, Tools & Licenses

### Third-Party Packages
- **nsoftware.IPWorksAuth**: 
  - **Purpose**: Used for LDAP / Active Directory authentication.
  - **Location**: This is a local NuGet package stored directly in the repository at `LocalDependencies/nsoftware.ipworksauth.24.0.9545.nupkg`.
  - **Configuration**: The project uses a custom `NuGet.Config` to add the `LocalDependencies` folder as a NuGet feed. The runtime license is loaded via `IANJA.lic` which should be placed in the executable directory (it is ignored by git for security).
- **Syncfusion.Blazor**: 
  - **Purpose**: Provides robust UI components, specifically used for natively rendering the Jalali (fa-IR) calendar in the evaluation modal (`SfDatePicker`).
  - **Packages**: `Syncfusion.Blazor.Calendars`, `Syncfusion.Blazor.Themes`, `Syncfusion.Blazor.Core`.
  - **License Registration**: The Syncfusion Enterprise License Key (`@32392e302e303b32393bKq35AiUSRDJT5uIaFzRCrJWDo7gKUKH1Rwb6jH+WX4o=`) is registered in `Program.cs` before `builder.Build()`.

## 3. Getting Started (Clone & Run)

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
   *(Note: The `system_software.xlsx`, local NuGet dependencies, and `seeder.py` are fully tracked in Git and will be available upon clone).*

5. **Provide IPWorks License**
   Place your `IANJA.lic` file in the root of the `Performance.Web` directory (this file is git-ignored).

6. **Run the Application**
   ```bash
   cd Performance.Web
   dotnet run
   ```
