# YHABudget

A personal budget management application built with WPF and .NET, designed to track income, expenses, absences, and recurring transactions.

## Features

### Transaction Management
- Track income and expenses with categories
- Filter transactions by month and category
- "Show All" option to view all transactions
- Recurring transaction support
- Visual category breakdown

### Absence Tracking
- Record work absences (VAB, sick leave)
- Calculate daily income, deductions, and compensation
- Filter absences by month
- Automatic salary impact calculations

### Salary Management
- Configure salary settings
- Track expected monthly results
- Account balance projections
- Scheduled income and expenses overview

### Overview Dashboard
- Monthly income and expense summary
- Net balance calculations
- Category breakdown visualization
- Current vs past month indicators

## Technical Stack

- **Frontend**: WPF (Windows Presentation Foundation)
- **Backend**: .NET with Entity Framework Core
- **Architecture**: MVVM (Model-View-ViewModel)
- **Database**: SQL Server (via Entity Framework Core)
- **Testing**: xUnit

## Project Structure

```
YHABudget/
├── YHABudget.Core/          # Core logic and ViewModels
│   ├── Commands/            # RelayCommand implementations
│   ├── Helpers/             # Shared utilities (DateFormatHelper)
│   ├── MVVM/                # ViewModelBase
│   ├── Services/            # Service interfaces
│   └── ViewModels/          # MVVM ViewModels
├── YHABudget.Data/          # Data access layer
│   ├── Models/              # Entity models
│   ├── Services/            # Business logic services
│   └── Context/             # Database context
├── YHABudget.WPF/           # WPF UI layer
│   ├── Views/               # XAML views
│   ├── Dialogs/             # Dialog windows
│   ├── Resources/           # Styles and themes
│   └── Services/            # UI services
└── YHABudget.Tests/         # Unit tests

## Key Features Implemented

### Filtering System
- Month-based filtering with "Visa alla" (Show All) option
- Category filtering for transactions
- Star indicator (★) for current month
- Consistent Swedish date formatting (e.g., "December 2025")

### Service Layer Architecture
- Data access logic separated from UI
- `ITransactionService` with filtering methods
- `IAbsenceService` with month-based queries
- Proper separation of concerns

### Shared Components
- `DateFormatHelper` for consistent date formatting
- `FilterButtonStyle` for uniform UI
- Reusable dialog services

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 or later
- SQL Server (LocalDB or full instance)

### Running the Application
1. Clone the repository
2. Open `YHABudget.sln` in Visual Studio
3. Restore NuGet packages
4. Build the solution
5. Run the `YHABudget.WPF` project

### Running Tests
```bash
dotnet test
```

## Development

The application follows MVVM architecture with clear separation between:
- **Views**: XAML-based UI
- **ViewModels**: UI logic and state management
- **Models**: Data entities
- **Services**: Business logic and data access

### Code Style
- Swedish language for UI strings
- English for code and comments
- MVVM pattern throughout
- Service layer for data access

## Author

Christoffer Isenberg

## Acknowledgments

Built as a personal budget management solution with focus on salary calculations and absence tracking for Swedish work conditions.
